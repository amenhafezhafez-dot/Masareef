using Masareef.Api.Authorization;
using Masareef.BusinessLayer.Services;
using Masareef.DAL.Context;
using Masareef.DAL.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;




Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();   // ← بيستبدل الـ logging الافتراضي بالكامل


// ─────────── التحقق من الإعدادات المطلوبة ───────────
// بنفحصهم كلهم مرة واحدة ونبلّغ بالناقص كله في رسالة واحدة، عشان اللي بينشر
// ميضطرش يصلّح واحد ويكتشف التاني. أحسن من إن التطبيق يقلع بأمان وهمي.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtKey = builder.Configuration["Jwt:Key"];

// AllowCredentials مابتشتغلش مع origins مفتوحة، فلازم قائمة صريحة.
var corsOrigins = (builder.Configuration["Cors:Origins"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var configErrors = new List<string>();

if (string.IsNullOrWhiteSpace(connectionString))
    configErrors.Add(
        "  • ConnectionStrings:DefaultConnection is missing. " +
        "Dev: appsettings.Development.json | Prod: env var ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    configErrors.Add(
        "  • Jwt:Key is missing or shorter than 32 characters. " +
        "Dev: dotnet user-secrets set \"Jwt:Key\" \"<random 32+ chars>\" | Prod: env var Jwt__Key");

if (corsOrigins.Length == 0)
    configErrors.Add(
        "  • Cors:Origins is missing — the browser would block the frontend. " +
        "Dev: appsettings.Development.json | Prod: env var Cors__Origins (https:// required)");

if (configErrors.Count > 0)
{
    // بنعرض كمان من فين كان بيدوّر، عشان اللي بينشر يعرف السبب بدل ما يخمّن
    // (مثلاً: البيئة مش Development فالـ User Secrets مش بتتحمّل أصلاً).
    var sources = string.Join(", ",
        ((IConfigurationRoot)builder.Configuration).Providers.Select(p => p.ToString()));

    throw new InvalidOperationException(
        $"Masareef API cannot start — {configErrors.Count} configuration problem(s):"
        + Environment.NewLine + string.Join(Environment.NewLine, configErrors)
        + Environment.NewLine
        + Environment.NewLine + "Where it looked:"
        + Environment.NewLine + $"  Environment     : {builder.Environment.EnvironmentName}"
        + Environment.NewLine + $"  ApplicationName : {builder.Environment.ApplicationName}"
        + Environment.NewLine + $"  ContentRoot     : {builder.Environment.ContentRootPath}"
        + Environment.NewLine + $"  Config sources  : {sources}"
        + Environment.NewLine
        + Environment.NewLine + "See README.md for setup steps.");
}

// بعد الفحص فوق الاتنين دول مضمون إنهم مش null — بنثبّتهم للكومبايلر.
var validatedConnectionString = connectionString!;
var validatedJwtKey = jwtKey!;


// ─────────── DbContext ───────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(validatedConnectionString));

// ─────────── DAL ───────────
builder.Services.AddScoped<IDebtRepository, DebtRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();   // ← جديد (refresh token)
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();           // ← جديد (auditing)


// ─────────── BLL ───────────
builder.Services.AddScoped<DebtService>();
builder.Services.AddScoped<HomeService>();
builder.Services.AddScoped<BusinessService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AuditService>();            // ← جديد (auditing)

// الـ AuditService محتاجه عشان يقرا الـ IP بتاع الطلب
builder.Services.AddHttpContextAccessor();


// ─────────── CORS ───────────
builder.Services.AddCors(o => o.AddPolicy("spa", p => p
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));


// ─────────── JWT Authentication ───────────
// المفتاح اتفحص فوق — جاي من User Secrets (تطوير) أو Jwt__Key (إنتاج).
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
                Encoding.UTF8.GetBytes(validatedJwtKey))
        };
    });


// ------------- Authorization & Ownership -----------------
builder.Services.AddSingleton<IAuthorizationHandler, AdminRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, OwnershipRequirementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new AdminRequirement()));

    options.AddPolicy("ResourceOwner", policy =>
        policy.Requirements.Add(new OwnershipRequirement()));
});


//------------- refresh token -------------------
// (التسجيل في بلوك DAL فوق — IRefreshTokenRepository)


// ------------ rate limit --------------------


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




// ------------- logging and audince ------------


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



var app = builder.Build();

app.UseSerilogRequestLogging();   // بيسجّل كل request تلقائيًا: method, path, status code, المدة

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseHttpsRedirection();
app.UseCors("spa");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.Run();
