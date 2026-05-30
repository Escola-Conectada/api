using ESCOLA_API.ViewModels;
using FluentValidation;

namespace ESCOLA_API.Validators
{
    public class AlunoTurmaEnsinoCreateUpdateViewModelValidator : AbstractValidator<AlunoTurmaEnsinoCreateUpdateViewModel>
    {
        public AlunoTurmaEnsinoCreateUpdateViewModelValidator()
        {
            RuleFor(model => model.IdAlunoUsuario)
                .GreaterThan(0).WithMessage("Informe um aluno valido.");

            RuleFor(model => model.IdTurmaEnsino)
                .GreaterThan(0).WithMessage("Informe uma turma de ensino valida.");
        }
    }
}
