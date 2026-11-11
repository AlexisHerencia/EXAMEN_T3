namespace ExamenT3.Api.Models;

/// <summary>
/// Representa un estudiante registrado en el sistema académico.
/// </summary>
public sealed record Estudiante(
    Guid Id,
    string Codigo,
    string NombreCompleto,
    string Correo,
    DateTime FechaRegistroUtc);

/// <summary>
/// Solicitud para registrar un nuevo estudiante.
/// </summary>
public sealed record RegistrarEstudianteRequest(
    string Codigo,
    string NombreCompleto,
    string Correo);

/// <summary>
/// Representa el horario asignado a un curso en un aula y día determinados.
/// </summary>
public sealed record HorarioCurso(
    Guid Id,
    string CodigoCurso,
    string NombreCurso,
    string DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string Aula);

/// <summary>
/// Solicitud para crear un nuevo horario de curso.
/// </summary>
public sealed record CrearHorarioRequest(
    string CodigoCurso,
    string NombreCurso,
    string DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string Aula);

/// <summary>
/// Representa la matrícula de un estudiante en un horario y periodo académico.
/// </summary>
public sealed record Matricula(
    Guid Id,
    Guid EstudianteId,
    Guid HorarioId,
    string Periodo,
    DateTime FechaMatriculaUtc);

/// <summary>
/// Solicitud para matricular a un estudiante en un curso.
/// </summary>
public sealed record RegistrarMatriculaRequest(
    Guid EstudianteId,
    Guid HorarioId,
    string Periodo);

/// <summary>
/// Representa una calificación registrada sobre una matrícula.
/// </summary>
public sealed record NotaEvaluacion(
    Guid Id,
    Guid MatriculaId,
    string Descripcion,
    decimal Calificacion,
    DateTime FechaRegistroUtc);

/// <summary>
/// Solicitud para registrar una nota en una matrícula existente.
/// </summary>
public sealed record RegistrarNotaRequest(
    Guid MatriculaId,
    string Descripcion,
    decimal Calificacion);

/// <summary>
/// Resumen académico de un curso dentro del reporte del estudiante.
/// </summary>
public sealed record ResumenCursoReporte(
    Guid MatriculaId,
    string CodigoCurso,
    string NombreCurso,
    string Periodo,
    IReadOnlyList<decimal> Notas,
    decimal Promedio,
    string Estado);

/// <summary>
/// Reporte académico consolidado de un estudiante.
/// </summary>
public sealed record ReporteAcademico(
    Guid EstudianteId,
    string CodigoEstudiante,
    string NombreEstudiante,
    decimal PromedioGeneral,
    string CondicionGeneral,
    IReadOnlyList<ResumenCursoReporte> Cursos);
