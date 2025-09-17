using Asp.Versioning;
using Asp.Versioning.Conventions;
using Azure.Identity;
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
if (builder.Environment.IsProduction())
{ 
    //key-vault
    var keyVaultUrl = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }

    builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) =>
        config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"],
    configureApplicationInsightsLoggerOptions: (options) =>
        options.TrackExceptionsAsExceptionTelemetry = true
 );
}

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TouristConnectionString"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));


builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

//Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("api-version")
        );
}).AddMvc(options =>
{
    options.Conventions.Add(new VersionByNamespaceConvention());
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

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
        logger.LogError(exception, "Unhandled exception caught by built-in middleware: {ExceptionType} - {Message}\nStackTrace: {StackTrace}", 
            exception.GetType().FullName, exception.Message, exception.StackTrace);
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

//cors
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
