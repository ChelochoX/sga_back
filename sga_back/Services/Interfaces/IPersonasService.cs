﻿using sga_back.Request;

namespace sga_back.Services.Interfaces;

public interface IPersonasService
{
    Task<int> Insertar(PersonaRequest request);
    Task<int> Actualizar(int id, PersonaRequest request);
    Task<bool> Eliminar(int id);
}
