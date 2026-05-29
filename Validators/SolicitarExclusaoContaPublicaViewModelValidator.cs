using ESCOLA_API.ViewModels;
using FluentValidation;

namespace ESCOLA_API.Validators
{
    public class SolicitarExclusaoContaPublicaViewModelValidator : AbstractValidator<SolicitarExclusaoContaPublicaViewModel>
    {
        public SolicitarExclusaoContaPublicaViewModelValidator()
        {
            RuleFor(model => model.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O email e obrigatorio.")
                .EmailAddress().WithMessage("Informe um email valido.");

            RuleFor(model => model.Motivo)
                .MaximumLength(500)
                .WithMessage("O motivo deve ter no maximo 500 caracteres.");
        }
    }
}
