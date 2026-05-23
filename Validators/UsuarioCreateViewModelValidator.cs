using FluentValidation;
using ESCOLA_API.Models;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Validators
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

            RuleFor(usuario => usuario.TipoUsuario)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O tipo do usuario e obrigatorio.")
                .Must(PerfilSistema.TipoUsuarioValido)
                .WithMessage("Informe um tipo de usuario valido: Aluno, Professor ou Administrador.");
        }
    }

    public class UsuarioUpdateViewModelValidator : AbstractValidator<UsuarioUpdateViewModel>
    {
        public UsuarioUpdateViewModelValidator()
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

            RuleFor(usuario => usuario.TipoUsuario)
                .Must(PerfilSistema.TipoUsuarioValido)
                .When(usuario => !string.IsNullOrWhiteSpace(usuario.TipoUsuario))
                .WithMessage("Informe um tipo de usuario valido: Aluno, Professor ou Administrador.");
        }
    }
}
