namespace AbstractPayments.Sandbox.Storage;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Storage.Models;
using Dapper;

/// <summary>
/// Data port for reading and writing transaction records.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Bootstraps the physical database schema and creates tables if missing.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Persists a new transaction record.
    /// </summary>
    Task InsertAsync(Transaction transaction);

    /// <summary>
    /// Fetches all persisted transactions.
    /// </summary>
    Task<IEnumerable<Transaction>> GetAllAsync();

    /// <summary>
    /// Fetches a specific transaction by its unique identifier.
    /// </summary>
    Task<Transaction?> GetByIdAsync(string id);

    /// <summary>
    /// Updates the status of an existing transaction record.
    /// </summary>
    Task UpdateAsync(Transaction transaction);
}

/// <summary>
/// High-performance Dapper implementation of the TransactionRepository.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionRepository"/> class.
    /// </summary>
    /// <param name="connectionFactory">The database connection factory.</param>
    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                Amount REAL NOT NULL,
                Provider TEXT NOT NULL,
                PaymentString TEXT NOT NULL,
                Status TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                EndToEndId TEXT UNIQUE
            );";
        await connection.ExecuteAsync(sql);
    }

    /// <inheritdoc />
    public async Task InsertAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt, EndToEndId)
            VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt, @EndToEndId);";
        await connection.ExecuteAsync(sql, transaction);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Transactions ORDER BY CreatedAt DESC;";
        return await connection.QueryAsync<Transaction>(sql);
    }

    /// <inheritdoc />
    public async Task<Transaction?> GetByIdAsync(string id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Transactions WHERE Id = @Id;";
        return await connection.QueryFirstOrDefaultAsync<Transaction>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Transactions 
            SET Status = @Status, EndToEndId = @EndToEndId 
            WHERE Id = @Id;";
        await connection.ExecuteAsync(sql, transaction);
    }
}
