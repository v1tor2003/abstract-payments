namespace AbstractPayments.Sandbox.Storage;

using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Service factory returning standard isolated SQL connections targeting the SQLite storage.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Instantiates and returns a raw database connection.
    /// </summary>
    IDbConnection CreateConnection();
}

/// <summary>
/// Concrete SQLite connection provider.
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionFactory"/> class from configurations.
    /// </summary>
    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=sandbox.db";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionFactory"/> class with an explicit connection string.
    /// </summary>
    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
