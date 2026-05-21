using FluentValidation;
using form_API.ViewModels;

namespace form_API.Validators
{
    public class UsuarioCreateViewModelValidator : AbstractValidator<UsuarioCreateViewModel>
    {
        public UsuarioCreateViewModelValidator()
        {
            RuleFor(usuario => usuario.Nome)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O nome do usuario e obrigatorio.")
                .MaximumLength(100).WithMessage("O nome do usuario deve ter no maximo 100 caracteres.");

            RuleFor(usuario => usuario.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O email e obrigatorio.")
                .EmailAddress().WithMessage("Informe um email valido.")
                .MaximumLength(150).WithMessage("O email deve ter no maximo 150 caracteres.");

            RuleFor(usuario => usuario.Telefone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O telefone e obrigatorio.")
                .MaximumLength(20).WithMessage("O telefone deve ter no maximo 20 caracteres.");

            RuleFor(usuario => usuario.Senha)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("A senha e obrigatoria.")
                .MinimumLength(8).WithMessage("A senha deve ter no minimo 8 caracteres.");

            RuleFor(usuario => usuario.IdPerfil)
                .GreaterThan(0).WithMessage("Informe um perfil valido.");
        }
    }
}
