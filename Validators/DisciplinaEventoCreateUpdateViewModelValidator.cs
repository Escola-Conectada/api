using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;
using FluentValidation;

namespace ESCOLA_API.Validators
{
    public class DisciplinaEventoCreateUpdateViewModelValidator : AbstractValidator<DisciplinaEventoCreateUpdateViewModel>
    {
        public DisciplinaEventoCreateUpdateViewModelValidator()
        {
            RuleFor(evento => evento.Tipo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O tipo do evento e obrigatorio.")
                .Must(TipoEventoDisciplina.TipoValido)
                .WithMessage("Informe um tipo valido: Avaliacao ou Trabalho.");

            RuleFor(evento => evento.Titulo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O titulo do evento e obrigatorio.")
                .MaximumLength(120).WithMessage("O titulo do evento deve ter no maximo 120 caracteres.");

            RuleFor(evento => evento.Descricao)
                .MaximumLength(500).WithMessage("A descricao do evento deve ter no maximo 500 caracteres.");

            RuleFor(evento => evento.Data)
                .Must(data => data != default)
                .WithMessage("A data do evento e obrigatoria.");
        }
    }
}
