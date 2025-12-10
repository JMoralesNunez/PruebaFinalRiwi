using System.ComponentModel.DataAnnotations;

namespace TalentoPlus.Api.Data.DTOs;

public class EmpleadoCreateDto
{
    [Required] public string Documento { get; set; } = string.Empty;
    [Required] public string Nombres { get; set; } = string.Empty;
    [Required] public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string Estado { get; set; } = "Activo";
    public string NivelEducativo { get; set; } = string.Empty;
    public string PerfilProfesional { get; set; } = string.Empty;
    [Required]
    public int DepartamentoId { get; set; } 
}