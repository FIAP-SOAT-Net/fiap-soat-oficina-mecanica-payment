using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Models;

[BsonIgnoreExtraElements]
public class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("paymentId")]
    public string PaymentId { get; set; } = null!;

    [BsonElement("budgetId")]
    public string BudgetId { get; set; } = null!;

    [BsonElement("orderId")]
    public string? OrderId { get; set; }

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = null!;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = null!; // credit_card, debit_card, pix, boleto, bank_transfer

    [BsonElement("status")]
    public string Status { get; set; } = "pending"; // pending, processing, completed, failed, refunded, cancelled

    [BsonElement("paymentDetails")]
    public PaymentDetails? PaymentDetails { get; set; }

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("failureReason")]
    public string? FailureReason { get; set; }

    [BsonElement("refundedAmount")]
    public decimal RefundedAmount { get; set; }

    [BsonElement("refundedAt")]
    public DateTime? RefundedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class PaymentDetails
{
    [BsonElement("transactionId")]
    public string? TransactionId { get; set; }

    [BsonElement("authorizationCode")]
    public string? AuthorizationCode { get; set; }

    [BsonElement("installments")]
    public int Installments { get; set; }

    [BsonElement("cardLastDigits")]
    public string? CardLastDigits { get; set; }
}
