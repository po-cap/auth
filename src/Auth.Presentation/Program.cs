using System.Net;
using Auth.Application;
using Auth.Infrastructure;
using Auth.Presentation.Endpoints;
using Auth.Presentation.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Po.Api.Response;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var dir    = Environment.GetEnvironmentVariable("ASPNETCORE_DIRECTORY");
var env    = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
             ?? throw new Exception("Set \"ASPNETCORE_ENVIRONMENT\"");;

builder.Configuration
    .SetBasePath(dir ?? Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


builder.Services.AddHttpContextAccessor();        // 加入 HttpContextAccessor 服務
builder.Services.AddControllersWithViews();       // 添加  MVC 服務 (支持 Controller + View)
builder.Services.AddOpenApi();                    // Open Document 文件
builder.Services
       .AddApplication()                          // 加入應用層
       .AddInfrastructure(builder.Configuration); // 加入建設層
// 認證
builder.Services.AddAuthentication()                       
       .State()                                   // Authorize Endpoint
       .Line(builder.Configuration)               // Line
       .Jwt();                                    // Json Web Token
// 授權
builder.Services.AddAuthorization(o => {
    o.Jwt();                                      // Json Web Token
    o.State();                                    // Authorize Endpoint
});



var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |  // 处理客户端真实 IP（对应 Nginx 的 X-Forwarded-For）
                           ForwardedHeaders.XForwardedHost | // 处理原始 Host 头 (对应 Nginx 的 Host)
                           ForwardedHeaders.XForwardedProto, // 处理原始协议(http/https)(对应 Nginx 的 X-Forwarded-Proto)
    
        // 以下两個參數，擇一使用就好
        // KnownNetworks，設定代理端（Nginx）必須在哪個子網域內
        // KnownProxies，設定代理端（Nginx）必須在哪個 IP 機器上
        //KnownNetworks =
        //{
        //    new IPNetwork(IPAddress.Parse("192.168.50.0"), 24)
        //},
        KnownProxies = { IPAddress.Parse("127.0.0.1") }
    });    
    //app.UsePathBase("/oauth");  // 表示 Host 還要再加上的路徑，例如這裡會變成 t8.supojen.com/aouth
    
    app.UseStaticFiles();      // 使用靜態資源(wwwroot directory 裡的資源)
    app.UseRouting();          // 路由匹配(Mini API 的 Map)
    app.UseAuthentication();   // 確認身份
    app.UseAuthorization();    // 確認權限
    app.UseExceptionHandle();  // 處理 Error 發生時的 Response
    
    // 主要是回傳身份確認用的頁面，like login page etc.
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}");
    
    
    app.MapWellKnownRoutes();
    app.MapOAuth();
    //app.MapLogin();
    app.MapLoginEndpoint();
    app.MapLineOAuth();

    app.Run();
}
