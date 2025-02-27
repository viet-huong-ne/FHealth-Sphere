

using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.UOW;
using Services.Service;
using System.Text.Json;
using System.Text.Json.Serialization;
using XuongMayBE.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.MaxDepth = 64; 
});
builder.Services.AddDbContext<FHealthSphereDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"), b =>
        b.MigrationsAssembly("FHealthSphere"));
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddLogging();
//builder.Services.AddIdentity<Account, ApplicationRole>()
//    .AddEntityFrameworkStores<FHealthSphereDBContext>()
//    .AddDefaultTokenProviders();
// Add services to the container.
//
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//add dependencyInjection
builder.Services.AddConfig(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();

