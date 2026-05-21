using FluentValidation.TestHelper;
using form_API.Security;
using form_API.Validators;
using form_API.ViewModels;

namespace form_API.Tests.Validators
{
    public class AlterarSenhaViewModelValidatorTests
    {
        private readonly AlterarSenhaViewModelValidator _validator = new();

        [Fact]
        public void Validate_WhenModelIsValid_ShouldNotHaveValidationErrors()
        {
            var model = new AlterarSenhaViewModel
            {
                SenhaAtual = DefaultPasswordPolicy.DefaultPassword,
                NovaSenha = "Senha@252526",
                ConfirmacaoSenha = "Senha@252526"
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenNewPasswordIsDefault_ShouldHaveValidationError()
        {
            var model = new AlterarSenhaViewModel
            {
                SenhaAtual = DefaultPasswordPolicy.DefaultPassword,
                NovaSenha = DefaultPasswordPolicy.DefaultPassword,
                ConfirmacaoSenha = DefaultPasswordPolicy.DefaultPassword
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(senha => senha.NovaSenha);
        }

        [Fact]
        public void Validate_WhenConfirmationDiffers_ShouldHaveValidationError()
        {
            var model = new AlterarSenhaViewModel
            {
                SenhaAtual = DefaultPasswordPolicy.DefaultPassword,
                NovaSenha = "Senha@252526",
                ConfirmacaoSenha = "Senha@252527"
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(senha => senha.ConfirmacaoSenha);
        }
    }
}
