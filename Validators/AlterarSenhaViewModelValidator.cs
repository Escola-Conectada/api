using FluentValidation;
using form_API.Security;
using form_API.ViewModels;

namespace form_API.Validators
{
    public class AlterarSenhaViewModelValidator : AbstractValidator<AlterarSenhaViewModel>
    {
        public AlterarSenhaViewModelValidator()
        {
            RuleFor(model => model.SenhaAtual)
                .NotEmpty().WithMessage("A senha atual e obrigatoria.");

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
