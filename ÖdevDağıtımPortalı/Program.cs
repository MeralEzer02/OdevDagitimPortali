using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ÖdevDaðýtým.API.Data;
using ÖdevDaðýtým.API.Models;
using ÖdevDaðýtým.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// SQL Server ve DbContext Ayarý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Ayarý
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>();

// Generic Repository Kaydý
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Özel Repository Kayýtlarý
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ÖdevDaðýtým.API.Services.ICourseService, ÖdevDaðýtým.API.Services.CourseService>();
builder.Services.AddScoped<ÖdevDaðýtým.API.Services.IAssignmentService, ÖdevDaðýtým.API.Services.AssignmentService>();
builder.Services.AddScoped<ÖdevDaðýtým.API.Services.ISubmissionService, ÖdevDaðýtým.API.Services.SubmissionService>();
builder.Services.AddScoped<ÖdevDaðýtým.API.Services.INotificationService, ÖdevDaðýtým.API.Services.NotificationService>();

// JWT Ayarlarýný Sýnýfa Baðlama (Options Pattern)
builder.Services.Configure<ÖdevDaðýtým.API.Settings.JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
// JwtService'i Sisteme Kaydetme
builder.Services.AddScoped<ÖdevDaðýtým.API.Services.IJwtService, ÖdevDaðýtým.API.Services.JwtService>();
// JWT Middleware (Güvenlik Görevlisi) Ayarý
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

// AutoMapper Kaydý
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ÖdevDaðýtým.API.Helpers.MappingProfile>();
});
// AuthService
builder.Services.AddScoped<ÖdevDaðýtým.API.Services.IAuthService, ÖdevDaðýtým.API.Services.AuthService>();

builder.Services.AddHttpContextAccessor();
// CurrentUserService Kaydý
builder.Services.AddScoped<ÖdevDaðýtým.API.Services.ICurrentUserService, ÖdevDaðýtým.API.Services.CurrentUserService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Ödev Daðýtým API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Token'ýnýzý buraya girin. Örnek: 'Bearer {token}'"
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
app.UseHttpsRedirection();

// Kimlik Doðrulama
app.UseAuthentication();

// Yetki Kontrolü
app.UseAuthorization();

app.MapControllers();

app.Run();
