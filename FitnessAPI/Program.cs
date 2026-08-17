using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessAPI.Middleware;
using FitnessAPI.Models;
using FitnessAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FitnessAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<FitnessAPI.Models.FitnessAppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSection["Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key is not configured. Add a Jwt section to appsettings.json or set it in the hosting environment.");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "FitnessAPI", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the token returned by /api/User/login. Do not include the word 'Bearer'."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            app.UseExceptionHandler();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowReactApp");

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FitnessAppDbContext>();
                context.Database.Migrate();

                try
                {
                    if (!context.Exercises.Any())
                    {
                        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "exercises.json");
                        var jsonData = File.ReadAllText(jsonPath);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };

                        var importedExercises = JsonSerializer.Deserialize<List<ExerciseDb>>(jsonData, options);

                        if (importedExercises != null)
                        {
                            string myApiUrl = "https://my-fitness-api-123-f5gcbyb0bzaggwdm.italynorth-01.azurewebsites.net";

                            var exercises = importedExercises.Select(e => new Exercise
                            {
                                Name = e.name,
                                BodyPart = e.body_part,
                                Target = e.target,
                                Equipment = e.equipment,
                                GifUrl = $"{myApiUrl}/{e.gif_url.Replace("videos", "gifs")}",
                                Instructions = e.instructions != null && e.instructions.en != null
                                    ? e.instructions.en
                                    : "No instructions provided."
                            }).ToList();

                            context.Exercises.AddRange(exercises);
                            context.SaveChanges();
                            Console.WriteLine("Exercise seeded successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding exercises: {ex.Message}");
                }
            }

            app.Run();
        }
    }
}
