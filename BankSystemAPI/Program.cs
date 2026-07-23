using BankSystemAPI.BackgroundWorkers;
using DataAccessLayer.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BankSystemAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
            builder.Services.AddDbContext<BankDbContext>(options =>
            {
                options.UseSqlServer(connectionString);

                // options.UseLazyLoadingProxies();
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
            {
                // Настройки безопасности паролей
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Защита от перебора (Lockout)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5; // Блокировка на 15 мин после 5 ошибок
                options.Lockout.AllowedForNewUsers = true;

                // Уникальность учетных записей
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<BankDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
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
            });

            // 4. Политики авторизации
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
                options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
                //options.AddPolicy("CanManageOperations", policy => policy.RequireRole("Employee", "Admin"));
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddHostedService<CreditPaymentBackgroundWorker>();
            builder.Services.AddHostedService<DepositPaymentBackgroundService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();    
            app.UseAuthorization();
            

            app.MapControllers();

            app.Run();
        }
    }
}
