using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StayEase.Admin.Models;

namespace StayEase.Admin.Services;

public sealed class AdminClient : IDisposable
{
    private readonly HttpClient _http=new(){Timeout=TimeSpan.FromSeconds(18)};
    private static readonly JsonSerializerOptions JsonOpts=new(JsonSerializerDefaults.Web);
    public string BaseUrl {get;private set;}="http://localhost:5188";
    public string Name {get;private set;}="Quản trị viên";

    public void Configure(string baseUrl)
    {
        if(!Uri.TryCreate(baseUrl,UriKind.Absolute,out var uri) || (uri.Scheme!="https" && uri.Scheme!="http"))
            throw new ArgumentException("Địa chỉ API phải bắt đầu http:// hoặc https://");
        if(uri.Scheme=="http" && uri.Host is not ("localhost" or "127.0.0.1"))
            throw new ArgumentException("API trên máy chủ khác phải sử dụng HTTPS.");
        BaseUrl=baseUrl.TrimEnd('/');
        _http.BaseAddress=new Uri(BaseUrl+"/");
    }

    public async Task LoginAsync(string email,string password)
    {
        using var result=await _http.PostAsJsonAsync("api/admin/auth/login",new {email,password});
        await VerifyAsync(result);
        var auth=await result.Content.ReadFromJsonAsync<AuthResult>(JsonOpts) ?? throw new Exception("API không trả kết quả đăng nhập.");
        _http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",auth.Token);
        Name=auth.Name;
    }
    public async Task<T> GetAsync<T>(string path)
    {
        using var response=await _http.GetAsync(path);
        await VerifyAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOpts) ?? throw new Exception("Máy chủ trả dữ liệu rỗng.");
    }
    public async Task PostAsync(string path,object data)
    {
        using var res=await _http.PostAsJsonAsync(path,data);
        await VerifyAsync(res);
    }
    public async Task PutAsync(string path,object data)
    {
        using var res=await _http.PutAsJsonAsync(path,data);
        await VerifyAsync(res);
    }
    public async Task PatchAsync(string path,object data)
    {
        using var res=await _http.PatchAsJsonAsync(path,data);
        await VerifyAsync(res);
    }
    public async Task DeleteAsync(string path)
    {
        using var res=await _http.DeleteAsync(path);
        await VerifyAsync(res);
    }
    private static async Task VerifyAsync(HttpResponseMessage response)
    {
        if(response.IsSuccessStatusCode)return;
        var details=await response.Content.ReadAsStringAsync();
        if(details.Length>220)details=details[..220];
        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {details}");
    }
    public void Dispose()=>_http.Dispose();
}
