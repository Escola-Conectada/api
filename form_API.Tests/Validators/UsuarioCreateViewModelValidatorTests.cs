using FluentValidation.TestHelper;
using form_API.Validators;
using form_API.ViewModels;

namespace form_API.Tests.Validators
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
                Senha = "Senha@123",
                IdPerfil = 2
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
            result.ShouldHaveValidationErrorFor(usuario => usuario.Senha);
            result.ShouldHaveValidationErrorFor(usuario => usuario.IdPerfil);
        }

        [Fact]
        public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
        {
            var model = new UsuarioCreateViewModel
            {
                Nome = "Usuario Teste",
                Email = "email-invalido",
                Telefone = "11999990000",
                Senha = "Senha@123",
                IdPerfil = 2
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(usuario => usuario.Email);
        }
    }
}
