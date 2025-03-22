using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Entity;
using Contract.Services.Interface;

using Services;
using Services.Service;
using Repositories.Base;
using FHealthSphere.Services.Services;

namespace FHealthSphere
{
    public static class DependencyInjection
    {
        public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigRoute();
            services.AddDatabase(configuration);
            //services.AddIdentity();
            services.AddInfrastructure(configuration);
            services.AddServices();
        }
        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FHealthSphereDBContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        //public static void AddIdentity(this IServiceCollection services)
        //{
        //    services.AddIdentity<Account, ApplicationRole>(options =>
        //    {
        //    })
        //     .AddEntityFrameworkStores<FHealthSphereDBContext>()
        //     .AddDefaultTokenProviders();
        //}
        public static void AddServices(this IServiceCollection services)
        {
            services
                .AddScoped<IUserService, UserService>()
                .AddScoped<TokenService>()
         .AddScoped<IBandBrandService, BandBrandService>()
         .AddScoped<IHealthRecordService, HealthRecordService>()
         .AddScoped<IMetricGroupService, MetricGroupService>()
         .AddScoped<IMetricService, MetricService>()
         .AddScoped<IRecordMetricItemService, RecordMetricItemService>()
        //.AddScoped(IAccountService, AccountService)
        .AddScoped<INotificationService, NotificationService>()
         .AddScoped<IBandService, BandService>();


        }
    }
}
