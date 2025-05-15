using System.Data;

namespace Auth.Application.Services;

public interface IUnitOfWork
{
    Task SaveChangeAsync();

    IDbTransaction Begin(IsolationLevel level = IsolationLevel.ReadCommitted);
}