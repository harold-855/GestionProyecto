using GestionProyecto.Application.Extensions;
using GestionProyecto.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de la API.
builder.Services.AddControllers();

// Registrar Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar Application.
builder.Services.AddApplication();

// Registrar Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configurar Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();