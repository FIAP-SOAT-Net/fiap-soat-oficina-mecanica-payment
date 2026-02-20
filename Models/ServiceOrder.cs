using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Models;

[BsonIgnoreExtraElements]
public class ServiceOrder
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("orderId")]
    public string OrderId { get; set; } = null!;

    [BsonElement("budgetId")]
    public string BudgetId { get; set; } = null!;

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = null!;

    [BsonElement("paymentId")]
    public string? PaymentId { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "pending_payment"; // pending_payment, in_progress, completed, cancelled

    [BsonElement("syncedWithOrderService")]
    public bool SyncedWithOrderService { get; set; }

    [BsonElement("lastSyncAt")]
    public DateTime? LastSyncAt { get; set; }

    [BsonElement("syncError")]
    public string? SyncError { get; set; }

    [BsonElement("syncAttempts")]
    public int SyncAttempts { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
