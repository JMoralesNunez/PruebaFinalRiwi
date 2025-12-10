using Microsoft.EntityFrameworkCore;
using TalentoPlus.Api.Data.Models;
using TalentoPlus.Api.Data.Repositories.Interfaces;

namespace TalentoPlus.Api.Data.Repositories.Implementations;

public class DepartamentoRepository : IDepartamentoRepository
{
    private readonly ApplicationDbContext _context;

    public DepartamentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Departamento>> GetAllAsync()
    {
        return await _context.Departamentos.ToListAsync();
    }

    public async Task<Departamento?> GetByNombreAsync(string nombre)
    {
        return await _context.Departamentos
            .FirstOrDefaultAsync(d => d.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<Departamento> CreateAsync(Departamento departamento)
    {
        _context.Departamentos.Add(departamento);
        await _context.SaveChangesAsync();
        return departamento;
    }
}