using TalentoPlus.Api.Data.Models;
using TalentoPlus.Api.Data.Repositories.Interfaces;
using TalentoPlus.Api.Data.DTOs;
using TalentoPlus.Api.Services.Interfaces;


namespace TalentoPlus.Api.Services.Implementations;

public class EmpleadoService : IEmpleadoService
    {
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IDepartamentoRepository _departamentoRepo;

        public EmpleadoService(IEmpleadoRepository empleadoRepo, IDepartamentoRepository departamentoRepo)
        {
            _empleadoRepo = empleadoRepo;
            _departamentoRepo = departamentoRepo;
        }

        public async Task<Empleado> CreateEmpleadoAsync(EmpleadoCreateDto dto)
        {
            var existente = await _empleadoRepo.GetByDocumentoAsync(dto.Documento);
            if (existente != null)
                throw new Exception($"El empleado con documento {dto.Documento} ya existe.");
            
            var empleado = new Empleado
            {
                Documento = dto.Documento,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                FechaNacimiento = dto.FechaNacimiento,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Cargo = dto.Cargo,
                Salario = dto.Salario,
                FechaIngreso = dto.FechaIngreso,
                Estado = dto.Estado,
                NivelEducativo = dto.NivelEducativo,
                PerfilProfesional = dto.PerfilProfesional,
                DepartamentoId = dto.DepartamentoId
            };
            
            await _empleadoRepo.CreateAsync(empleado);
            return empleado;
        }

        public async Task<Empleado?> UpdateEmpleadoAsync(int id, EmpleadoUpdateDto dto)
        {
            var empleadoDb = await _empleadoRepo.GetByIdAsync(id);
            if (empleadoDb == null) return null;
            
            empleadoDb.Nombres = dto.Nombres;
            empleadoDb.Apellidos = dto.Apellidos;
            empleadoDb.FechaNacimiento = dto.FechaNacimiento;
            empleadoDb.Direccion = dto.Direccion;
            empleadoDb.Telefono = dto.Telefono;
            empleadoDb.Email = dto.Email;
            empleadoDb.Cargo = dto.Cargo;
            empleadoDb.Salario = dto.Salario;
            empleadoDb.FechaIngreso = dto.FechaIngreso;
            empleadoDb.Estado = dto.Estado;
            empleadoDb.NivelEducativo = dto.NivelEducativo;
            empleadoDb.PerfilProfesional = dto.PerfilProfesional;
            empleadoDb.DepartamentoId = dto.DepartamentoId;

            await _empleadoRepo.UpdateAsync(empleadoDb);
            return empleadoDb;
        }

        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            var empleado = await _empleadoRepo.GetByIdAsync(id);
            if (empleado == null) return false;

            await _empleadoRepo.DeleteAsync(id);
            return true;
        }
    }