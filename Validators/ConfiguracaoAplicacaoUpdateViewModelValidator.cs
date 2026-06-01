using ESCOLA_API.ViewModels;
using FluentValidation;

namespace ESCOLA_API.Validators
{
    public class ConfiguracaoAplicacaoUpdateViewModelValidator : AbstractValidator<ConfiguracaoAplicacaoUpdateViewModel>
    {
        public ConfiguracaoAplicacaoUpdateViewModelValidator()
        {
            RuleFor(model => model.NomeEscola)
                .NotEmpty()
                .WithMessage("Informe o nome da escola.")
                .MaximumLength(120)
                .WithMessage("O nome da escola deve ter ate 120 caracteres.");
        }
    }
}
