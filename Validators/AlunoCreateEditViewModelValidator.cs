using System.Globalization;
using FluentValidation;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Validators
{
    public class AlunoCreateEditViewModelValidator : AbstractValidator<AlunoCreateEditViewModel>
    {
        public AlunoCreateEditViewModelValidator()
        {
            RuleFor(aluno => aluno.Nome)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O nome do aluno e obrigatorio.")
                .MaximumLength(100).WithMessage("O nome do aluno deve ter no maximo 100 caracteres.");

            RuleFor(aluno => aluno.Sobrenome)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O sobrenome do aluno e obrigatorio.")
                .MaximumLength(100).WithMessage("O sobrenome do aluno deve ter no maximo 100 caracteres.");

            RuleFor(aluno => aluno.DataNasc)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("A data de nascimento e obrigatoria.")
                .Must(BeValidBirthDate).WithMessage("A data de nascimento deve estar no formato dd/MM/yyyy.");

            RuleFor(aluno => aluno.ProfessorId)
                .GreaterThan(0).WithMessage("Informe um professor valido.");

            RuleFor(aluno => aluno.IdUsuario)
                .GreaterThan(0).WithMessage("Informe um usuario de aluno valido.");
        }

        private static bool BeValidBirthDate(string value)
        {
            return DateTime.TryParseExact(
                value,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }
    }
}
