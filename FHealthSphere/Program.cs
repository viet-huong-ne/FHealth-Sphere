

using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using FHealthSphere;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Base;
using Repositories.UOW;
using System;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Cấu hình database
builder.Services.AddDbContext<FHealthSphereDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"), b =>
        b.MigrationsAssembly("Repositories"));
});

// Đăng ký Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Thêm Identity(Chỉ cần gọi 1 lần)
builder.Services.AddIdentity<Account, ApplicationRole>()
    .AddEntityFrameworkStores<FHealthSphereDBContext>()
    .AddDefaultTokenProviders();

// Cấu hình Authentication (Không cần AddCookie vì Identity đã xử lý)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

})
    //.AddCookie()
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse(); // Chặn phản hồi mặc định của ASP.NET
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    error = "Unauthorized",
                    error_description = "You need to provide a valid token to access this resource."
                }));
            }
        };
    })

    .AddGoogle(options =>
    {
        options.ClientId = "295397235364-20u43d17lluu93lo4a4kfuft2f62np07.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });


builder.Services.AddAuthorization();

// Thêm DI khác
builder.Services.AddConfig(builder.Configuration);

// Thêm Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//Configure Swagger with JWT Bearer Support
builder.Services.AddSwaggerGen(c =>
{
    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer prefix in the field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    //c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    //{
    //    Type = SecuritySchemeType.OAuth2,
    //    Flows = new OpenApiOAuthFlows
    //    {
    //        AuthorizationCode = new OpenApiOAuthFlow
    //        {
    //            AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/auth"),
    //            TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
    //            Scopes = new Dictionary<string, string>
    //            {
    //                { "openid", "OpenID Connect" },
    //                { "profile", "Access Profile" },
    //                { "email", "Access Email" }
    //            }
    //        }
    //    }
    //});

    //c.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "OAuth2"
    //            }
    //        },
    //        new List<string>()
    //    }
    //});
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
        c.RoutePrefix = "swagger";
    });
}
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());
app.UseHttpsRedirection();
app.UseAuthentication();  // 🔥 Cần đặt trước Authorization
app.UseAuthorization();

app.MapControllers();
app.Run();
