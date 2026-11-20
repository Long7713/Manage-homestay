@echo off
chcp 65001 >nul
echo ====================================================================
echo   KHOI DONG WEB NGUOI DUNG - HENRY HOMESTAY (ASP.NET CORE MVC)
echo   Dia chi truy cap: http://localhost:5188
echo ====================================================================
cd /d "%~dp0HenryHomestay.Web"
start "" "http://localhost:5188"
dotnet run --project "%~dp0HenryHomestay.Web\HenryHomestay.Web.csproj" --urls="http://localhost:5188"
pause
