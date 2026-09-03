using Masareef.BusinessLayer.Services;
using Masareef.DAL.Context;
using Masareef.DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;          // ← جديد
using Microsoft.IdentityModel.Tokens;                          // ← جديد
using System.Text;                                             // ← جديد

var builder = WebApplication.CreateBuilder(args);

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
    .AllowAnyMethod()));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("spa");
app.UseAuthentication();      // ← جديد — لازم قبل UseAuthorization
app.UseAuthorization();       // ← جديد
app.MapControllers();
app.Run();