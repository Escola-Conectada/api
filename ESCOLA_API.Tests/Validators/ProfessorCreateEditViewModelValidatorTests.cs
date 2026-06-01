using FluentValidation.TestHelper;
using ESCOLA_API.Validators;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Tests.Validators
{
    public class ProfessorCreateEditViewModelValidatorTests
    {
        private readonly ProfessorCreateEditViewModelValidator _validator = new();

        [Fact]
        public void Validate_WhenModelIsValid_ShouldNotHaveValidationErrors()
        {
            var model = new ProfessorCreateEditViewModel
            {
                Nome = "Vinicius",
                IdUsuario = 2
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenNameIsMissing_ShouldHaveValidationError()
        {
            var model = new ProfessorCreateEditViewModel();

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(professor => professor.Nome);
            result.ShouldHaveValidationErrorFor(professor => professor.IdUsuario);
        }
    }
}
