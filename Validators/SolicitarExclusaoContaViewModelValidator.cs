using ESCOLA_API.ViewModels;
using FluentValidation;

namespace ESCOLA_API.Validators
{
    public class SolicitarExclusaoContaViewModelValidator : AbstractValidator<SolicitarExclusaoContaViewModel>
    {
        public SolicitarExclusaoContaViewModelValidator()
        {
            RuleFor(model => model.Confirmacao)
                .Equal(true)
                .WithMessage("Confirme que deseja solicitar a exclusao da conta.");

            RuleFor(model => model.Motivo)
                .MaximumLength(500)
                .WithMessage("O motivo deve ter no maximo 500 caracteres.");
        }
    }
}
