﻿using sga_back.Models;
using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IUsuariosService
{
    Task<int> Insertar(UsuarioRequest request);
    Task<bool> ActualizarUsuario(UsuarioUpdateRequest request, int idUsuario);
    Task<Usuario?> ValidarCredenciales(string usuario, string contrasena);
    Task<bool> CambiarContrasena(int idUsuario, string nuevaContrasena);
    Task<(IEnumerable<Usuario>, int)> ObtenerUsuarios(string? filtro, int pageNumber, int pageSize);
    Task Actualizar(UsuarioNameUpdateRequest request);
    Task<bool> CambiarEstadoUsuario(int idUsuario);
}
