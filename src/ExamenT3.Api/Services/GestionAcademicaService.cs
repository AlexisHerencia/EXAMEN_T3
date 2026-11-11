using ExamenT3.Api.Models;

namespace ExamenT3.Api.Services;

/// <summary>
/// Servicio principal para la gestión académica de Innovatec Education:
/// estudiantes, horarios, matrículas, notas y reportes.
/// </summary>
public sealed class GestionAcademicaService
{
    public const decimal NotaMinima = 0m;
    public const decimal NotaMaxima = 20m;
    public const decimal NotaMinimaAprobatoria = 11.0m;

    private readonly object _syncLock = new();
    private readonly List<Estudiante> _estudiantes = [];
    private readonly List<HorarioCurso> _horarios = [];
    private readonly List<Matricula> _matriculas = [];
    private readonly List<NotaEvaluacion> _notas = [];

    /// <summary>
    /// Carga datos iniciales de demostración si el servicio está vacío.
    /// </summary>
    public void CargarDatosDemo()
    {
        lock (_syncLock)
        {
            if (_estudiantes.Count > 0 || _horarios.Count > 0)
            {
                return;
            }

            var estudiante1 = RegistrarEstudianteInterno(new RegistrarEstudianteRequest(
                "EST-2026-001",
                "Lucía Fernández Paredes",
                "lucia.fernandez@innovatec.edu.pe"));

            var estudiante2 = RegistrarEstudianteInterno(new RegistrarEstudianteRequest(
                "EST-2026-002",
                "Carlos Mendoza Quispe",
                "carlos.mendoza@innovatec.edu.pe"));

            var horario1 = CrearHorarioInterno(new CrearHorarioRequest(
                "INF-301",
                "Calidad y Automatización de Pruebas",
                "Lunes",
                new TimeOnly(8, 0),
                new TimeOnly(10, 0),
                "LAB-401"));

            var horario2 = CrearHorarioInterno(new CrearHorarioRequest(
                "INF-302",
                "Integración Continua y Contenedores",
                "Miércoles",
                new TimeOnly(10, 0),
                new TimeOnly(12, 0),
                "LAB-402"));

            var matricula1 = RegistrarMatriculaInterna(new RegistrarMatriculaRequest(
                estudiante1.Id,
                horario1.Id,
                "2026-II"));

            RegistrarMatriculaInterna(new RegistrarMatriculaRequest(
                estudiante2.Id,
                horario2.Id,
                "2026-II"));

            RegistrarNotaInterna(new RegistrarNotaRequest(matricula1.Id, "Evaluación T1", 15.5m));
            RegistrarNotaInterna(new RegistrarNotaRequest(matricula1.Id, "Evaluación T2", 17.0m));
        }
    }

    /// <summary>
    /// Obtiene todos los estudiantes registrados.
    /// </summary>
    public IReadOnlyList<Estudiante> ObtenerEstudiantes()
    {
        lock (_syncLock)
        {
            return _estudiantes.ToList();
        }
    }

    /// <summary>
    /// Busca un estudiante por su identificador único.
    /// </summary>
    public Estudiante? ObtenerEstudiantePorId(Guid id)
    {
        lock (_syncLock)
        {
            return _estudiantes.FirstOrDefault(e => e.Id == id);
        }
    }

