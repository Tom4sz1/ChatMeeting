﻿using ChatMeeting.Core.Domain;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Interfaces.Services;
using ChatMeeting.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ChatMeeting.Core.Application.Services;
using ChatMeeting.Core.Domain.Options;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ChatMeeting.API.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration) 
        {
            AddCustomAuthentication(services, configuration);
            var connectionString = configuration.GetValue<string>("ConnectionStrings");
            services.AddDbContext<ChatDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }

        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettingsOption>(options => configuration.GetSection(nameof(JwtSettingsOption)).Bind(options));
            return services;
        }

        private static void AddCustomAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(nameof(JwtSettingsOption)).Get<JwtSettingsOption>();
            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey)) throw new ArgumentException("Secret Key is empty");
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = GetTokenValidationParams(key);

            });
        }

        private static TokenValidationParameters GetTokenValidationParams(byte[] key)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(120)
            };
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IChatRepository, ChatRepository>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IJwtService, JwtService>();
            return services;
        }
    }
}
