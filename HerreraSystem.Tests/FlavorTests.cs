using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.FlavorDtos;
using HerreraSystem.Infrastructure.Data;
using HerreraSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace HerreraSystem.Tests
{
    [TestFixture]
    public class FlavorTests
    {
        private DbContextOptions<HerreraSystemContext> _options;
        private CreateFlavorDto _createFlavor1;
        private CreateFlavorDto _createFlavor2;

        [SetUp]
        public void SetUp()
        {
            _options = new DbContextOptionsBuilder<HerreraSystemContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _createFlavor1 = new CreateFlavorDto
            {
                FlavorName = "Fresa",
                ImageUrl = "https://servidor.com/fresa.png",
                FlavorColor = "#FF0000"
            };

            _createFlavor2 = new CreateFlavorDto
            {
                FlavorName = "Chocolate",
                ImageUrl = "https://servidor.com/chocolate.png",
                FlavorColor = "#3B220E"
            };
        }

        [Test]
        [Order(1)]
        public async Task FlavorRepository_CreateAsync_DebeCrearFlavorCorrectamente()
        {
            var context = new HerreraSystemContext(_options);
            var repository = new FlavorRepository(context);

            FlavorDto resultado = await repository.CreateAsync(_createFlavor1);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.FlavorName, Is.EqualTo(_createFlavor1.FlavorName));
            Assert.That(resultado.ImageUrl, Is.EqualTo(_createFlavor1.ImageUrl));
            Assert.That(resultado.FlavorColor, Is.EqualTo(_createFlavor1.FlavorColor));
            Assert.That(resultado.IsActive, Is.EqualTo(true));
        }

       

        [Test]
        [Order(3)]
        public async Task FlavorRepository_GetByIdAsync_DebeRetornarFlavorExistente()
        {
            var context = new HerreraSystemContext(_options);
            var repository = new FlavorRepository(context);

            var creado = await repository.CreateAsync(_createFlavor1);

            var obtenido = await repository.GetByIdAsync(creado.Id);

            Assert.That(obtenido, Is.Not.Null);
            Assert.That(obtenido!.Id, Is.EqualTo(creado.Id));
            Assert.That(obtenido.FlavorName, Is.EqualTo("Fresa"));
            Assert.That(obtenido.FlavorColor, Is.EqualTo("#FF0000"));
        }

        [Test]
        [Order(4)]
        public async Task FlavorRepository_UpdateAsync_DebeActualizarFlavorCorrectamente()
        {
            var context = new HerreraSystemContext(_options);
            var repository = new FlavorRepository(context);

            var creado = await repository.CreateAsync(_createFlavor1);

            var updateDto = new UpdateFlavorDto
            {
                FlavorName = "Fresa Premium",
                IsActive = true,
                ImageUrl = "https://servidor.com/fresa-premium.png",
                FlavorColor = "#FF6699"
            };

            var actualizado = await repository.UpdateAsync(creado.Id, updateDto);
            var flavorActualizado = await repository.GetByIdAsync(creado.Id);

            Assert.That(actualizado, Is.True);
            Assert.That(flavorActualizado, Is.Not.Null);
            Assert.That(flavorActualizado!.FlavorName, Is.EqualTo("Fresa Premium"));
            Assert.That(flavorActualizado.ImageUrl, Is.EqualTo("https://servidor.com/fresa-premium.png"));
            Assert.That(flavorActualizado.FlavorColor, Is.EqualTo("#FF6699"));
            Assert.That(flavorActualizado.IsActive, Is.EqualTo(true));
        }

        [Test]
        [Order(5)]
        public async Task FlavorRepository_DeleteAsync_DebeEliminarFlavorCorrectamente()
        {
            var context = new HerreraSystemContext(_options);
            var repository = new FlavorRepository(context);

            var creado = await repository.CreateAsync(_createFlavor1);

            var eliminado = await repository.DeleteAsync(creado.Id);
            var buscado = await repository.GetByIdAsync(creado.Id);

            Assert.That(eliminado, Is.True);
            Assert.That(buscado, Is.Null);
        }
    }
}