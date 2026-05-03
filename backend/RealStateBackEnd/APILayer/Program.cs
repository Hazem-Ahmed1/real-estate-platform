using APILayer.MiddleWare;
using APILayer.Factories;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Implementation;
using DataAccessLayer.Contracts;
using DataAccessLayer.Data;
using DataAccessLayer.Implementation.Repositories;
using DataAccessLayer.Implementation.Seed;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Common;
using BusinessLogicLayer.Helpers;

namespace APILayer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- 1. Add services to the container ---
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        // [IMPORTANT] Custom Validation Error Response Factory
        builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = ApiResponseFactory.CustumValidationErrorResponse;
        });

        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen();

        // Data Access & Infrastructure
        builder.Services.AddDbContext<RealEstateDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<IDataSeeder, DataSeeder>();

        // BLL Services
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IMediaService, MediaService>();

        // Cloudinary Settings
        builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

        // AutoMapper
        builder.Services.AddAutoMapper(cfg => { }, typeof(BusinessLogicLayer.AssemblyReference));


        var app = builder.Build();

        // --- 2. Configure the HTTP request pipeline ---

        // [IMPORTANT] Global Exception Handling Middleware
        app.UseMiddleware<GlobalExpectionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Seed Database Automatically
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<RealEstateDbContext>();
            await context.Database.MigrateAsync();
            var seeder = services.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
