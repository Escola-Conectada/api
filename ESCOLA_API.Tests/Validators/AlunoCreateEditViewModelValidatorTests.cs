using FluentValidation.TestHelper;
using ESCOLA_API.Validators;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Tests.Validators
{
    public class AlunoCreateEditViewModelValidatorTests
    {
        private readonly AlunoCreateEditViewModelValidator _validator = new();

        [Fact]
        public void Validate_WhenModelIsValid_ShouldNotHaveValidationErrors()
        {
            var model = new AlunoCreateEditViewModel
            {
                Nome = "Maria",
                Sobrenome = "Solano",
                DataNasc = "25/02/1982",
                ProfessorId = 1,
                IdUsuario = 12
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenRequiredFieldsAreMissing_ShouldHaveValidationErrors()
        {
            var model = new AlunoCreateEditViewModel();

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(aluno => aluno.Nome);
            result.ShouldHaveValidationErrorFor(aluno => aluno.Sobrenome);
            result.ShouldHaveValidationErrorFor(aluno => aluno.DataNasc);
            result.ShouldHaveValidationErrorFor(aluno => aluno.ProfessorId);
            result.ShouldHaveValidationErrorFor(aluno => aluno.IdUsuario);
        }

        [Fact]
        public void Validate_WhenBirthDateIsInvalid_ShouldHaveValidationError()
        {
            var model = new AlunoCreateEditViewModel
            {
                Nome = "Maria",
                Sobrenome = "Solano",
                DataNasc = "31/02/1982",
                ProfessorId = 1,
                IdUsuario = 12
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(aluno => aluno.DataNasc);
        }
    }
}
