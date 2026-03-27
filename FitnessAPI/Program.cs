using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessAPI.Models;
using Microsoft.EntityFrameworkCore;

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
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FitnessAppDbContext>();

                if (!context.Exercises.Any())
                {
                    try
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
                            string myApiUrl = "http://localhost:5226";

                            var exercises = importedExercises.Select(e => new Exercise
                            {
                                Name = e.name,
                                BodyPart = e.body_part,
                                Target = e.target,
                                Equipment = e.equipment,


                                GifUrl = $"{myApiUrl}/{e.gif_url.Replace("videos", "gifs")}",

                                Instructions = e.instructions != null && e.instructions.en != null? e.instructions.en : "No instructions provided."

                            }).ToList();

                            context.Exercises.AddRange(exercises);
                            context.SaveChanges();
                            Console.WriteLine("Exercise seeded successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error seeding exercises: {ex.Message}");
                    }
                }




                app.UseCors("AllowReactApp");

                app.UseHttpsRedirection();

                app.UseStaticFiles();
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
        }
    }
}