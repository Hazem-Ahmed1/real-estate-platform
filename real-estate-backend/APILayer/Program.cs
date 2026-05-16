using APILayer.MiddleWare;
using APILayer.Factories;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Implementation;
using DataAccessLayer.Contracts;
using DataAccessLayer.Data;
using DataAccessLayer.Implementation.Repositories;
using DataAccessLayer.Implementation.Seed;
using DataAccessLayer.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Common;
using BusinessLogicLayer.Helpers;
using APILayer.Options;
using APILayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
            options.InvalidModelStateResponseFactory = ApiResponseFactory.CustomValidationErrorResponse;
        });

        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen(options =>
        {
            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: Bearer <JWT>",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { scheme, Array.Empty<string>() }
            });
        });

        // Data Access & Infrastructure
        builder.Services.AddDbContext<RealEstateDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Identity
        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<RealEstateDbContext>()
            .AddDefaultTokenProviders();

        // JWT
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.AddScoped<JwtTokenService>();

        var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        builder.Services
            .AddAuthentication(options =>
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
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };
            });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<IDataSeeder, DataSeeder>();

        // BLL Services
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IMediaService, MediaService>();
        builder.Services.AddScoped<IBlogService, BlogService>();
        builder.Services.AddScoped<IMessageService, MessageService>();

        builder.Services.AddScoped<IUnitService, UnitService>();
        builder.Services.AddScoped<IBuildingService, BuildingService>();
        builder.Services.AddScoped<ILookupService, LookupService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();


        // Cloudinary Settings
        builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

        // AutoMapper
        builder.Services.AddAutoMapper(cfg => { }, typeof(BusinessLogicLayer.AssemblyReference));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAny", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });


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

        app.UseCors("AllowAny");

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
