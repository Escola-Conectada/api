using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    public interface ICadernetaDigitalEventPublisher
    {
        Task PublishNotasPublicadasAsync(
            CadernetaDigitalViewModel caderneta,
            string operacao,
            CancellationToken cancellationToken = default);
    }
}
