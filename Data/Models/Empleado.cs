using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentoPlus.Api.Data.Models;

public class Empleado
{
    [Key]
    public int Id { get; set; }

    // --- Columnas del Excel ---
    [Required]
    public string Documento { get; set; } = string.Empty; // Servirá para Login y búsqueda
        
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; // Para notificaciones
    public string Cargo { get; set; } = string.Empty;
        
    [Column(TypeName = "decimal(18,2)")]
    public decimal Salario { get; set; }
        
    public DateTime FechaIngreso { get; set; }
    public string Estado { get; set; } = "Activo"; 
    public string NivelEducativo { get; set; } = string.Empty;
    public string PerfilProfesional { get; set; } = string.Empty;

    // Relación con Departamento
    // En el Excel viene el nombre, pero en BD guardamos el ID
    public int DepartamentoId { get; set; }
    public Departamento? Departamento { get; set; }

    // Campo auxiliar para Login (Contraseña hasheada, si aplica)
    // O usaremos Identity User aparte, pero por simplicidad de la prueba, 
    // a veces se vincula aquí. Por ahora dejémoslo limpio.
}