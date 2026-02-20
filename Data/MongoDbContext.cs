using MongoDB.Driver;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IMongoDbContext
{
    IMongoCollection<Budget> Budgets { get; }
    IMongoCollection<Payment> Payments { get; }
    IMongoCollection<ServiceOrder> ServiceOrders { get; }
}

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("payment_service");
    }

    public IMongoCollection<Budget> Budgets => _database.GetCollection<Budget>("budgets");
    public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("payments");
    public IMongoCollection<ServiceOrder> ServiceOrders => _database.GetCollection<ServiceOrder>("serviceorders");
}
