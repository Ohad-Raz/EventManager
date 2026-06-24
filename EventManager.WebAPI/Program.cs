using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1",
        new OpenApiInfo { Title = "Event Manager Web API", Version = "v1" });
    // Configure Swagger to accept JWT Bearer tokens through the Authorize button.
    option.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter valid JWT",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
    // Apply the Bearer token configuration to protected API endpoints.
    option.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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
                new List<string>()
            }
        });
});
//Controllers,mapper,repositories-services for DI
builder.Services.AddControllers();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddScoped<IEventRepository, DbEventRepository>();
builder.Services.AddScoped<IEventTypeRepository, DbEventTypeRepository>();
builder.Services.AddScoped<IEventPerformerRepository, DbEventPerformerRepository>();
builder.Services.AddScoped<ILogRepository, DbLogRepository>();
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddScoped<IPerformerRepository, DbPerformerRepository>();
builder.Services.AddScoped<IRegistrationRepository, DbRegistrationRepository>();
builder.Services.AddEndpointsApiExplorer();
// using named connection string from appsettings.json
builder.Services.AddDbContext<EventManagerDbContext>(options => {
    options.UseSqlServer("name=ConnectionStrings:DefaultConn");
});
// Configure JWT security services
var secureKey = builder.Configuration["JWT:SecureKey"]
    ?? throw new InvalidOperationException("JWT secure key is missing.");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var Key = Encoding.UTF8.GetBytes(secureKey);
        // Keep JWT claim names as "unique_name", "sub", "nameid", and "role"
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Key),
            // User.Identity.Name reads the "unique_name" claim
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            // [Authorize(Roles = "...")] reads the "role" claim
            RoleClaimType = "role"
        };
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
