using FluentValidation;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Validators
{
    public class GoogleLoginRequestViewModelValidator : AbstractValidator<GoogleLoginRequestViewModel>
    {
        public GoogleLoginRequestViewModelValidator()
        {
            RuleFor(login => login.IdToken)
                .NotEmpty().WithMessage("O token do Google e obrigatorio.");
        }
    }
}
