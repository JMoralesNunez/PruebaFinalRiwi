using Microsoft.EntityFrameworkCore;
using TalentoPlus.Api.Data.Models;
using TalentoPlus.Api.Data.Repositories.Interfaces;

namespace TalentoPlus.Api.Data.Repositories.Implementations;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly ApplicationDbContext _context;

    public EmpleadoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        // Incluimos el Departamento para que no venga null
        return await _context.Empleados
            .Include(e => e.Departamento) 
            .ToListAsync();
    }

    public async Task<Empleado?> GetByIdAsync(int id)
    {
        return await _context.Empleados
            .Include(e => e.Departamento)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Empleado?> GetByDocumentoAsync(string documento)
    {
        return await _context.Empleados
            .Include(e => e.Departamento)
            .FirstOrDefaultAsync(e => e.Documento == documento);
    }

    public async Task CreateAsync(Empleado empleado)
    {
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);
        if (empleado != null)
        {
            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> CountAsync()
    {
        return await _context.Empleados.CountAsync();
    }

    public async Task<int> CountByEstadoAsync(string estado)
    {
        return await _context.Empleados
            .CountAsync(e => e.Estado == estado);
    }

    public async Task<Dictionary<string, int>> GetEmployeesbyDepartmentAsync()
    {
        return await _context.Empleados
            .Where(e => e.Departamento != null)
            .GroupBy(e => e.Departamento!.Nombre)
            .Select(g => new { Departamento = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Departamento, x => x.Cantidad);
    }
}