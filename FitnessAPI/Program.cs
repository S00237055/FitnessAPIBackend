using System.Text.Json.Serialization;

namespace FitnessAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<FitnessAPI.Models.FitnessAppDbContext>();

            // 1. ADD THIS: Configure CORS service
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:8081")  // Allows requests from localhost:8081, mobile, etc.
                          .AllowAnyMethod()  // Allows GET, POST, PUT, DELETE
                          .AllowAnyHeader(); // Allows Custom Headers
                });
            });

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 2. ADD THIS: Enable CORS Middleware
            // (Must be BEFORE UseAuthorization)
            app.UseCors("AllowReactApp");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}