

using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.UOW;
using XuongMayBE.API;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FHealthSphereDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"), b =>
        b.MigrationsAssembly("FHealthSphere"));
});
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

