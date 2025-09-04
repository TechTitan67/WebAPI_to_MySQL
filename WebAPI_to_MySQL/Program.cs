using Microsoft.AspNetCore.Authentication.JwtBearer;    // Needed for JWT Authentication
using Microsoft.AspNetCore.Identity;                    // Needed for Identity support
using Microsoft.AspNetCore.RateLimiting;                // Needed for rate limiting
using Microsoft.EntityFrameworkCore;                    // Needed for Entity Framework Core
using Microsoft.IdentityModel.Tokens;                   // Needed for JWT Token validation
using Microsoft.OpenApi.Models;                         // Needed for Swagger/OpenAPI documentation
using System.Text;                                      // Needed for encoding the JWT signing key
using System.Threading.RateLimiting;                    // Needed for rate limiting policies
using WebAPI_to_MySQL.Entities;                         // Needed for rate limiting policies

namespace WebAPI_to_MySQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Rate Limiting - limiting login attempts to 5 per minute
            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("LoginPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 requests
                            Window = TimeSpan.FromMinutes(1), // per minute
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });

            // Configure JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://dev-gmgpbwcjsvlum352.us.auth0.com/";
                    options.Audience = "https://eeprojectapi"; // <-- Use your custom API identifier
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });
            builder.Services.AddScoped<IUserStatusService, UserStatusService>();
            builder.Services.AddAuthorization();        // Needed for authorization policies

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI_to_MySQL", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid JWT with Bearer prefix (Bearer {token})",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
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
            });

            builder.Services.AddDbContext<NeurotechnexusContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ));

            // Configure CORS to allow requests from the Blazor application
            var blazorOrigin = "https://localhost:7095";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("BlazorPolicy", policy =>
                {
                    policy.WithOrigins(blazorOrigin)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            //Temporarily Commented out Identity configuration
            // -----------------------------------------------------
            //builder.Services.AddScoped<IUserStatusService, UserStatusService>();

            // Register the payment Identity services in Dependency Injection container
            if (builder.Configuration.GetValue<bool>("UseMockPayments"))
                builder.Services.AddScoped<IPaymentService, MockPaymentService>();
            else
            {
                builder.Services.AddHttpClient<PayPalPaymentService>();
                builder.Services.AddScoped<IPaymentService, PayPalPaymentService>();
            }

            // Removed duplicate AddCors block for clarity

            var app = builder.Build();                  // Build the application

            app.UseRateLimiter();                       // Enable rate limiting middleware
            app.UseCors("BlazorPolicy");                // Enable CORS middleware
            app.UseAuthentication();                    // Enable authentication middleware
            app.UseAuthorization();                     // Enable authorization middleware

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapControllers().RequireRateLimiting("LoginPolicy"); // Apply rate limiting policy to all controllers

            app.Run();
        }
    }
}