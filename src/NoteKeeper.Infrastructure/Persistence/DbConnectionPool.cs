using System.Collections.Concurrent;
using System.Data;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Infrastructure.Persistence;

public sealed class DbConnectionPool : IDbConnectionPool
{
    private static DbConnectionPool? _instance;

    private readonly ConcurrentBag<IDbConnection> _connections = new();
    private readonly string _connectionString;

    private DbConnectionPool(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static void Initialize(string connectionString) =>
        _instance ??= new DbConnectionPool(connectionString);

    public static DbConnectionPool Instance => _instance!;

    public IDbConnection GetConnection()
    {
        if (_connections.TryTake(out var connection) && connection.State == ConnectionState.Open)
            return connection;

        var newConnection = new SqlConnection(_connectionString);
        newConnection.Open();
        return newConnection;
    }

    public void ReturnConnection(IDbConnection connection)
    {
        if (connection.State == ConnectionState.Open)
            _connections.Add(connection);
        else
            connection.Dispose();
    }
}