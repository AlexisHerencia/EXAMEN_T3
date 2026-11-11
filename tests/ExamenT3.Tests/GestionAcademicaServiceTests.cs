using ExamenT3.Api.Models;
using ExamenT3.Api.Services;

namespace ExamenT3.Tests;

public sealed class GestionAcademicaServiceTests
{
    private readonly GestionAcademicaService _servicio = new();

    [Fact]
    public void RegistrarEstudiante_DatosValidos_RegistraYPermiteConsultar()
    {
        var creado = _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
            "EST-001",
            "Ana Torres López",
            "ana.torres@innovatec.edu.pe"));

        var encontrado = _servicio.ObtenerEstudiantePorId(creado.Id);

        Assert.NotNull(encontrado);
        Assert.Equal("EST-001", encontrado.Codigo);
        Assert.Equal("Ana Torres López", encontrado.NombreCompleto);
        Assert.Single(_servicio.ObtenerEstudiantes());
    }

    [Theory]
    [InlineData("", "Ana Torres", "ana@innovatec.edu.pe")]
    [InlineData("EST-001", "", "ana@innovatec.edu.pe")]
    [InlineData("EST-001", "Ana Torres", "correo-invalido")]
    [InlineData("EST-001", "Ana Torres", "@innovatec.edu.pe")]
    public void RegistrarEstudiante_DatosInvalidos_LanzaArgumentException(
        string codigo,
        string nombre,
        string correo)
    {
        Assert.Throws<ArgumentException>(() =>
            _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(codigo, nombre, correo)));
    }

    [Fact]
    public void RegistrarEstudiante_CodigoOCorreoDuplicado_LanzaInvalidOperationException()
    {
        _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
            "EST-001",
            "Ana Torres",
            "ana@innovatec.edu.pe"));

        Assert.Throws<InvalidOperationException>(() =>
            _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
                "est-001",
                "Otra Persona",
                "otra@innovatec.edu.pe")));

        Assert.Throws<InvalidOperationException>(() =>
            _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
                "EST-002",
                "Otra Persona",
                "ANA@innovatec.edu.pe")));
    }

    [Fact]
    public void CrearHorario_RangoInvalidoOCruceEnAula_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            _servicio.CrearHorario(new CrearHorarioRequest(
                "INF-101",
                "Algoritmos",
                "Lunes",
                new TimeOnly(10, 0),
                new TimeOnly(10, 0),
                "A-101")));

        _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-101",
            "Algoritmos",
            "Lunes",
            new TimeOnly(8, 0),
            new TimeOnly(10, 0),
            "A-101"));

        Assert.Throws<InvalidOperationException>(() =>
            _servicio.CrearHorario(new CrearHorarioRequest(
                "INF-102",
                "Base de Datos",
                "Lunes",
                new TimeOnly(9, 30),
                new TimeOnly(11, 0),
                "A-101")));

        // Horario contiguo sin superposición en la misma aula sí debe permitirse
        var contiguo = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-102",
            "Base de Datos",
            "Lunes",
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            "A-101"));

        Assert.Equal("INF-102", contiguo.CodigoCurso);
    }

    [Fact]
    public void RegistrarMatricula_CruceDeHorarioODuplicado_LanzaInvalidOperationException()
    {
        var estudiante = _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
            "EST-010",
            "Luis Vega",
            "luis.vega@innovatec.edu.pe"));

        var horarioManana = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-201",
            "Programación Orientada a Objetos",
            "Martes",
            new TimeOnly(8, 0),
            new TimeOnly(10, 0),
            "LAB-1"));

        var horarioCruce = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-202",
            "Arquitectura de Software",
            "Martes",
            new TimeOnly(9, 0),
            new TimeOnly(11, 0),
            "LAB-2"));

        var horarioValido = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-203",
            "Pruebas de Software",
            "Jueves",
            new TimeOnly(8, 0),
            new TimeOnly(10, 0),
            "LAB-1"));

        _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, horarioManana.Id, "2026-II"));

        Assert.Throws<InvalidOperationException>(() =>
            _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, horarioManana.Id, "2026-II")));

        Assert.Throws<InvalidOperationException>(() =>
            _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, horarioCruce.Id, "2026-II")));

        var segunda = _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, horarioValido.Id, "2026-II"));

        Assert.Equal(2, _servicio.ObtenerMatriculasPorEstudiante(estudiante.Id).Count);
        Assert.Equal(horarioValido.Id, segunda.HorarioId);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(20.1)]
    public void RegistrarNota_FueraDeRangoVigesimal_LanzaArgumentException(double calificacion)
    {
        var estudiante = _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
            "EST-020",
            "Sofía Díaz",
            "sofia.diaz@innovatec.edu.pe"));
        var horario = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-305",
            "DevOps",
            "Viernes",
            new TimeOnly(14, 0),
            new TimeOnly(16, 0),
            "LAB-3"));
        var matricula = _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, horario.Id, "2026-II"));

        Assert.Throws<ArgumentException>(() =>
            _servicio.RegistrarNota(new RegistrarNotaRequest(matricula.Id, "Examen Parcial", (decimal)calificacion)));
    }

    [Fact]
    public void GenerarReporte_CalculaPromediosYCondicionCorrectamente()
    {
        var estudiante = _servicio.RegistrarEstudiante(new RegistrarEstudianteRequest(
            "EST-030",
            "Mateo Rojas",
            "mateo.rojas@innovatec.edu.pe"));

        var cursoAprobado = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-401",
            "CI/CD con GitHub Actions",
            "Lunes",
            new TimeOnly(18, 0),
            new TimeOnly(20, 0),
            "AULA-1"));

        var cursoDesaprobado = _servicio.CrearHorario(new CrearHorarioRequest(
            "INF-402",
            "Contenedores con Docker",
            "Miércoles",
            new TimeOnly(18, 0),
            new TimeOnly(20, 0),
            "AULA-2"));

        var m1 = _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, cursoAprobado.Id, "2026-II"));
        var m2 = _servicio.RegistrarMatricula(new RegistrarMatriculaRequest(estudiante.Id, cursoDesaprobado.Id, "2026-II"));

        _servicio.RegistrarNota(new RegistrarNotaRequest(m1.Id, "T1", 16m));
        _servicio.RegistrarNota(new RegistrarNotaRequest(m1.Id, "T2", 18m));

        _servicio.RegistrarNota(new RegistrarNotaRequest(m2.Id, "T1", 8m));
        _servicio.RegistrarNota(new RegistrarNotaRequest(m2.Id, "T2", 10m));

        var reporte = _servicio.GenerarReporte(estudiante.Id);

        Assert.Equal("EST-030", reporte.CodigoEstudiante);
        Assert.Equal(2, reporte.Cursos.Count);
        Assert.Equal(17.0m, reporte.Cursos[0].Promedio);
        Assert.Equal("Aprobado", reporte.Cursos[0].Estado);
        Assert.Equal(9.0m, reporte.Cursos[1].Promedio);
        Assert.Equal("Desaprobado", reporte.Cursos[1].Estado);
        Assert.Equal(13.0m, reporte.PromedioGeneral);
        Assert.Equal("Aprobado", reporte.CondicionGeneral);
    }

    [Fact]
    public void CargarDatosDemo_EsIdempotenteYGeneraInformacionInicial()
    {
        _servicio.CargarDatosDemo();
        _servicio.CargarDatosDemo();

        var estudiantes = _servicio.ObtenerEstudiantes();
        Assert.Equal(2, estudiantes.Count);
        Assert.Equal(2, _servicio.ObtenerHorarios().Count);

        var reporte = _servicio.GenerarReporte(estudiantes[0].Id);
        Assert.Single(reporte.Cursos);
        Assert.Equal("Aprobado", reporte.CondicionGeneral);
    }
}
