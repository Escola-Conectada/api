using System.Threading.Tasks;
using ESCOLA_API.Controllers;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ESCOLA_API.Tests.Controllers
{
    public class UsuariosControllerTests
    {
        [Fact]
        public async Task Post_WhenUsuarioIsValid_ReturnsCreatedAtAction()
        {
            var model = new UsuarioCreateViewModel
            {
                Nome = "Usuario Novo",
                Email = "novo@escola.com",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Professor
            };

            var expected = new UsuarioSummaryViewModel
            {
                IdUsuario = 21,
                Nome = model.Nome,
                Email = model.Email,
                Telefone = model.Telefone,
                IdPerfil = PerfilSistema.ProfessorId,
                DescricaoPerfil = PerfilSistema.Professor,
                TipoUsuario = PerfilSistema.Professor
            };

            var service = new Mock<IUsuarioService>();
            service.Setup(s => s.AddAsync(model, It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(expected);

            var logger = new Mock<ILogger<UsuariosController>>();
            var controller = new UsuariosController(service.Object, logger.Object);

            var actionResult = await controller.Post(model);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            var value = Assert.IsType<UsuarioSummaryViewModel>(createdResult.Value!);

            Assert.Equal(expected.IdUsuario, value.IdUsuario);
            Assert.Equal(expected.Email, value.Email);
        }
    }
}