    /// <summary>
    /// Registra un nuevo estudiante validando campos obligatorios y unicidad de código y correo.
    /// </summary>
    public Estudiante RegistrarEstudiante(RegistrarEstudianteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_syncLock)
        {
            return RegistrarEstudianteInterno(request);
        }
    }

    /// <summary>
    /// Obtiene todos los horarios de cursos disponibles.
    /// </summary>
    public IReadOnlyList<HorarioCurso> ObtenerHorarios()
    {
        lock (_syncLock)
        {
            return _horarios.ToList();
        }
    }

    /// <summary>
    /// Crea un horario de curso validando rango horario y ausencia de cruces en el mismo aula.
    /// </summary>
    public HorarioCurso CrearHorario(CrearHorarioRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_syncLock)
        {
            return CrearHorarioInterno(request);
        }
    }

    /// <summary>
    /// Lista las matrículas de un estudiante específico.
    /// </summary>
    public IReadOnlyList<Matricula> ObtenerMatriculasPorEstudiante(Guid estudianteId)
    {
        lock (_syncLock)
        {
            return _matriculas.Where(m => m.EstudianteId == estudianteId).ToList();
        }
    }

    /// <summary>
    /// Registra la matrícula de un estudiante verificando duplicados y cruces de horario.
    /// </summary>
    public Matricula RegistrarMatricula(RegistrarMatriculaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_syncLock)
        {
            return RegistrarMatriculaInterna(request);
        }
    }

    /// <summary>
    /// Registra una calificación dentro de la escala vigesimal (0 a 20).
    /// </summary>
    public NotaEvaluacion RegistrarNota(RegistrarNotaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_syncLock)
        {
            return RegistrarNotaInterna(request);
        }
    }

    /// <summary>
    /// Genera el reporte académico consolidado de un estudiante con sus cursos, promedios y condición.
    /// </summary>
    public ReporteAcademico GenerarReporte(Guid estudianteId)
    {
        lock (_syncLock)
        {
            var estudiante = _estudiantes.FirstOrDefault(e => e.Id == estudianteId)
                ?? throw new KeyNotFoundException($"No existe el estudiante con Id '{estudianteId}'.");

            var matriculasEstudiante = _matriculas
                .Where(m => m.EstudianteId == estudianteId)
                .ToList();

            var resumenCursos = new List<ResumenCursoReporte>();

            foreach (var matricula in matriculasEstudiante)
            {
                var horario = _horarios.First(h => h.Id == matricula.HorarioId);
                var calificaciones = _notas
                    .Where(n => n.MatriculaId == matricula.Id)
                    .Select(n => n.Calificacion)
                    .ToList();

                var promedio = calificaciones.Count > 0
                    ? Math.Round(calificaciones.Average(), 2, MidpointRounding.AwayFromZero)
                    : 0m;

                var estado = calificaciones.Count == 0
                    ? "Sin notas"
                    : promedio >= NotaMinimaAprobatoria
                        ? "Aprobado"
                        : "Desaprobado";

                resumenCursos.Add(new ResumenCursoReporte(
                    matricula.Id,
                    horario.CodigoCurso,
                    horario.NombreCurso,
                    matricula.Periodo,
                    calificaciones,
                    promedio,
                    estado));
            }

            var cursosConNotas = resumenCursos.Where(c => c.Notas.Count > 0).ToList();
            var promedioGeneral = cursosConNotas.Count > 0
                ? Math.Round(cursosConNotas.Average(c => c.Promedio), 2, MidpointRounding.AwayFromZero)
                : 0m;

            var condicionGeneral = cursosConNotas.Count == 0
                ? "Sin calificaciones"
                : promedioGeneral >= NotaMinimaAprobatoria
                    ? "Aprobado"
                    : "En observación";

            return new ReporteAcademico(
                estudiante.Id,
                estudiante.Codigo,
                estudiante.NombreCompleto,
                promedioGeneral,
                condicionGeneral,
                resumenCursos);
        }
    }

    private Estudiante RegistrarEstudianteInterno(RegistrarEstudianteRequest request)
    {
        var codigo = ValidarTextoObligatorio(request.Codigo, nameof(request.Codigo));
        var nombre = ValidarTextoObligatorio(request.NombreCompleto, nameof(request.NombreCompleto));
        var correo = ValidarTextoObligatorio(request.Correo, nameof(request.Correo));

        if (!correo.Contains('@', StringComparison.Ordinal) || correo.StartsWith('@') || correo.EndsWith('@'))
        {
            throw new ArgumentException("El correo electrónico no tiene un formato válido.", nameof(request.Correo));
        }

        if (_estudiantes.Any(e => e.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Ya existe un estudiante registrado con el código '{codigo}'.");
        }

        if (_estudiantes.Any(e => e.Correo.Equals(correo, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Ya existe un estudiante registrado con el correo '{correo}'.");
        }

        var estudiante = new Estudiante(Guid.NewGuid(), codigo, nombre, correo, DateTime.UtcNow);
        _estudiantes.Add(estudiante);
        return estudiante;
    }

    private HorarioCurso CrearHorarioInterno(CrearHorarioRequest request)
    {
        var codigoCurso = ValidarTextoObligatorio(request.CodigoCurso, nameof(request.CodigoCurso));
        var nombreCurso = ValidarTextoObligatorio(request.NombreCurso, nameof(request.NombreCurso));
        var diaSemana = ValidarTextoObligatorio(request.DiaSemana, nameof(request.DiaSemana));
        var aula = ValidarTextoObligatorio(request.Aula, nameof(request.Aula));

        if (request.HoraFin <= request.HoraInicio)
        {
            throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");
        }

        var hayCruceEnAula = _horarios.Any(h =>
            h.DiaSemana.Equals(diaSemana, StringComparison.OrdinalIgnoreCase) &&
            h.Aula.Equals(aula, StringComparison.OrdinalIgnoreCase) &&
            SeSuperponen(h.HoraInicio, h.HoraFin, request.HoraInicio, request.HoraFin));

        if (hayCruceEnAula)
        {
            throw new InvalidOperationException(
                $"El aula '{aula}' ya se encuentra ocupada el día '{diaSemana}' en ese rango horario.");
        }

        var horario = new HorarioCurso(
            Guid.NewGuid(),
            codigoCurso,
            nombreCurso,
            diaSemana,
            request.HoraInicio,
            request.HoraFin,
            aula);

        _horarios.Add(horario);
        return horario;
    }

    private Matricula RegistrarMatriculaInterna(RegistrarMatriculaRequest request)
    {
        var periodo = ValidarTextoObligatorio(request.Periodo, nameof(request.Periodo));

        var estudianteExiste = _estudiantes.Any(e => e.Id == request.EstudianteId);
        if (!estudianteExiste)
        {
            throw new KeyNotFoundException($"No existe el estudiante con Id '{request.EstudianteId}'.");
        }

        var horarioNuevo = _horarios.FirstOrDefault(h => h.Id == request.HorarioId)
            ?? throw new KeyNotFoundException($"No existe el horario con Id '{request.HorarioId}'.");

        var matriculasPeriodo = _matriculas
            .Where(m => m.EstudianteId == request.EstudianteId &&
                        m.Periodo.Equals(periodo, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var matriculaActual in matriculasPeriodo)
        {
            var horarioActual = _horarios.First(h => h.Id == matriculaActual.HorarioId);

            if (horarioActual.Id == horarioNuevo.Id ||
                horarioActual.CodigoCurso.Equals(horarioNuevo.CodigoCurso, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"El estudiante ya está matriculado en el curso '{horarioNuevo.CodigoCurso}' para el periodo '{periodo}'.");
            }

            if (horarioActual.DiaSemana.Equals(horarioNuevo.DiaSemana, StringComparison.OrdinalIgnoreCase) &&
                SeSuperponen(horarioActual.HoraInicio, horarioActual.HoraFin, horarioNuevo.HoraInicio, horarioNuevo.HoraFin))
            {
                throw new InvalidOperationException(
                    $"Cruce de horario detectado el día '{horarioNuevo.DiaSemana}' con el curso '{horarioActual.NombreCurso}'.");
            }
        }

        var matricula = new Matricula(
            Guid.NewGuid(),
            request.EstudianteId,
            request.HorarioId,
            periodo,
            DateTime.UtcNow);

        _matriculas.Add(matricula);
        return matricula;
    }

    private NotaEvaluacion RegistrarNotaInterna(RegistrarNotaRequest request)
    {
        var descripcion = ValidarTextoObligatorio(request.Descripcion, nameof(request.Descripcion));

        if (request.Calificacion < NotaMinima || request.Calificacion > NotaMaxima)
        {
            throw new ArgumentException(
                $"La calificación debe estar entre {NotaMinima} y {NotaMaxima}.",
                nameof(request.Calificacion));
        }

        if (_matriculas.All(m => m.Id != request.MatriculaId))
        {
            throw new KeyNotFoundException($"No existe la matrícula con Id '{request.MatriculaId}'.");
        }

        var nota = new NotaEvaluacion(
            Guid.NewGuid(),
            request.MatriculaId,
            descripcion,
            request.Calificacion,
            DateTime.UtcNow);

        _notas.Add(nota);
        return nota;
    }

    private static string ValidarTextoObligatorio(string? valor, string nombreCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException($"El campo '{nombreCampo}' es obligatorio.", nombreCampo);
        }

        return valor.Trim();
    }

    private static bool SeSuperponen(TimeOnly inicioA, TimeOnly finA, TimeOnly inicioB, TimeOnly finB) =>
        inicioA < finB && finA > inicioB;
}
