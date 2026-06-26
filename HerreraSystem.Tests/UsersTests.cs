using HerreraSystem.Application.DTOs.UserDto;
using HerreraSystem.Application.Services;
using HerreraSystem.Infrastructure.Data;
using HerreraSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace HerreraSystem.Tests
{
    [TestFixture]
    public class UsersTests
    {
        private HerreraSystemContext _context;
        private UserService _service;
        private CreateUserDto _createUser;
        private CreateUserDto _nuevoUser;

        [SetUp]
        public void SetUp()
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
                UserName = "rOMINA",
                Email = "rominanarte12@gmail.com",
                IdNumber = "0412210061200F",
                FirstName = "Romina",
                LastName = "Herrera",
                Password = "Test123*",
                RoleName = "Administrador"
            };

            _nuevoUser = new CreateUserDto
            {
                UserName = "UsuarioPrueba",
                Email = "prueba@gmail.com",
                IdNumber = "00000000000000",
                FirstName = "Prueba",
                LastName = "Prueba",
                Password = "Test123*",
                RoleName = "Administrador"
            };
        }

        [TearDown]
        public async Task TearDown()
        {
            var usuario = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserName == _nuevoUser.UserName);

            if (usuario != null)
            {
                if (usuario.UserRoles.Any())
                {
                    _context.UserRoles.RemoveRange(usuario.UserRoles);
                }
                _context.Users.Remove(usuario);
                await _context.SaveChangesAsync();
            }

            await _context.DisposeAsync();
        }

        [Test]
        public async Task UserService_CreateAsync_DebeCrearUsuarioYAsignarRolCorrectamente()
        {
            var resultado = await _service.CreateAsync(_nuevoUser);

            var usuarioDespues = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == _nuevoUser.UserName);

            Assert.That(resultado.Success, Is.True);
            Assert.That(usuarioDespues, Is.Not.Null);
            Assert.That(usuarioDespues!.Email, Is.EqualTo(_nuevoUser.Email));
            Assert.That(usuarioDespues.IdNumber, Is.EqualTo(_nuevoUser.IdNumber));
            Assert.That(usuarioDespues.UserRoles.Any(ur => ur.Role.RoleName == _nuevoUser.RoleName), Is.True);
        }

        [Test]
        public async Task UserService_CreateAsync_DebeFallarSiElUserNameYaExiste()
        {
            var usuarioDuplicado = new CreateUserDto
            {
                UserName = _createUser.UserName,
                Email = "otro@gmail.com",
                IdNumber = "12345678901234",
                FirstName = "Otro",
                LastName = "Usuario",
                Password = "Test123*",
                RoleName = "Administrador"
            };

            var resultado = await _service.CreateAsync(usuarioDuplicado);

            Assert.That(resultado.Success, Is.False);
            Assert.That(resultado.ErrorMessage, Does.Contain("nombre de usuario").IgnoreCase);
        }

        [Test]
        public async Task UserService_CreateAsync_DebeFallarSiElEmailYaExiste()
        {
            var usuarioDuplicado = new CreateUserDto
            {
                UserName = "otro_username",
                Email = _createUser.Email,
                IdNumber = "12345678901234",
                FirstName = "Otro",
                LastName = "Usuario",
                Password = "Test123*",
                RoleName = "Administrador"
            };

            var resultado = await _service.CreateAsync(usuarioDuplicado);

            Assert.That(resultado.Success, Is.False);
            Assert.That(resultado.ErrorMessage, Does.Contain("correo electrónico").IgnoreCase);
        }

        [Test]
        public async Task UserService_CreateAsync_DebeFallarSiLaCedulaYaExiste()
        {
            var usuarioDuplicado = new CreateUserDto
            {
                UserName = "otro_username",
                Email = "otro@gmail.com",
                IdNumber = _createUser.IdNumber,
                FirstName = "Otro",
                LastName = "Usuario",
                Password = "Test123*",
                RoleName = "Administrador"
            };

            var resultado = await _service.CreateAsync(usuarioDuplicado);

            Assert.That(resultado.Success, Is.False);
            Assert.That(resultado.ErrorMessage, Does.Contain("cédula").IgnoreCase);
        }
    }
}