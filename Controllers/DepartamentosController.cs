using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Api.Data.Repositories.Interfaces;

namespace TalentoPlus.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartamentosController : ControllerBase
{
    private readonly IDepartamentoRepository _repo;

    public DepartamentosController(IDepartamentoRepository repo)
    {
        _repo = repo;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetDepartamentos()
    {
        var deptos = await _repo.GetAllAsync();
        return Ok(deptos);
    }
}