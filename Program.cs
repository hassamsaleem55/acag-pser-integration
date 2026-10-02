using ACAG_PSER_Integration.Models.Configuration;
using ACAG_PSER_Integration.Services;
using ACAG_PSER_Integration.Services.Interfaces;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Configuration
// ---------------------------------------------------------

builder.Services
    .AddOptions<PserApiSettings>()
    .Bind(builder.Configuration.GetSection(PserApiSettings.SectionName))
    .Validate(
        settings => Uri.TryCreate(
            settings.BaseUrl,
            UriKind.Absolute,
            out _),
        "BaseUrl must be a valid absolute URI.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.SecretKey),
        "SecretKey is required.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.ConnectionString),
        "ConnectionString is required.")
    .ValidateOnStart();

// ---------------------------------------------------------
// Controllers / Swagger
// ---------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------------
// CORS
// ---------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://acag.punjab.gov.pk"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---------------------------------------------------------
// Application Services
// ---------------------------------------------------------

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IDecryptionService, DecryptionService>();
builder.Services.AddScoped<ICredentialFormatter, CredentialFormatter>();

// ---------------------------------------------------------
// ACAG HTTP Client
// ---------------------------------------------------------

builder.Services.AddHttpClient<IAcagHttpClient, AcagHttpClient>(
    (serviceProvider, client) =>
    {
        var settings =
            serviceProvider
                .GetRequiredService<IOptions<PserApiSettings>>()
                .Value;

        client.BaseAddress = new Uri(settings.BaseUrl);

        client.DefaultRequestHeaders.Add(
            "Accept",
            "application/json");

        client.Timeout = TimeSpan.FromSeconds(30);
    });

// ---------------------------------------------------------
// ACAG PSER Client
// ---------------------------------------------------------

builder.Services.AddScoped<IAcagPserClient, AcagPserClient>();

// ---------------------------------------------------------
// Build
// ---------------------------------------------------------

var app = builder.Build();

// ---------------------------------------------------------
// Swagger
// ---------------------------------------------------------
//
// Swagger is enabled for Development and QA.
// Do not expose it in Production unless intentionally required.
// ---------------------------------------------------------

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("QA"))
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "ACAG PSER API v1");

        options.RoutePrefix = string.Empty;
    });
}

// ---------------------------------------------------------
// HTTP Pipeline
// ---------------------------------------------------------

app.UseHttpsRedirection();

// Apply the registered "Client" CORS policy.
app.UseCors("Client");

app.UseAuthorization();

// ---------------------------------------------------------
// Health / Root Endpoint
// ---------------------------------------------------------

// ---------------------------------------------------------
// Controllers
// ---------------------------------------------------------

app.MapControllers();

app.Run();