﻿using sga_back.DTOs;

namespace sga_back.Request;

public class AsignarPermisosRequest
{
    public int IdRol { get; set; }
    public List<PermisoDto> Permisos { get; set; }
}
