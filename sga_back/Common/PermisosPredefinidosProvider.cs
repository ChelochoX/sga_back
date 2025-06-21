namespace sga_back.Common;

public static class PermisosPredefinidosProvider
{
    public static List<(string Entidad, string Recurso)> ObtenerPorRol(string nombreRol)
    {
        return nombreRol switch
        {
            "Cajero" => new()
            {
                ("Cursos", "Consultar"),
                ("Cursos", "Crear"),
                ("Cursos", "Editar"),
                ("Cursos", "Eliminar"),

                ("Inscripciones", "Consultar"),
                ("Inscripciones", "Crear"),
                ("Inscripciones", "Editar"),
                ("Inscripciones", "Eliminar"),

                ("Pagos", "Consultar"),
                ("Pagos", "Crear"),
                ("Pagos", "Editar"),
                ("Pagos", "Eliminar"),
                ("Pagos", "Reporte"),

                ("Caja", "Consultar"),
                ("Caja", "Crear"),
                ("Caja", "Editar"),
                ("Caja", "Eliminar"),
                ("Caja", "Imprimir"),
                ("Caja", "CambiarEstado"),
                ("Caja", "Reporte")
            },

            "Superadministrador" => ObtenerTodosLosPermisos(),

            _ => new()
        };
    }

    private static List<(string Entidad, string Recurso)> ObtenerTodosLosPermisos()
    {
        var entidades = new[]
        {
            "Cursos", "Inscripciones", "Pagos", "Caja",
            "Personas", "Usuarios", "Roles", "Permisos", "Configuracion"
        };

        var recursos = new[]
        {
            "Consultar", "Crear", "Editar", "Eliminar", "Imprimir", "Reporte", "CambiarEstado"
        };

        var permisos = new List<(string, string)>();
        foreach (var entidad in entidades)
        {
            foreach (var recurso in recursos)
            {
                permisos.Add((entidad, recurso));
            }
        }

        return permisos;
    }
}

