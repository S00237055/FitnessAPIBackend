using System.Text.Json.Serialization;

namespace FitnessAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<FitnessAPI.Models.FitnessAppDbContext>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:8081") 
                          .AllowAnyMethod()  
                          .AllowAnyHeader(); 
                });
            });

            
            builder.Services.AddControllers()
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            app.UseCors("AllowReactApp");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}