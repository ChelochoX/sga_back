using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using System.Data;

namespace sga_back.Repositories;

public class CursosRepository : ICursosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<CursosRepository> _logger;

    public CursosRepository(ILogger<CursosRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }
    public async Task<int> Insertar(Curso curso)
    {
        IDbTransaction? transaction = null;

        try
        {
            if (_conexion.State != ConnectionState.Open)
                _conexion.Open();

            transaction = _conexion.BeginTransaction();

            _logger.LogInformation("Intentando insertar curso: {Nombre}", curso.Nombre);

            // Mantenemos insert sobre Cursos sin romper estructura existente.
            // Los campos viejos quedan con valores por defecto/controlados para compatibilidad.
            string queryCurso = @"
                INSERT INTO Cursos
                (
                    nombre,
                    descripcion,
                    duracion,
                    unidad_duracion,
                    cantidad_cuota,
                    monto_cuota,
                    tiene_practica,
                    costo_practica,
                    fecha_inicio,
                    fecha_fin,
                    monto_matricula,
                    activo
                )
                VALUES
                (
                    @Nombre,
                    @Descripcion,
                    @Duracion,
                    @UnidadDuracion,
                    @CantidadCuota,
                    @MontoCuota,
                    @TienePractica,
                    @CostoPractica,
                    @FechaInicio,
                    @FechaFin,
                    @MontoMatricula,
                    @Activo
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int idCurso = await _conexion.ExecuteScalarAsync<int>(
                queryCurso,
                new
                {
                    curso.Nombre,
                    curso.Descripcion,
                    curso.Duracion,
                    curso.UnidadDuracion,
                    CantidadCuota = ObtenerCantidadCuotas(curso),
                    MontoCuota = ObtenerMontoCuota(curso),
                    TienePractica = TienePracticas(curso) ? "S" : "N",
                    CostoPractica = ObtenerMontoPractica(curso),
                    curso.FechaInicio,
                    curso.FechaFin,
                    MontoMatricula = ObtenerMontoMatricula(curso),
                    curso.Activo
                },
                transaction);

            if (curso.Conceptos != null && curso.Conceptos.Any())
            {
                foreach (var concepto in curso.Conceptos)
                {
                    string queryConcepto = @"
                        INSERT INTO CursoConceptos
                        (
                            id_curso,
                            tipo_concepto,
                            descripcion,
                            activo
                        )
                        VALUES
                        (
                            @IdCurso,
                            @TipoConcepto,
                            @Descripcion,
                            @Activo
                        );

                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idCursoConcepto = await _conexion.ExecuteScalarAsync<int>(
                        queryConcepto,
                        new
                        {
                            IdCurso = idCurso,
                            concepto.TipoConcepto,
                            concepto.Descripcion,
                            concepto.Activo
                        },
                        transaction);

                    if (concepto.Vencimientos != null && concepto.Vencimientos.Any())
                    {
                        foreach (var vencimiento in concepto.Vencimientos.OrderBy(x => x.NroOrden))
                        {
                            string queryVencimiento = @"
                                INSERT INTO CursoConceptosVencimientos
                                (
                                    id_curso_concepto,
                                    nro_orden,
                                    monto,
                                    fecha_vencimiento,
                                    descripcion,
                                    activo
                                )
                                VALUES
                                (
                                    @IdCursoConcepto,
                                    @NroOrden,
                                    @Monto,
                                    @FechaVencimiento,
                                    @Descripcion,
                                    @Activo
                                );";

                            await _conexion.ExecuteAsync(
                                queryVencimiento,
                                new
                                {
                                    IdCursoConcepto = idCursoConcepto,
                                    vencimiento.NroOrden,
                                    vencimiento.Monto,
                                    vencimiento.FechaVencimiento,
                                    vencimiento.Descripcion,
                                    vencimiento.Activo
                                },
                                transaction);
                        }
                    }
                }
            }

            transaction.Commit();

            _logger.LogInformation("Curso insertado con ID: {IdCurso}", idCurso);
            return idCurso;
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _logger.LogError(ex, "Error al insertar curso");
            throw new RepositoryException("Ocurrió un error al intentar insertar el curso.", ex);
        }
        finally
        {
            if (_conexion.State == ConnectionState.Open)
                _conexion.Close();
        }
    }

    public async Task<int> Actualizar(Curso curso)
    {
        IDbTransaction? transaction = null;

        try
        {
            if (_conexion.State != ConnectionState.Open)
                _conexion.Open();

            transaction = _conexion.BeginTransaction();

            _logger.LogInformation("Intentando actualizar curso con ID: {IdCurso}", curso.IdCurso);

            string queryCurso = @"
                UPDATE Cursos
                SET nombre = @Nombre,
                    descripcion = @Descripcion,
                    duracion = @Duracion,
                    unidad_duracion = @UnidadDuracion,
                    cantidad_cuota = @CantidadCuota,
                    monto_cuota = @MontoCuota,
                    tiene_practica = @TienePractica,
                    costo_practica = @CostoPractica,
                    fecha_inicio = @FechaInicio,
                    fecha_fin = @FechaFin,
                    monto_matricula = @MontoMatricula,
                    activo = @Activo
                WHERE id_curso = @IdCurso;";

            int filasAfectadas = await _conexion.ExecuteAsync(
                queryCurso,
                new
                {
                    curso.IdCurso,
                    curso.Nombre,
                    curso.Descripcion,
                    curso.Duracion,
                    curso.UnidadDuracion,
                    CantidadCuota = ObtenerCantidadCuotas(curso),
                    MontoCuota = ObtenerMontoCuota(curso),
                    TienePractica = TienePracticas(curso) ? "S" : "N",
                    CostoPractica = ObtenerMontoPractica(curso),
                    curso.FechaInicio,
                    curso.FechaFin,
                    MontoMatricula = ObtenerMontoMatricula(curso),
                    curso.Activo
                },
                transaction);

            if (filasAfectadas == 0)
            {
                transaction.Rollback();
                _logger.LogWarning("No se encontró el curso con ID: {IdCurso} para actualizar.", curso.IdCurso);
                throw new NoDataFoundException("No se encontró el curso para actualizar.");
            }

            // Limpiamos la configuración nueva del curso
            // OJO: acá borramos primero vencimientos y luego conceptos,
            // porque en tu script compartido no se ve claramente la FK con cascade
            string deleteVencimientos = @"
                DELETE v
                FROM CursoConceptosVencimientos v
                INNER JOIN CursoConceptos c
                    ON c.id_curso_concepto = v.id_curso_concepto
                WHERE c.id_curso = @IdCurso;";

            await _conexion.ExecuteAsync(
                deleteVencimientos,
                new { curso.IdCurso },
                transaction);

            string deleteConceptos = @"
                DELETE FROM CursoConceptos
                WHERE id_curso = @IdCurso;";

            await _conexion.ExecuteAsync(
                deleteConceptos,
                new { curso.IdCurso },
                transaction);

            if (curso.Conceptos != null && curso.Conceptos.Any())
            {
                foreach (var concepto in curso.Conceptos)
                {
                    string queryConcepto = @"
                        INSERT INTO CursoConceptos
                        (
                            id_curso,
                            tipo_concepto,
                            descripcion,
                            activo
                        )
                        VALUES
                        (
                            @IdCurso,
                            @TipoConcepto,
                            @Descripcion,
                            @Activo
                        );

                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idCursoConcepto = await _conexion.ExecuteScalarAsync<int>(
                        queryConcepto,
                        new
                        {
                            IdCurso = curso.IdCurso,
                            concepto.TipoConcepto,
                            concepto.Descripcion,
                            concepto.Activo
                        },
                        transaction);

                    if (concepto.Vencimientos != null && concepto.Vencimientos.Any())
                    {
                        foreach (var vencimiento in concepto.Vencimientos.OrderBy(x => x.NroOrden))
                        {
                            string queryVencimiento = @"
                                INSERT INTO CursoConceptosVencimientos
                                (
                                    id_curso_concepto,
                                    nro_orden,
                                    monto,
                                    fecha_vencimiento,
                                    descripcion,
                                    activo
                                )
                                VALUES
                                (
                                    @IdCursoConcepto,
                                    @NroOrden,
                                    @Monto,
                                    @FechaVencimiento,
                                    @Descripcion,
                                    @Activo
                                );";

                            await _conexion.ExecuteAsync(
                                queryVencimiento,
                                new
                                {
                                    IdCursoConcepto = idCursoConcepto,
                                    vencimiento.NroOrden,
                                    vencimiento.Monto,
                                    vencimiento.FechaVencimiento,
                                    vencimiento.Descripcion,
                                    vencimiento.Activo
                                },
                                transaction);
                        }
                    }
                }
            }

            transaction.Commit();

            _logger.LogInformation("Curso con ID: {IdCurso} actualizado exitosamente.", curso.IdCurso);
            return filasAfectadas;
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _logger.LogError(ex, "Error al actualizar curso con ID: {IdCurso}", curso.IdCurso);
            throw new RepositoryException("Ocurrió un error al intentar actualizar el curso.", ex);
        }
        finally
        {
            if (_conexion.State == ConnectionState.Open)
                _conexion.Close();
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        IDbTransaction? transaction = null;

        try
        {
            if (_conexion.State != ConnectionState.Open)
                _conexion.Open();

            transaction = _conexion.BeginTransaction();

            _logger.LogInformation("Intentando eliminar curso con ID: {IdCurso}", id);

            string deleteVencimientos = @"
                DELETE v
                FROM CursoConceptosVencimientos v
                INNER JOIN CursoConceptos c
                    ON c.id_curso_concepto = v.id_curso_concepto
                WHERE c.id_curso = @IdCurso;";

            await _conexion.ExecuteAsync(deleteVencimientos, new { IdCurso = id }, transaction);

            string deleteConceptos = @"
                DELETE FROM CursoConceptos
                WHERE id_curso = @IdCurso;";

            await _conexion.ExecuteAsync(deleteConceptos, new { IdCurso = id }, transaction);

            string deleteCurso = @"
                DELETE FROM Cursos
                WHERE id_curso = @IdCurso;";

            int filasAfectadas = await _conexion.ExecuteAsync(deleteCurso, new { IdCurso = id }, transaction);

            if (filasAfectadas == 0)
            {
                transaction.Rollback();
                _logger.LogWarning("No se encontró el curso con ID: {IdCurso} para eliminar.", id);
                return false;
            }

            transaction.Commit();

            _logger.LogInformation("Curso con ID: {IdCurso} eliminado exitosamente.", id);
            return true;
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _logger.LogError(ex, "Error al eliminar curso con ID: {IdCurso}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar el curso.", ex);
        }
        finally
        {
            if (_conexion.State == ConnectionState.Open)
                _conexion.Close();
        }
    }

    public async Task<CursoDetalleDto?> ObtenerDetallePorId(int idCurso)
    {
        try
        {
            _logger.LogInformation("Obteniendo detalle del curso con ID: {IdCurso}", idCurso);

            string queryCurso = @"
                SELECT
                    id_curso        AS IdCurso,
                    nombre          AS Nombre,
                    descripcion     AS Descripcion,
                    duracion        AS Duracion,
                    unidad_duracion AS UnidadDuracion,
                    fecha_inicio    AS FechaInicio,
                    fecha_fin       AS FechaFin,
                    activo          AS Activo
                FROM Cursos
                WHERE id_curso = @IdCurso;";

            var curso = await _conexion.QueryFirstOrDefaultAsync<CursoDetalleDto>(
                queryCurso,
                new { IdCurso = idCurso });

            if (curso == null)
            {
                _logger.LogWarning("No se encontró el curso con ID: {IdCurso}", idCurso);
                return null;
            }

            string queryConceptos = @"
                SELECT
                    id_curso_concepto AS IdCursoConcepto,
                    tipo_concepto     AS TipoConcepto,
                    descripcion       AS Descripcion,
                    activo            AS Activo
                FROM CursoConceptos
                WHERE id_curso = @IdCurso
                ORDER BY id_curso_concepto;";

            var conceptos = (await _conexion.QueryAsync<CursoConceptoDto>(
                queryConceptos,
                new { IdCurso = idCurso })).ToList();

            foreach (var concepto in conceptos)
            {
                string queryVencimientos = @"
                    SELECT
                        id_curso_concepto_vencimiento AS IdCursoConceptoVencimiento,
                        nro_orden                     AS NroOrden,
                        monto                         AS Monto,
                        fecha_vencimiento             AS FechaVencimiento,
                        descripcion                   AS Descripcion,
                        activo                        AS Activo
                    FROM CursoConceptosVencimientos
                    WHERE id_curso_concepto = @IdCursoConcepto
                    ORDER BY nro_orden;";

                var vencimientos = await _conexion.QueryAsync<CursoConceptoVencimientoDto>(
                    queryVencimientos,
                    new { IdCursoConcepto = concepto.IdCursoConcepto });

                concepto.Vencimientos = vencimientos.ToList();
            }

            curso.Conceptos = conceptos;

            _logger.LogInformation("Detalle del curso obtenido correctamente. IdCurso: {IdCurso}", idCurso);

            return curso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle del curso con ID: {IdCurso}", idCurso);
            throw new RepositoryException("Ocurrió un error al obtener el detalle del curso.", ex);
        }
    }

    public async Task<IEnumerable<CursoDto>> ObtenerCursosPorFecha(ObtenerCursosRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Obteniendo cursos. FechaInicio: {FechaInicio}, FechaFin: {FechaFin}, Activo: {Activo}",
                request.FechaInicio,
                request.FechaFin,
                request.Activo);

            string query = @"
                SELECT
                    id_curso           AS IdCurso,
                    nombre             AS Nombre,
                    descripcion        AS Descripcion,
                    duracion           AS Duracion,
                    unidad_duracion    AS UnidadDuracion,
                    cantidad_cuota     AS CantidadCuota,
                    monto_cuota        AS MontoCuota,
                    tiene_practica     AS TienePractica,
                    costo_practica     AS CostoPractica,
                    fecha_inicio       AS FechaInicio,
                    fecha_fin          AS FechaFin,
                    monto_matricula    AS MontoMatricula,
                    activo             AS Activo
                FROM Cursos
                WHERE
                    (@FechaInicio IS NULL OR fecha_inicio >= @FechaInicio)
                    AND (@FechaFin IS NULL OR fecha_inicio <= @FechaFin)
                    AND (@Activo IS NULL OR activo = @Activo)
                ORDER BY fecha_inicio DESC;";

            var cursos = await _conexion.QueryAsync<CursoDto>(
                query,
                new
                {
                    FechaInicio = request.FechaInicio,
                    FechaFin = request.FechaFin,
                    request.Activo
                });

            _logger.LogInformation("Se obtuvieron {Cantidad} cursos.", cursos.Count());

            return cursos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los cursos");
            throw new RepositoryException("Ocurrió un error al intentar obtener los cursos.", ex);
        }
    }

    public async Task CambiarEstado(int idCurso, bool activo)
    {
        try
        {
            _logger.LogInformation("Cambiando estado del curso. IdCurso: {IdCurso}, Activo: {Activo}", idCurso, activo);

            string query = @"
                UPDATE Cursos
                SET activo = @Activo
                WHERE id_curso = @IdCurso;";

            int filas = await _conexion.ExecuteAsync(query, new
            {
                Activo = activo,
                IdCurso = idCurso
            });

            if (filas == 0)
            {
                _logger.LogWarning("No se encontró el curso con IdCurso: {IdCurso} para cambiar estado.", idCurso);
                throw new InvalidOperationException("No se encontró el curso para cambiar estado.");
            }

            _logger.LogInformation("Estado del curso actualizado correctamente. IdCurso: {IdCurso}", idCurso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del curso con IdCurso: {IdCurso}", idCurso);
            throw new RepositoryException("Ocurrió un error al intentar cambiar el estado del curso.", ex);
        }
    }

    private static int ObtenerCantidadCuotas(Curso curso)
    {
        return curso.Conceptos?
            .Where(x => string.Equals(x.TipoConcepto, "Cuota", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Vencimientos?.Count ?? 0)
            .FirstOrDefault() ?? 0;
    }

    private static decimal ObtenerMontoCuota(Curso curso)
    {
        return curso.Conceptos?
            .Where(x => string.Equals(x.TipoConcepto, "Cuota", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Vencimientos?
                .OrderBy(v => v.NroOrden)
                .FirstOrDefault()?.Monto ?? 0m)
            .FirstOrDefault() ?? 0m;
    }

    private static bool TienePracticas(Curso curso)
    {
        return (curso.Conceptos?
            .Any(x => string.Equals(x.TipoConcepto, "Practica", StringComparison.OrdinalIgnoreCase)
                   && x.Vencimientos != null
                   && x.Vencimientos.Any())) ?? false;
    }

    private static decimal ObtenerMontoPractica(Curso curso)
    {
        return curso.Conceptos?
            .Where(x => string.Equals(x.TipoConcepto, "Practica", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Vencimientos?
                .OrderBy(v => v.NroOrden)
                .FirstOrDefault()?.Monto ?? 0m)
            .FirstOrDefault() ?? 0m;
    }

    private static decimal ObtenerMontoMatricula(Curso curso)
    {
        return curso.Conceptos?
            .Where(x => string.Equals(x.TipoConcepto, "Matricula", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Vencimientos?
                .OrderBy(v => v.NroOrden)
                .FirstOrDefault()?.Monto ?? 0m)
            .FirstOrDefault() ?? 0m;
    }
}
