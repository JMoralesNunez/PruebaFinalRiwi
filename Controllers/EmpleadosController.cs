using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentoPlus.Api.Data.DTOs;
using TalentoPlus.Api.Data.Models;
using TalentoPlus.Api.Data.Repositories.Interfaces;
using TalentoPlus.Api.Services.Interfaces;

namespace TalentoPlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IDepartamentoRepository _departamentoRepo;
        private readonly IEmpleadoService _empleadoService;

        public EmpleadosController(
            IEmpleadoRepository empleadoRepo, 
            IDepartamentoRepository departamentoRepo, 
            IEmpleadoService empleadoService)
        {
            _empleadoRepo = empleadoRepo;
            _departamentoRepo = departamentoRepo;
            _empleadoService = empleadoService;
        }

        // --- SECCIÓN 1: IMPORTACIÓN EXCEL (Solo Admin) ---
        [HttpPost("Import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor suba un archivo Excel válido.");

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                int procesados = 0;

                foreach (var row in rows)
                {
                    var documento = row.Cell(1).GetValue<string>();
                    var nombres = row.Cell(2).GetValue<string>();
                    var apellidos = row.Cell(3).GetValue<string>();
                    DateTime.TryParse(row.Cell(4).GetValue<string>(), out DateTime fechaNacimiento);
                    var direccion = row.Cell(5).GetValue<string>();
                    var telefono = row.Cell(6).GetValue<string>();
                    var email = row.Cell(7).GetValue<string>();
                    var cargo = row.Cell(8).GetValue<string>();
                    decimal.TryParse(row.Cell(9).GetValue<string>(), out decimal salario);
                    DateTime.TryParse(row.Cell(10).GetValue<string>(), out DateTime fechaIngreso);
                    var estado = row.Cell(11).GetValue<string>();
                    var nivelEducativo = row.Cell(12).GetValue<string>();
                    var perfil = row.Cell(13).GetValue<string>();
                    var nombreDepartamento = row.Cell(14).GetValue<string>();

                    if (string.IsNullOrWhiteSpace(documento)) continue;

                    // Gestión de Departamento
                    var departamento = await _departamentoRepo.GetByNombreAsync(nombreDepartamento);
                    if (departamento == null)
                    {
                        departamento = new Departamento { Nombre = nombreDepartamento };
                        await _departamentoRepo.CreateAsync(departamento);
                    }

                    // Gestión de Empleado (Upsert manual sin DTO por complejidad del Excel)
                    var empleadoExistente = await _empleadoRepo.GetByDocumentoAsync(documento);

                    if (empleadoExistente != null)
                    {
                        empleadoExistente.Nombres = nombres;
                        empleadoExistente.Apellidos = apellidos;
                        empleadoExistente.FechaNacimiento = fechaNacimiento;
                        empleadoExistente.Direccion = direccion;
                        empleadoExistente.Telefono = telefono;
                        empleadoExistente.Email = email;
                        empleadoExistente.Cargo = cargo;
                        empleadoExistente.Salario = salario;
                        empleadoExistente.FechaIngreso = fechaIngreso;
                        empleadoExistente.Estado = estado;
                        empleadoExistente.NivelEducativo = nivelEducativo;
                        empleadoExistente.PerfilProfesional = perfil;
                        empleadoExistente.DepartamentoId = departamento.Id;

                        await _empleadoRepo.UpdateAsync(empleadoExistente);
                    }
                    else
                    {
                        var nuevoEmpleado = new Empleado
                        {
                            Documento = documento,
                            Nombres = nombres,
                            Apellidos = apellidos,
                            FechaNacimiento = fechaNacimiento,
                            Direccion = direccion,
                            Telefono = telefono,
                            Email = email,
                            Cargo = cargo,
                            Salario = salario,
                            FechaIngreso = fechaIngreso,
                            Estado = estado,
                            NivelEducativo = nivelEducativo,
                            PerfilProfesional = perfil,
                            DepartamentoId = departamento.Id
                        };
                        await _empleadoRepo.CreateAsync(nuevoEmpleado);
                    }
                    procesados++;
                }

                return Ok(new { Mensaje = "Carga exitosa", EmpleadosProcesados = procesados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error procesando el archivo: {ex.Message}");
            }
        }

        // --- SECCIÓN 2: CRUD DE EMPLEADOS (Solo Admin) ---

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmpleados()
        {
            return Ok(await _empleadoRepo.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmpleadoById(int id)
        {
            var empleado = await _empleadoRepo.GetByIdAsync(id);
            if (empleado == null) return NotFound("Empleado no encontrado.");
            return Ok(empleado);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEmpleado([FromBody] EmpleadoCreateDto dto)
        {
            try
            {
                var nuevoEmpleado = await _empleadoService.CreateEmpleadoAsync(dto);
                return CreatedAtAction(nameof(GetEmpleadoById), new { id = nuevoEmpleado.Id }, nuevoEmpleado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmpleado(int id, [FromBody] EmpleadoUpdateDto dto)
        {
            try 
            {
                var actualizado = await _empleadoService.UpdateEmpleadoAsync(id, dto);
                if (actualizado == null) return NotFound("Empleado no encontrado.");
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            var borrado = await _empleadoService.DeleteEmpleadoAsync(id);
            if (!borrado) return NotFound();
            return NoContent();
        }

        // --- SECCIÓN 3: PDF (Hoja de Vida) ---

        // Endpoint Admin: Descargar de cualquiera por ID
        [HttpGet("{id}/CV")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadCV(int id)
        {
            return await GenerateInternalPdf(id);
        }

        // Endpoint Empleado: 
        [HttpGet("Me/CV")]
        public async Task<IActionResult> DescargarMiHojaDeVida()
        {
            var empleadoIdClaim = User.FindFirst("EmpleadoId")?.Value;
            if (string.IsNullOrEmpty(empleadoIdClaim) || empleadoIdClaim == "0")
                return BadRequest("No tienes un perfil de empleado asociado.");

            int id = int.Parse(empleadoIdClaim);
            return await GenerateInternalPdf(id);
        }

        // --- SECCIÓN 4: INFO PROPIA (Empleado) ---

        [HttpGet("Me")]
        public async Task<IActionResult> GetMyInfo()
        {
            var empleadoIdClaim = User.FindFirst("EmpleadoId")?.Value;
            if (string.IsNullOrEmpty(empleadoIdClaim) || empleadoIdClaim == "0")
                return BadRequest("No tienes un perfil de empleado asociado.");

            int id = int.Parse(empleadoIdClaim);
            var empleado = await _empleadoRepo.GetByIdAsync(id);
            if (empleado == null) return NotFound();
            return Ok(empleado);
        }

        // --- MÉTODO PRIVADO REUTILIZABLE PARA PDF ---
        private async Task<IActionResult> GenerateInternalPdf(int id)
        {
            var empleado = await _empleadoRepo.GetByIdAsync(id);
            if (empleado == null) return NotFound("Empleado no encontrado.");

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"{empleado.Nombres} {empleado.Apellidos}")
                                .FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text(empleado.Cargo.ToUpper())
                                .FontSize(14).SemiBold().FontColor(Colors.Grey.Darken2);
                        });
                        row.ConstantItem(100).AlignRight().Text("TalentoPlus")
                            .FontSize(10).FontColor(Colors.Grey.Lighten1);
                    });

                    // Contenido
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text("Perfil Profesional").FontSize(14).Bold().Underline();
                        col.Item().PaddingTop(5).Text(empleado.PerfilProfesional).Justify();
                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(innerCol =>
                            {
                                innerCol.Item().Text("Información Laboral").FontSize(12).Bold();
                                innerCol.Item().PaddingTop(5).Text($"Departamento: {empleado.Departamento?.Nombre ?? "Sin asignar"}");
                                innerCol.Item().Text($"Fecha de Ingreso: {empleado.FechaIngreso:dd/MM/yyyy}");
                                innerCol.Item().Text($"Estado Actual: {empleado.Estado}");
                                innerCol.Item().Text($"Salario: ${empleado.Salario:N0}");
                            });
                            row.RelativeItem().Column(innerCol =>
                            {
                                innerCol.Item().Text("Datos de Contacto").FontSize(12).Bold();
                                innerCol.Item().PaddingTop(5).Text($"Email: {empleado.Email}");
                                innerCol.Item().Text($"Teléfono: {empleado.Telefono}");
                                innerCol.Item().Text($"Dirección: {empleado.Direccion}");
                                innerCol.Item().Text($"Nivel Educativo: {empleado.NivelEducativo}");
                            });
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado automáticamente por el sistema TalentoPlus - ");
                        x.CurrentPageNumber();
                    });
                });
            });

            byte[] pdfBytes = documento.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"HojaVida_{empleado.Documento}.pdf");
        }
    }
}