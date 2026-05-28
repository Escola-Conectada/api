using FluentValidation.TestHelper;
using ESCOLA_API.Models;
using ESCOLA_API.Validators;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Tests.Validators
{
    public class UsuarioCreateViewModelValidatorTests
    {
        private readonly UsuarioCreateViewModelValidator _validator = new();

        [Fact]
        public void Validate_WhenModelIsValid_ShouldNotHaveValidationErrors()
        {
            var model = new UsuarioCreateViewModel
            {
                Nome = "Usuario Teste",
                Email = "usuario@escola.com",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Professor
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenRequiredFieldsAreMissing_ShouldHaveValidationErrors()
        {
            var model = new UsuarioCreateViewModel();

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(usuario => usuario.Nome);
            result.ShouldHaveValidationErrorFor(usuario => usuario.Email);
            result.ShouldHaveValidationErrorFor(usuario => usuario.Telefone);
            result.ShouldHaveValidationErrorFor(usuario => usuario.TipoUsuario);
        }

        [Fact]
        public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
        {
            var model = new UsuarioCreateViewModel
            {
                Nome = "Usuario Teste",
                Email = "email-invalido",
                Telefone = "11999990000",
                TipoUsuario = PerfilSistema.Professor
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(usuario => usuario.Email);
        }

        [Fact]
        public void Validate_WhenDataNascimentoIsFuture_ShouldHaveValidationError()
        {
            var model = new UsuarioCreateViewModel
            {
                Nome = "Usuario Teste",
                Email = "usuario@escola.com",
                Telefone = "11999990000",
                DataNascimento = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(1),
                TipoUsuario = PerfilSistema.Professor
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(usuario => usuario.DataNascimento);
        }
    }
}
