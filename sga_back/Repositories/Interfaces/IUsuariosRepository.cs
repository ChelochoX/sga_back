﻿using sga_back.Models;

namespace sga_back.Repositories.Interfaces;

public interface IUsuariosRepository
{
    Task<int> Insertar(Usuario usuario);
    Task<bool> Eliminar(int id);
    Task<bool> ExisteNombreUsuario(string nombreUsuario);
    Task<bool> ActualizarUsuario(int idUsuario, string nombreUsuario, string nuevaContrasena);
}
