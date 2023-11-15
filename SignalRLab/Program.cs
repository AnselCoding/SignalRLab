using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SignalRLab.Hubs;
using SignalRLab.Middleware;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews(options =>
{
    // Apply ExceptionFilter globally.
    options.Filters.Add(typeof(CommonExceptionFilter));
});

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
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
        Description = "JWT驗證描述"
    });
    //製作額外的過濾器，過濾Authorize、AllowAnonymous，甚至是沒有打attribute
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

var config = builder.Configuration;
// Add Jwt Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<FirstSignalRHub>("/yourHubPath");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
