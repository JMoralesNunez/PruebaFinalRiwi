using TalentoPlus.Api.Data.Models;

namespace TalentoPlus.Api.Data.Repositories.Interfaces;

public interface IDepartamentoRepository
{
    Task<IEnumerable<Departamento>> GetAllAsync();
    Task<Departamento?> GetByNombreAsync(string nombre);
    Task<Departamento> CreateAsync(Departamento departamento);
}