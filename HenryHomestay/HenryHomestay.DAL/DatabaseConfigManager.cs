using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HenryHomestay.DAL;

public class DbConnectionConfig
{
    public string Provider { get; set; } = "Auto"; // SqlServer, Sqlite, Auto
    public string SqlServerConnectionString { get; set; } =
        "Server=.\\SQLEXPRESS;Database=HenryHomestayDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;";
}

public static class DatabaseConfigManager
{
    private static string? _resolvedProvider;
    private static string? _resolvedConnStr;
    private static readonly object _lock = new();

    public static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "HenryHomestay.slnx")) ||
                File.Exists(Path.Combine(dir.FullName, "HenryHomestay.sln")) ||
                Directory.Exists(Path.Combine(dir.FullName, "HenryHomestay.DAL")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return @"D:\Csharp\HenryHomestay";
    }

    public static string GetWorkspaceRoot()
    {
        var slnRoot = GetSolutionRoot();
        var parent = Directory.GetParent(slnRoot);
        if (parent != null && (Directory.Exists(Path.Combine(parent.FullName, "user")) ||
                               Directory.Exists(Path.Combine(parent.FullName, "henry-homestay"))))
        {
            return parent.FullName;
        }
        return @"D:\Csharp";
    }

    public static string ConfigFilePath => Path.Combine(GetSolutionRoot(), "dbconfig.json");
    public static string SharedSqlitePath => Path.Combine(GetSolutionRoot(), "HenryHomestayShared.db");

    public static DbConnectionConfig LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var cfg = JsonSerializer.Deserialize<DbConnectionConfig>(json);
                if (cfg != null) return cfg;
            }
        }
        catch
        {
            // Ignore and use default
        }

        var defaultCfg = new DbConnectionConfig();
        SaveConfig(defaultCfg);
        return defaultCfg;
    }

    public static void SaveConfig(DbConnectionConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
            lock (_lock)
            {
                _resolvedProvider = null;
                _resolvedConnStr = null;
            }
        }
        catch
        {
            // Ignore write errors
        }
    }

    public static bool TrySqlServerConnection(string connectionString, out string error)
    {
        error = string.Empty;
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = 1,
                TrustServerCertificate = true
            };
            var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
            {
                InitialCatalog = "master"
            };
            using var conn = new SqlConnection(masterBuilder.ConnectionString);
            conn.Open();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static (string Provider, string ConnectionString) ResolveActiveConnection()
    {
        lock (_lock)
        {
            if (_resolvedProvider != null && _resolvedConnStr != null)
            {
                return (_resolvedProvider, _resolvedConnStr);
            }

            var cfg = LoadConfig();
            if (string.Equals(cfg.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                _resolvedProvider = "SqlServer";
                _resolvedConnStr = cfg.SqlServerConnectionString;
                return (_resolvedProvider, _resolvedConnStr);
            }

            if (string.Equals(cfg.Provider, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                string[] candidates =
                [
                    cfg.SqlServerConnectionString,
                    "Server=localhost;Database=HenryHomestayDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1;",
                    "Server=.\\SQLEXPRESS;Database=HenryHomestayDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1;",
                    "Server=(localdb)\\MSSQLLocalDB;Database=HenryHomestayDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1;"
                ];

                string? workingSql = null;
                Parallel.ForEach(candidates.Distinct().Where(c => !string.IsNullOrWhiteSpace(c)), (candidate, state) =>
                {
                    if (TrySqlServerConnection(candidate, out _))
                    {
                        workingSql = candidate;
                        state.Stop();
                    }
                });

                if (workingSql != null)
                {
                    _resolvedProvider = "SqlServer";
                    _resolvedConnStr = workingSql;
                    return (_resolvedProvider, _resolvedConnStr);
                }
            }

            _resolvedProvider = "Sqlite";
            _resolvedConnStr = $"Data Source={SharedSqlitePath}";
            return (_resolvedProvider, _resolvedConnStr);
        }
    }

    public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        var (provider, connStr) = ResolveActiveConnection();
        if (provider == "SqlServer")
        {
            optionsBuilder.UseSqlServer(connStr);
        }
        else
        {
            optionsBuilder.UseSqlite(connStr);
        }
    }

    public static HenryDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<HenryDbContext>();
        ConfigureDbContext(builder);
        return new HenryDbContext(builder.Options);
    }

    public static string GetActiveDatabaseSummary()
    {
        var (provider, connStr) = ResolveActiveConnection();
        if (provider == "SqlServer")
        {
            var b = new SqlConnectionStringBuilder(connStr);
            return $"SQL Server ({b.DataSource} / {b.InitialCatalog})";
        }
        return $"Shared Local DB (HenryHomestayShared.db — đồng bộ Web & Desktop)";
    }
}
