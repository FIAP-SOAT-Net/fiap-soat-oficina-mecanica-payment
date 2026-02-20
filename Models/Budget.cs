using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Models;

[BsonIgnoreExtraElements]
public class Budget
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("budgetId")]
    public string BudgetId { get; set; } = null!;

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = null!;

    [BsonElement("customerEmail")]
    public string CustomerEmail { get; set; } = null!;

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = null!;

    [BsonElement("vehicleInfo")]
    public VehicleInfo? VehicleInfo { get; set; }

    [BsonElement("items")]
    public List<BudgetItem> Items { get; set; } = new();

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("taxAmount")]
    public decimal TaxAmount { get; set; }

    [BsonElement("discountAmount")]
    public decimal DiscountAmount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "pending"; // pending, sent, approved, rejected, expired

    [BsonElement("sentAt")]
    public DateTime? SentAt { get; set; }

    [BsonElement("approvedAt")]
    public DateTime? ApprovedAt { get; set; }

    [BsonElement("rejectedAt")]
    public DateTime? RejectedAt { get; set; }

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class VehicleInfo
{
    [BsonElement("licensePlate")]
    public string? LicensePlate { get; set; }

    [BsonElement("model")]
    public string? Model { get; set; }

    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("brand")]
    public string? Brand { get; set; }
}

[BsonIgnoreExtraElements]
public class BudgetItem
{
    [BsonElement("description")]
    public string Description { get; set; } = null!;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("unitPrice")]
    public decimal UnitPrice { get; set; }

    [BsonElement("total")]
    public decimal Total { get; set; }
}
