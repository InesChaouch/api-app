using API.Data;
using API.Intefaces;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)//why do we use this in here ?
        {  

            services.AddControllers();
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConection"));
            });
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>(); //builder.Services.AddScoped<TokenService>(); this would work just fine but the other one is a common way 
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}