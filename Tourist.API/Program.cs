using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tourist.API;
using Tourist.API.Data;
using Tourist.API.Models;
using Tourist.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//logger
if (builder.Environment.IsProduction()) 
{
    builder.Logging.AddApplicationInsights(
        configureTelemetryConfiguration: (config) =>
            config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"],
        configureApplicationInsightsLoggerOptions: (options) =>
            options.TrackExceptionsAsExceptionTelemetry = true
    );
}

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TouristConnectionString")));

// Add global logging filter
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

//JWT Token
builder.Services.AddScoped<ITokenRepository, TokenRepository>();


builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("Tourist")
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(OptionsBuilderConfigurationExtensions =>
{
    OptionsBuilderConfigurationExtensions.Password.RequireDigit = false;
    OptionsBuilderConfigurationExtensions.Password.RequireLowercase = false;
    OptionsBuilderConfigurationExtensions.Password.RequireNonAlphanumeric = false;
    OptionsBuilderConfigurationExtensions.Password.RequireUppercase = false;
    OptionsBuilderConfigurationExtensions.Password.RequiredLength = 6;
    OptionsBuilderConfigurationExtensions.Password.RequiredUniqueChars = 1;
    OptionsBuilderConfigurationExtensions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    OptionsBuilderConfigurationExtensions.Lockout.MaxFailedAccessAttempts = 5;
    OptionsBuilderConfigurationExtensions.Lockout.AllowedForNewUsers = false;
    OptionsBuilderConfigurationExtensions.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    OptionsBuilderConfigurationExtensions.User.RequireUniqueEmail = false;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            AuthenticationType = "Jwt",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });



var app = builder.Build();

// Use built-in exception handler
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
}


// Error endpoint for production
app.Map("/error", (HttpContext httpContext) =>
{
    var feature = httpContext.Features.Get<IExceptionHandlerFeature>();
    var exception = feature?.Error;
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    if (exception != null)
    {
        logger.LogError(exception, "Unhandled exception caught by built-in middleware");
    }
    return Results.Problem("An unexpected error occurred.");
});

// Apply migrations automatically at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate(); // applies any pending migrations
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
