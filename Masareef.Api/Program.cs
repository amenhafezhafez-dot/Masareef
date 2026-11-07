using Masareef.BusinessLayer.Services;
using Masareef.DAL.Context;
using Masareef.DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;          // ← جديد
using Microsoft.AspNetCore.Authorization;                      // ← جديد
using Microsoft.AspNetCore.RateLimiting;                       // ← جديد
using System.Threading.RateLimiting;                           // ← جديد
using Masareef.Api.Authorization;                              // ← جديد
using Microsoft.IdentityModel.Tokens;                          // ← جديد
using Serilog;                                                 // ← جديد
using System.Text;                                             // ← جديد

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();   // ← بيستبدل الـ logging الافتراضي بالكامل

// ─────────── DbContext ───────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────── DAL ───────────
builder.Services.AddScoped<IDebtRepository, DebtRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();           // ← جديد
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();   // ← جديد (refresh token)
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();           // ← جديد (auditing)
builder.Services.AddScoped<IProductRepository, ProductRepository>();



// ─────────── BLL ───────────
builder.Services.AddScoped<DebtService>();
builder.Services.AddScoped<HomeService>();
builder.Services.AddScoped<BusinessService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AuthService>();                              // ← جديد
builder.Services.AddScoped<TokenService>();                             // ← جديد
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AuditService>();            // ← جديد (auditing)

// الـ AuditService محتاجه عشان يقرا الـ IP بتاع الطلب
builder.Services.AddHttpContextAccessor();

// ─────────── JWT Authentication ───────────                          // ← كل البلوك ده جديد
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

// ─────────── CORS ───────────
builder.Services.AddCors(o => o.AddPolicy("spa", p => p
    .WithOrigins(builder.Configuration["Cors:Origins"]!.Split(','))
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));   // ← لازم عشان كوكي الـ refresh تتبعت عبر الـ origins

// ─────────── Authorization ───────────
builder.Services.AddSingleton<IAuthorizationHandler, AdminRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, OwnershipRequirementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new AdminRequirement()));

    options.AddPolicy("ResourceOwner", policy =>
        policy.Requirements.Add(new OwnershipRequirement()));
});

// ─────────── Rate limiting ───────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                      // 5 محاولات بس
                Window = TimeSpan.FromMinutes(1),      // كل دقيقة
                QueueLimit = 0                         // مفيش انتظار في الطابور — يترفض فورًا
            }));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Masareef API", Version = "v1" });

    // ← ده اللي بيخلّي زر Authorize يظهر
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "اكتب: Bearer ثم مسافة ثم التوكن"
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
            Array.Empty<string>()
        }
    });
});

//builder.Services.AddSwaggerGen(c =>
//    c.SwaggerDoc("v1", new() { Title = "Masareef API", Version = "v1" }));

var app = builder.Build();

app.UseSerilogRequestLogging();   // بيسجّل كل request تلقائيًا: method, path, status code, المدة

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("spa");
app.UseAuthentication();      // ← جديد — لازم قبل UseAuthorization
app.UseRateLimiter();         // ← جديد
app.UseAuthorization();       // ← جديد
app.MapControllers();
app.Run();