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

// Add SignalR �ϥΪ��ѧOId
builder.Services.AddSingleton<IUserIdProvider, MyUserIdProvider>();
// Add DisconnectTimer service ���U �� Scoped �A��
builder.Services.AddScoped<IDisconnectTimer,DisconnectTimer>();
// ���ۭq����s�� HttpContext
builder.Services.AddHttpContextAccessor();

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("controllers", new OpenApiInfo { Title = "SignalR Lab", Version = "v1", });
    options.SwaggerDoc("hubs", new OpenApiInfo { Title = "SignalR Lab", Version = "v2", Description = "SignalR API ���� JWT ���ҡA�u�O�� SwaggerUI �S��������Y�ϥܡC" });

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
        Description = "JWT����"
    });
    //�s�@�B�~���L�o���A�L�oAuthorize�BAllowAnonymous�A�ƦܬO�S����attribute
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();

    // �[�JSignalRSwagger�A��W�[ SignalR �ƥ� API ��ܡA������b Swagger �i��ާ@
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
                // SignalR �|�N Token �H�ѼƦW�� access_token ���覡��b URL �d�߰ѼƸ�
                // Read the token out of the query string
                var accessToken = context.Request.Query["access_token"];

                // �s�u���}�� Hubs �������|�~�ˬd
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
                // SignalR �|�N Token �H�ѼƦW�� access_token ���覡��b URL �d�߰ѼƸ�
                // Read the token out of the query string
                var accessToken = context.Request.Query["access_token"];

                // �s�u���}�� Hubs �������|�~�ˬd
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
        policy.RequireAuthenticatedUser(); // �ϥΪ̥����w����
    });
    options.AddPolicy("PolicyForPath2", policy =>
    {
        policy.AuthenticationSchemes.Add("Path2Scheme");
        policy.RequireAuthenticatedUser(); // �ϥΪ̥����w����
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
    // �����v������������s�u
    options.CloseOnAuthenticationExpiration = true;
});

app.MapHub<AllHub>("/HubPathAll", options =>
{
    // �����v������������s�u
    options.CloseOnAuthenticationExpiration = true;
});
app.MapHub<GroupHub>("/HubPathGroup", options =>
{
    // �����v������������s�u
    options.CloseOnAuthenticationExpiration = true;
});
app.MapHub<PersonHub>("/HubPathPerson", options =>
{
    // �����v������������s�u
    options.CloseOnAuthenticationExpiration = true;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
