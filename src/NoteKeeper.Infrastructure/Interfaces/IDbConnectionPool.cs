using System.Data;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IDbConnectionPool
{
    IDbConnection GetConnection();

    void ReturnConnection(IDbConnection connection);
}