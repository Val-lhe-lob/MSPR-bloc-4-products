using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MSPR_bloc_4_products.Data;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
bool isTesting = builder.Environment.IsEnvironment("Testing");

// DbContext conditionnel
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    if (!isTesting)
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Swagger avec JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Products API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ex: Bearer eyJhbGciOiJIUzI1NiIs..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Authentification JWT
if (!isTesting)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
}

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (!isTesting)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
else
{
    // Mock Auth en Testing si nécessaire
    app.Use(async (context, next) =>
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "admin")
        }, "TestAuth");

        context.User = new ClaimsPrincipal(identity);
        await next();
    });
}

app.MapControllers();
app.Run();

public partial class Program { }
