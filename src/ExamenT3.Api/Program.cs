using ExamenT3.Api.Models;
using ExamenT3.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ =>
{
    var servicio = new GestionAcademicaService();
    servicio.CargarDatosDemo();
    return servicio;
});

var app = builder.Build();

app.MapGet("/", (IHostEnvironment env) => Results.Ok(new
{
    sistema = "Innovatec Education - Sistema de Gestión Integral de Servicios Académicos",
    version = "1.0.0",
    estado = "Activo",
    entorno = env.EnvironmentName,
    endpoints = new[]
    {
        "/health",
        "/api/estudiantes",
        "/api/horarios",
        "/api/matriculas",
        "/api/notas",
        "/api/reportes/{estudianteId}"
    }
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestampUtc = DateTime.UtcNow
}));

app.MapGet("/api/estudiantes", (GestionAcademicaService servicio) =>
    Results.Ok(servicio.ObtenerEstudiantes()));

app.MapGet("/api/estudiantes/{id:guid}", (Guid id, GestionAcademicaService servicio) =>
{
    var estudiante = servicio.ObtenerEstudiantePorId(id);
    return estudiante is null
        ? Results.NotFound(new { error = $"No se encontró el estudiante '{id}'." })
        : Results.Ok(estudiante);
});

app.MapPost("/api/estudiantes", (RegistrarEstudianteRequest request, GestionAcademicaService servicio) =>
    EjecutarOperacion(() =>
    {
        var creado = servicio.RegistrarEstudiante(request);
        return Results.Created($"/api/estudiantes/{creado.Id}", creado);
    }));

app.MapGet("/api/horarios", (GestionAcademicaService servicio) =>
    Results.Ok(servicio.ObtenerHorarios()));

app.MapPost("/api/horarios", (CrearHorarioRequest request, GestionAcademicaService servicio) =>
    EjecutarOperacion(() =>
    {
        var creado = servicio.CrearHorario(request);
        return Results.Created($"/api/horarios/{creado.Id}", creado);
    }));

app.MapGet("/api/matriculas/estudiante/{estudianteId:guid}", (Guid estudianteId, GestionAcademicaService servicio) =>
    Results.Ok(servicio.ObtenerMatriculasPorEstudiante(estudianteId)));

app.MapPost("/api/matriculas", (RegistrarMatriculaRequest request, GestionAcademicaService servicio) =>
    EjecutarOperacion(() =>
    {
        var creada = servicio.RegistrarMatricula(request);
        return Results.Created($"/api/matriculas/{creada.Id}", creada);
    }));

app.MapPost("/api/notas", (RegistrarNotaRequest request, GestionAcademicaService servicio) =>
    EjecutarOperacion(() =>
    {
        var creada = servicio.RegistrarNota(request);
        return Results.Created($"/api/notas/{creada.Id}", creada);
    }));

app.MapGet("/api/reportes/{estudianteId:guid}", (Guid estudianteId, GestionAcademicaService servicio) =>
    EjecutarOperacion(() => Results.Ok(servicio.GenerarReporte(estudianteId))));

app.Run();

static IResult EjecutarOperacion(Func<IResult> accion)
{
    try
    {
        return accion();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
}

public partial class Program;
