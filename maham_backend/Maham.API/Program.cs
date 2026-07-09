using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Maham.API.Middleware;
using Maham.Application.Mappings;
using Maham.Application.Services.Implementations;
using Maham.Application.Services.Interfaces;
using Maham.Application.Validators;
using Maham.Domain.Interfaces;
using Maham.Infrastructure.Data;
using Maham.Infrastructure.Hubs;
using Maham.Infrastructure.Repositories;
using Maham.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 1. Database — PostgreSQL via Npgsql
// ──────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ──────────────────────────────────────────────
// 2. Repository / Unit Of Work
// ──────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ──────────────────────────────────────────────
// 3. Infrastructure Services
// ──────────────────────────────────────────────
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();

// ──────────────────────────────────────────────
// 4. Application Services
// ──────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IColumnService, ColumnService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ──────────────────────────────────────────────
// 5. AutoMapper
// ──────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ──────────────────────────────────────────────
// 6. FluentValidation
// ──────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// ──────────────────────────────────────────────
// 7. JWT Authentication
// ──────────────────────────────────────────────
var jwtSecret  = builder.Configuration["Jwt:Secret"]   ?? "MahamSuperSecretJwtKey2026_At_Least32Chars!";
var jwtIssuer  = builder.Configuration["Jwt:Issuer"]   ?? "MahamAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MahamClient";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(key)
        };

        // Allow SignalR to pass token via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────
// 8. SignalR
// ──────────────────────────────────────────────
builder.Services.AddSignalR();

// ──────────────────────────────────────────────
// 9. CORS — Allow Flutter App & Web MVC
// ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("MahamCors", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ──────────────────────────────────────────────
// 10. Controllers + JSON options
// ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ──────────────────────────────────────────────
// 11. Swagger with JWT support
// ──────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Maham API — مهام",
        Version = "v1",
        Description = "Collaborative Kanban Project Management API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ──────────────────────────────────────────────
// Build
// ──────────────────────────────────────────────
var app = builder.Build();

// Seed Initial Data Automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(dbContext);
}

// ──────────────────────────────────────────────
// Middleware Pipeline (ORDER MATTERS)
// ──────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maham API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();

app.UseCors("MahamCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BoardHub>("/hubs/board");

app.Run();
