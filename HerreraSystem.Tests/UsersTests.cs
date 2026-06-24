using HerreraSystem.Application.DTOs.UserDto;
using HerreraSystem.Application.Services;
using HerreraSystem.Infrastructure.Data;
using HerreraSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;

namespace HerreraSystem.Tests
{
    [TestFixture]
    public class UsersTests
    {
        private HerreraSystemContext _context;
        private UserService _service;
        private CreateUserDto _createUser;

        [SetUp]
        public async Task SetUp()
        {
            var connectionString = "Server=DESKTOP-VSK4022\\SQLEXPRESS01;" +
                                   "Database=HerreraSystem;" +
                                   "Trusted_Connection=True;" +
                                   "TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<HerreraSystemContext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new HerreraSystemContext(options);
            var repository = new UserRepository(_context);
            _service = new UserService(repository);

            _createUser = new CreateUserDto
            {
                UserName = "fHerrera_svc",
                Email = "fabian_svc@sorbeteria.com",
                IdNumber = "041221006100F",
                FirstName = "Fabian",
                LastName = "Herrera",
                Password = "Password123!",
                RoleName = "Administrador"
            };

            var usuarioExistente = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _createUser.UserName || u.Email == _createUser.Email);

            if (usuarioExistente != null)
            {
                _context.Users.Remove(usuarioExistente);
                await _context.SaveChangesAsync();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        [Order(1)]
        public async Task UserService_CreateAsync_DebeCrearUsuarioYAsignarRolCorrectamente()
        {
            var rolExiste = await _context.Roles
                .AnyAsync(r => r.RoleName == _createUser.RoleName);

            Assert.That(rolExiste, Is.True, $"El rol '{_createUser.RoleName}' no existe en la base de datos.");

            var usuarioAntes = await _context.Users
                .AnyAsync(u => u.UserName == _createUser.UserName || u.Email == _createUser.Email);

            Assert.That(usuarioAntes, Is.False, "Ya existe un usuario con ese UserName o Email antes de ejecutar CreateAsync.");

            var resultado = await _service.CreateAsync(_createUser);

            var usuarioDespues = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _createUser.UserName);

            Assert.That(
                resultado.Success,
                Is.True,
                $"CreateAsync devolvió false. Data: '{resultado.Data ?? "NULL"}'. Usuario insertado en BD: {(usuarioDespues != null ? "Sí" : "No")}"
            );

            Assert.That(usuarioDespues, Is.Not.Null, "El usuario no fue insertado en la base de datos.");
            Assert.That(usuarioDespues!.Email, Is.EqualTo(_createUser.Email));
        }

        [Test]
        [Order(2)]
        public async Task UserService_CleanUp_DebeEliminarUsuarioDePrueba()
        {
            var usuario = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _createUser.UserName);

            if (usuario != null)
            {
                _context.Users.Remove(usuario);
                await _context.SaveChangesAsync();
            }

            var usuarioBorrado = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _createUser.UserName);

            Assert.That(usuarioBorrado, Is.Null);
        }
    }
}