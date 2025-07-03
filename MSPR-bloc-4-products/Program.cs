using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MSPR_bloc_4_products.Data;
using MSPR_bloc_4_products.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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

// Authentification JWT ou Mock
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
else
{

    // Ajout d'un handler minimaliste pour satisfaire UseAuthorization sans package externe

    builder.Services.AddAuthentication("TestAuth")
        .AddScheme<AuthenticationSchemeOptions, DummyHandler>("TestAuth", _ => { });
}

builder.Services.AddControllers();

// Injection RabbitMQ Streams Consumer
builder.Services.AddSingleton<RabbitMqConsumer>();


var app = builder.Build();

//Initialisation du Consumer RabbitMQ
var consumer = app.Services.GetRequiredService<RabbitMqConsumer>();


if (!isTesting)
{
    builder.Services.AddSingleton<RabbitMqConsumer>();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Toujours utiliser Auth pour satisfaire Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }

// DummyHandler interne sans dépendance pour mocker automatiquement le user dans les tests
public class DummyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DummyHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestAuth");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
