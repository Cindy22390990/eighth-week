using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.Text;
using Training.ImageSystem.Background_Worker;
using Training.ImageSystem.Data;
using Training.ImageSystem.Repositories;
using Training.ImageSystem.Repositories.Interfaces;
using Training.ImageSystem.Security;
using Training.ImageSystem.Security.Interfaces;
using Training.ImageSystem.Services;
using Training.ImageSystem.Services.Interfaces;


namespace Training.ImageSystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var logger =
            NLog.LogManager
            .Setup()
            .LoadConfigurationFromFile(
                "NLog.config")
            .GetCurrentClassLogger();


        try
        {

            var builder =
                WebApplication.CreateBuilder(args);



            builder.Logging.ClearProviders();

            builder.Host.UseNLog();


            // Controller
            builder.Services.AddControllers();


        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                "Bearer",
                new Microsoft.OpenApi.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.ParameterLocation.Header,
                    Description =
                        "輸入 JWT Token"
                });


            options.AddSecurityRequirement(
                document =>
                {
                    return new Microsoft.OpenApi.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                                "Bearer",
                                document
                            ),
                            new List<string>()
                        }
                    };
                });
        });


        // Database
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration
                    .GetConnectionString("DefaultConnection"));
        });


        // Repository
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<
            IImageJobRepository,
            ImageJobRepository>();
        builder.Services.AddHostedService<ImageProcessingWorker>();

        // Services
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IImageJobService, ImageJobService>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        builder.Services.AddScoped<PythonImageProcessor>();

        // JWT Authentication
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSettings =
                    builder.Configuration.GetSection("Jwt");


                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,

                        ValidIssuer =
                            jwtSettings["Issuer"],


                        ValidateAudience = true,

                        ValidAudience =
                            jwtSettings["Audience"],


                        ValidateLifetime = true,


                        ValidateIssuerSigningKey = true,


                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtSettings["Key"]!))
                    };
            });


        var app = builder.Build();



        // Database Initialize
        using (var scope = app.Services.CreateScope())
        {
            var db =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();


            var passwordService =
                scope.ServiceProvider
                    .GetRequiredService<IPasswordService>();


            await DbInitializer.InitializeAsync(
                db,
                passwordService);
        }



        // HTTP Pipeline

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        

        app.UseHttpsRedirection();


        // 驗證 JWT
        app.UseAuthentication();


        // 權限判斷
        app.UseAuthorization();



        app.MapControllers();


            app.Run();

        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "Application stopped");

            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }
    }
}
