using ESCOLA_API.Data;
using ESCOLA_API.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Services
{
    public sealed class DatabaseCadernetaDigitalEventPublisher : ICadernetaDigitalEventPublisher
    {
        private readonly DataContext _context;

        public DatabaseCadernetaDigitalEventPublisher(DataContext context)
        {
            _context = context;
        }

        public async Task PublishNotasPublicadasAsync(
            CadernetaDigitalViewModel caderneta,
            string operacao,
            CancellationToken cancellationToken = default)
        {
            var payload = CadernetaDigitalNotificacaoBuilder.CreateMessage(caderneta, operacao);
            await PublishMessageAsync(payload, cancellationToken);
        }

        public async Task PublishMessageAsync(
            CadernetaDigitalNotificacaoMessage payload,
            CancellationToken cancellationToken = default)
        {
            _context.Notificacoes.Add(CadernetaDigitalNotificacaoBuilder.CreateNotificacao(payload));

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsDuplicateMessage(ex))
            {
                _context.ChangeTracker.Clear();
            }
        }

        internal static bool IsDuplicateMessage(DbUpdateException exception)
        {
            return exception.InnerException switch
            {
                SqlException sqlException => sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627),
                SqliteException sqliteException => sqliteException.SqliteErrorCode == 19,
                _ => false
            };
        }
    }
}
