
using FluentValidation;
using FluentValidation.AspNetCore;
using InventoryManagementSystemAPI.Configurations;
using InventoryManagementSystemAPI.Data;
using InventoryManagementSystemAPI.Data.Seed;
using InventoryManagementSystemAPI.Middleware;
using InventoryManagementSystemAPI.Models.Identity;
using InventoryManagementSystemAPI.Repositories.Implementations;
using InventoryManagementSystemAPI.Repositories.Interfaces;
using InventoryManagementSystemAPI.Services.Implementations;
using InventoryManagementSystemAPI.Services.Interfaces;
using InventoryManagementSystemAPI.Validators.Category;
using InventoryManagementSystemAPI.Validators.Product;
using InventoryManagementSystemAPI.Validators.PurchaseOrder;
using InventoryManagementSystemAPI.Validators.PurchaseOrderItem;
using InventoryManagementSystemAPI.Validators.Stock;
using InventoryManagementSystemAPI.Validators.Supplier;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Microsoft.OpenApi.Models;

namespace InventoryManagementSystemAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options => 
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;


                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtKey = builder.Configuration["JWT:Key"]
                             ?? Environment.GetEnvironmentVariable("JWT__Key");

                if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        jwtKey = "DevelopmentFallbackKeyForTestingOnlyMustBeAtLeast256BitsLong!";
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "JWT:Key is not configured or is shorter than 256 bits (32 bytes). Set the JWT__Key environment variable.");
                    }
                }

                var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "InventoryManagementSystemAPI";
                var jwtAudience = builder.Configuration["JWT:Audience"] ?? "InventoryManagementSystemAPI";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection("JWT"));

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                    .Select(e => e.ErrorMessage)
                                    .ToArray()
                        );

                    var response = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Validation failed.",
                        Errors = errors
                    };

                    return new BadRequestObjectResult(response);
                };
            });

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            builder.Services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();
            builder.Services.AddScoped<IStockRepository, StockRepository>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            builder.Services.AddScoped<IPurchaseOrderItemService, PurchaseOrderItemService>();
            builder.Services.AddScoped<IStockService, StockService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddHttpClient<ILlmClient, OpenAiLlmClient>();

            builder.Services.AddScoped<IdentityDataSeeder>();
            builder.Services.AddScoped<AppDataSeeder>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<AdjustStockDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateCategoryDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreatePurchaseOrderDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdatePurchaseOrderDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreatePurchaseOrderItemDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdatePurchaseOrderItemDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateSupplierDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateSupplierDtoValidator>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Inventory Management System API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token.\n\nExample:\nBearer eyJhbGciOiJIUzI1NiIs..."
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy
                        .WithOrigins("https://127.0.0.1:7011", "https://localhost:7011")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Seeders 
            try
            {
                using (var scope = app.Services.CreateScope()) 
                {
                    var services = scope.ServiceProvider;

                    var IdentityDataSeeder = services.GetRequiredService<IdentityDataSeeder>();
                    var appDataSeeder = services.GetRequiredService<AppDataSeeder>();

                    await IdentityDataSeeder.SeedAsync();
                    await appDataSeeder.SeedAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetService<ILogger<Program>>();
                logger?.LogWarning(ex, "Database seeding could not be executed on startup. Continuing application startup.");
            }

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/stock"));
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

            app.MapControllers();

            app.Run();
        }
    }
}
