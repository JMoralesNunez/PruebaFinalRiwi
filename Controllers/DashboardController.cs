using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Api.Data.Repositories.Interfaces;
using System.Text;
using System.Text.Json;
using TalentoPlus.Api.Data.Models;

namespace TalentoPlus.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
private readonly IEmpleadoRepository _empleadoRepo;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public DashboardController(IEmpleadoRepository empleadoRepo, IConfiguration config)
    {
        _empleadoRepo = empleadoRepo;
        _config = config;
        _httpClient = new HttpClient();
    }
    
    //dashboard cards
    [HttpGet("Kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var totalEmpleados = await _empleadoRepo.CountAsync();
        var enVacaciones = await _empleadoRepo.CountByEstadoAsync("Vacaciones");
        var porDepartamento = await _empleadoRepo.GetEmployeesbyDepartmentAsync();

        return Ok(new
        {
            TotalEmpleados = totalEmpleados,
            EnVacaciones = enVacaciones,
            EmpleadosPorDepartamento = porDepartamento
        });
    }

    // IA interaction
    [HttpPost("ia-query")]
    public async Task<IActionResult> ConsultarIA([FromBody] questionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("La pregunta no puede estar vacía.");
        
        var empleados = await _empleadoRepo.GetAllAsync();
        
        var contextData = empleados.Select(e => new 
        {
            e.Nombres,
            e.Cargo,
            Departamento = e.Departamento?.Nombre ?? "Sin Asignar",
            e.Estado,
            Salario = (int)e.Salario, 
            e.NivelEducativo
        });

        string jsonDatos = JsonSerializer.Serialize(contextData);

        // B. Construimos el Prompt para Gemini
        var prompt = $@"
            Eres un analista de recursos humanos experto. Tienes los siguientes datos de empleados en formato JSON:
            {jsonDatos}

            Usuario pregunta: ""{request.Question}""

            Instrucciones:
            1. Responde basándote ÚNICAMENTE en los datos JSON proporcionados.
            2. Sé breve y directo.
            3. Si la respuesta es un número, da el número. Si es una lista, da los nombres.
            4. Si te preguntan algo que no está en los datos, di que no tienes esa información.
        ";

        // C. Llamamos a la API de Gemini
        var respuestaIA = await CallGeminiApi(prompt);

        return Ok(new { Respuesta = respuestaIA });
    }

    // Método auxiliar para llamar a Google
    private async Task<string> CallGeminiApi(string prompt)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        try 
        {
            var response = await _httpClient.PostAsync(url, jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"ERROR DE GOOGLE ({response.StatusCode}): {responseString}";

            // Parseamos la respuesta compleja de Google para sacar solo el texto
            using var doc = JsonDocument.Parse(responseString);
            var texto = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return texto ?? "Sin respuesta.";
        }
        catch
        {
            return "Error interno al procesar la IA.";
        }
    }
    [HttpGet("test-models")]
    public async Task<IActionResult> ListModels()
    {
        var apiKey = _config["Gemini:ApiKey"];
        // Esta URL consulta la lista de modelos disponibles para tu cuenta
        var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

        try 
        {
            var response = await _httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();
        
            // Retornamos la respuesta cruda de Google para ver qué nombres nos da
            return Content(jsonString, "application/json");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }
}

// DTO simple para recibir la pregunta
public class questionRequest
{
    public string Question { get; set; } = string.Empty;
}
