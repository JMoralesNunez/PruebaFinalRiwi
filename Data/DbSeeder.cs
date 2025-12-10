using Microsoft.AspNetCore.Identity;

namespace TalentoPlus.Api.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        // Obtener los servicios necesarios del contenedor de inyección de dependencias
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Crear Roles (Admin y Empleado)
        string[] roleNames = { "Admin", "Empleado" };
            
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Crear Usuario Administrador por defecto
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            var newAdmin = new IdentityUser
            {
                UserName = "admin",
                Email = "admin@talentoplus.com",
                EmailConfirmed = true
            };
                
            var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin123!");
                
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
}