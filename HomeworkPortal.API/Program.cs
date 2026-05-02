using HomeworkPortal.API.Data;
using HomeworkPortal.API.Middlewares;
using HomeworkPortal.API.Models;
using HomeworkPortal.API.Repositories;
using HomeworkPortal.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .Enrich.FromLogContext()
        .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
});

// Add services to the container.
builder.Services.AddControllers();

// CORS AYARI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyUI", policy =>
    {
        policy.WithOrigins("https://localhost:7205", "https://localhost:7141")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// SQL Server ve DbContext Ayarı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Ayarı
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Generic Repository Kaydı
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Özel Repository Kayıtları
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<HomeworkPortal.API.Services.ICourseService, HomeworkPortal.API.Services.CourseService>();
builder.Services.AddScoped<HomeworkPortal.API.Services.IAssignmentService, HomeworkPortal.API.Services.AssignmentService>();
builder.Services.AddScoped<HomeworkPortal.API.Services.ISubmissionService, HomeworkPortal.API.Services.SubmissionService>();
builder.Services.AddScoped<HomeworkPortal.API.Services.INotificationService, HomeworkPortal.API.Services.NotificationService>();

// JWT Ayarlarını Sınıfa Bağlama (Options Pattern)
builder.Services.Configure<HomeworkPortal.API.Settings.JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// JwtService'i Sisteme Kaydetme
builder.Services.AddScoped<HomeworkPortal.API.Services.IJwtService, HomeworkPortal.API.Services.JwtService>();

// JWT Middleware (Güvenlik Görevlisi) Ayarı
builder.Services.AddAuthentication(options =>
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

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? ""))
    };
});

// AutoMapper Kaydı
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<HomeworkPortal.API.Helpers.MappingProfile>();
});

// AuthService
builder.Services.AddScoped<HomeworkPortal.API.Services.IAuthService, HomeworkPortal.API.Services.AuthService>();

builder.Services.AddHttpContextAccessor();

// CurrentUserService Kaydı
builder.Services.AddScoped<HomeworkPortal.API.Services.ICurrentUserService, HomeworkPortal.API.Services.CurrentUserService>();

builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddEndpointsApiExplorer();

// Health Check Servisleri
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DB Connection string bulunamadı."),
        name: "SQL Server DB Check",
        tags: new[] { "ready" }
    );

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "HomeworkPortal API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Token'ınızı buraya girin. Örnek: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Correlation ID Bekçisi
app.UseMiddleware<HomeworkPortal.API.Middlewares.CorrelationIdMiddleware>();

// Hata Yakalayıcı Middleware
app.UseMiddleware<HomeworkPortal.API.Middlewares.GlobalExceptionMiddleware>();

// Health Check Endpoint'leri
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.UseHttpsRedirection();

app.UseStaticFiles();

// Güvenlik Başlıkları
app.UseMiddleware<HomeworkPortal.API.Middlewares.SecurityHeadersMiddleware>();

// CORS MİDDLEWARE
app.UseRouting();

app.UseCors("AllowMyUI");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseHttpMetrics();

app.MapControllers().RequireRateLimiting("api");

app.MapMetrics();

app.Run();