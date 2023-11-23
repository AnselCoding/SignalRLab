using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SignalRLab.Hubs;
using SignalRLab.Middleware;
using SignalRLab.SignalRTimer;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews(options =>
{
    // Apply ExceptionFilter globally.
    options.Filters.Add(typeof(CommonExceptionFilter));
});

// Add SignalR 使用者識別Id
builder.Services.AddSingleton<IUserIdProvider, MyUserIdProvider>();
// Add DisconnectTimer service 註冊 為 Scoped 服務
builder.Services.AddScoped<IDisconnectTimer,DisconnectTimer>();
// 讓自訂元件存取 HttpContext
builder.Services.AddHttpContextAccessor();

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("controllers", new OpenApiInfo { Title = "SignalR Lab", Version = "v1", });
    options.SwaggerDoc("hubs", new OpenApiInfo { Title = "SignalR Lab", Version = "v2", Description = "SignalR API 都有 JWT 驗證，只是此 SwaggerUI 沒有顯示鎖頭圖示。" });

    // 設置指定 XML 路徑，啟用API註解
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Swagger加入驗證
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        //選擇類型，type選擇http時，透過swagger畫面做認證時可以省略Bearer前綴詞(如下圖)
        Type = SecuritySchemeType.Http,
        //採用Bearer token
        Scheme = "Bearer",
        //bearer格式使用jwt
        BearerFormat = "JWT",
        //認證放在http request的header上
        In = ParameterLocation.Header,
        //描述
        Description = "JWT驗證"
    });
    //製作額外的過濾器，過濾Authorize、AllowAnonymous，甚至是沒有打attribute
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();

    // 加入SignalRSwagger，能增加 SignalR 事件 API 顯示，但不能在 Swagger 進行操作
    options.AddSignalRSwaggerGen(ssgOptions =>
    {
        ssgOptions.HubPathFunc = name => name switch
        {
            nameof(FirstSignalRHub) => "/HubPathFirst",
            nameof(AllHub) => "/HubPathAll",
            nameof(GroupHub) => "/HubPathGroup",
            nameof(PersonHub) => "/HubPathPerson",
            _ => string.Empty,
        };
    });
});

var config = builder.Configuration;
// Add Jwt Authentication
// Use Multiple AddAuthentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Path1Scheme", options =>
    {
        options.SaveToken = true;
        // Issuer and Audience shall be true for production, also GetPrincipalFromExpiredToken() and IsRefreshTokenValidate(). Put Issuer and Audience into SecurityTokenDescriptor at GenerateToken().
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidIssuer = config["JWT:Issuer"],
            //ValidAudience = config["JWT:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JWT:Key"]))
        };

        // SignalR add Authorization
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR 會將 Token 以參數名稱 access_token 的方式放在 URL 查詢參數裡
                // Read the token out of the query string
                var accessToken = context.Request.Query["access_token"];

                // 連線網址為 Hubs 相關路徑才檢查
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (
                    path.StartsWithSegments("/HubPathFirst") ||
                    path.StartsWithSegments("/HubPathAll") ||
                    path.StartsWithSegments("/HubPathGroup") ||
                    path.StartsWithSegments("/HubPathPerson")
                ))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddJwtBearer("Path2Scheme", options =>
    {
        options.SaveToken = true;
        // Issuer and Audience shall be true for production, also GetPrincipalFromExpiredToken() and IsRefreshTokenValidate(). Put Issuer and Audience into SecurityTokenDescriptor at GenerateToken().
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JWT:Key"]))
        };

        // SignalR add Authorization
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR 會將 Token 以參數名稱 access_token 的方式放在 URL 查詢參數裡
                // Read the token out of the query string
                var accessToken = context.Request.Query["access_token"];

                // 連線網址為 Hubs 相關路徑才檢查
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && ( 
                    path.StartsWithSegments("/HubPathFirst") ||
                    path.StartsWithSegments("/HubPathAll") ||
                    path.StartsWithSegments("/HubPathGroup") ||
                    path.StartsWithSegments("/HubPathPerson")
                ))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Use Multiple AddAuthentication and Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PolicyForPath1", policy =>
    {
        policy.AuthenticationSchemes.Add("Path1Scheme");
        policy.RequireAuthenticatedUser(); // 使用者必須已驗證
    });
    options.AddPolicy("PolicyForPath2", policy =>
    {
        policy.AuthenticationSchemes.Add("Path2Scheme");
        policy.RequireAuthenticatedUser(); // 使用者必須已驗證
    });
});

// Add SignalR service
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/controllers/swagger.json", "RESTful");
        options.SwaggerEndpoint("/swagger/hubs/swagger.json", "SignalR");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use multiple SignalR hub
app.MapHub<FirstSignalRHub>("/HubPathFirst", options =>
{
    // 驗證權杖到期時關閉連線
    options.CloseOnAuthenticationExpiration = true;
});

app.MapHub<AllHub>("/HubPathAll", options =>
{
    // 驗證權杖到期時關閉連線
    options.CloseOnAuthenticationExpiration = true;
});
app.MapHub<GroupHub>("/HubPathGroup", options =>
{
    // 驗證權杖到期時關閉連線
    options.CloseOnAuthenticationExpiration = true;
});
app.MapHub<PersonHub>("/HubPathPerson", options =>
{
    // 驗證權杖到期時關閉連線
    options.CloseOnAuthenticationExpiration = true;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
