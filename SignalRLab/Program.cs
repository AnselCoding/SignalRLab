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
    // �]�m���w XML ���|�A�ҥ�API����
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Swagger�[�J����
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        //��������Atype���http�ɡA�z�Lswagger�e�����{�Үɥi�H�ٲ�Bearer�e���(�p�U��)
        Type = SecuritySchemeType.Http,
        //�ĥ�Bearer token
        Scheme = "Bearer",
        //bearer�榡�ϥ�jwt
        BearerFormat = "JWT",
        //�{�ҩ�bhttp request��header�W
        In = ParameterLocation.Header,
        //�y�z
        Description = "JWT���Ҵy�z"
    });
    //�s�@�B�~���L�o���A�L�oAuthorize�BAllowAnonymous�A�ƦܬO�S����attribute
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
