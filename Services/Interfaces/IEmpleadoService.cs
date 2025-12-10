using TalentoPlus.Api.Data.Models;
using TalentoPlus.Api.Data.DTOs;

namespace TalentoPlus.Api.Services.Interfaces;


public interface IEmpleadoService
{
    Task<Empleado> CreateEmpleadoAsync(EmpleadoCreateDto dto);
    Task<Empleado?> UpdateEmpleadoAsync(int id, EmpleadoUpdateDto dto);
    Task<bool> DeleteEmpleadoAsync(int id);
}