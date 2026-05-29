using ESCOLA_API.Security;
using ESCOLA_API.ViewModels;
using FluentValidation;

namespace ESCOLA_API.Validators
{
    public class RedefinirSenhaViewModelValidator : AbstractValidator<RedefinirSenhaViewModel>
    {
        public RedefinirSenhaViewModelValidator()
        {
            RuleFor(model => model.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O email e obrigatorio.")
                .EmailAddress().WithMessage("Informe um email valido.");

            RuleFor(model => model.Token)
                .NotEmpty().WithMessage("O token e obrigatorio.");

            RuleFor(model => model.NovaSenha)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("A nova senha e obrigatoria.")
                .MinimumLength(8).WithMessage("A nova senha deve ter no minimo 8 caracteres.")
                .Matches("[A-Z]").WithMessage("A nova senha deve conter uma letra maiuscula.")
                .Matches("[a-z]").WithMessage("A nova senha deve conter uma letra minuscula.")
                .Matches("[0-9]").WithMessage("A nova senha deve conter um numero.")
                .Matches("[^a-zA-Z0-9]").WithMessage("A nova senha deve conter um caractere especial.")
                .Must(senha => senha != DefaultPasswordPolicy.DefaultPassword)
                .WithMessage("A nova senha nao pode ser a senha padrao.");

            RuleFor(model => model.ConfirmacaoSenha)
                .Equal(model => model.NovaSenha)
                .WithMessage("A confirmacao da senha nao confere.");
        }
    }
}
