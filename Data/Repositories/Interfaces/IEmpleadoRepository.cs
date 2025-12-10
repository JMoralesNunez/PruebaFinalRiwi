using TalentoPlus.Api.Data.Models;

namespace TalentoPlus.Api.Data.Repositories.Interfaces;

public interface IEmpleadoRepository
{
    Task<IEnumerable<Empleado>> GetAllAsync();
    Task<Empleado?> GetByIdAsync(int id);
    Task<Empleado?> GetByDocumentoAsync(string documento);
    Task CreateAsync(Empleado empleado);
    Task UpdateAsync(Empleado empleado);
    Task DeleteAsync(int id);
    Task<int> CountAsync();
    Task<int> CountByEstadoAsync(string estado);
    Task<Dictionary<string, int>> GetEmployeesbyDepartmentAsync();
}