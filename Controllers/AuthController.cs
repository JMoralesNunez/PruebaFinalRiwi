using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TalentoPlus.Api.Data.Models;
using TalentoPlus.Api.Data.Repositories.Interfaces;
using TalentoPlus.Api.Services;

namespace TalentoPlus.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly IEmpleadoRepository _empleadoRepo;
    private readonly IEmailService _emailService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config,
        IEmpleadoRepository empleadoRepo,
        IEmailService emailService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _empleadoRepo = empleadoRepo;
        _emailService = emailService;
        _roleManager = roleManager;
    }
    
    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var empleado = await _empleadoRepo.GetByDocumentoAsync(model.Documento);
        
        if (empleado == null)
        {
           return BadRequest("Tu documento no está registrado en la base de datos de empleados. Contacta a RRHH para importar Excel.");
        }

        var user = new IdentityUser
        {
            UserName = model.Documento,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Send email
            try 
            {
                await _emailService.SendEmailAsync(
                    model.Email, 
                    "Bienvenido a TalentoPlus", 
                    $"<h1>Hola {empleado.Nombres},</h1><p>Tu registro ha sido exitoso. Ya puedes ingresar a la plataforma.</p>"
                );
            }
            catch
            {
                // Log error de correo, pero no fallamos el registro
                Console.WriteLine("--- SMTP no configurado, modo consola ---");
                Console.WriteLine($"Para: {model.Email}");
                Console.WriteLine($"Asunto: Bienvenido a TalentoPlus");
                Console.WriteLine("Cuerpo:");
                Console.WriteLine("<h1>Hola {empleado.Nombres},</h1><p>Tu registro ha sido exitoso. Ya puedes ingresar a la plataforma.</p>");
                Console.WriteLine("-----------------------------------------");
            }
            await _userManager.AddToRoleAsync(user, "Empleado");
            return Ok(new { Mensaje = "Registro exitoso. Revisa tu correo." });
        }

        return BadRequest(result.Errors);
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // Buscamos por Documento (UserName)
        var user = await _userManager.FindByNameAsync(model.Documento);
        
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            // Buscar datos del empleado para meterlos en el token o devolverlos
            var empleado = await _empleadoRepo.GetByDocumentoAsync(model.Documento);
            
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("EmpleadoId", empleado?.Id.ToString() ?? "0") 
            };
            
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = new { 
                    user.UserName, 
                    user.Email, 
                    Nombre = empleado?.Nombres ?? "Usuario"
                }
            });
        }
        return Unauthorized("Usuario o contraseña incorrectos");
    }
}

// DTOs 
public class RegisterDto
{
    public string Documento { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginDto
{
    public string Documento { get; set; }
    public string Password { get; set; }
}

