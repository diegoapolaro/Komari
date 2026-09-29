using Komari.Api.Middleware;
using Komari.Application;
using Komari.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Controllers e OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Komari API",
        Version = "v1",
        Description = "API de operações e gestão para restaurantes e pizzarias (pedidos, cardápio, comandas, delivery, caixa)."
    });
});

// Tratamento global de erros (RFC 7807)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Injeção de dependências das camadas Clean Architecture
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS para desenvolvimento (permitindo conexão do painel web e do app mobile)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Pipeline HTTP
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Komari API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("DevCorsPolicy");

// Health check inicial
app.MapGet("/", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("System");

app.MapControllers();

app.Run();
