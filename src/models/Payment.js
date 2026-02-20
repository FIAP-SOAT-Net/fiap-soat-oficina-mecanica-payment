import mongoose from 'mongoose';

const paymentSchema = new mongoose.Schema(
  {
    paymentId: {
      type: String,
      required: true,
      unique: true,
      index: true
    },
    budgetId: {
      type: String,
      required: true,
      index: true
    },
    orderId: {
      type: String,
      index: true
    },
    customerId: {
      type: String,
      required: true,
      index: true
    },
    amount: {
      type: Number,
      required: true
    },
    paymentMethod: {
      type: String,
      enum: ['credit_card', 'debit_card', 'pix', 'boleto', 'bank_transfer'],
      required: true
    },
    status: {
      type: String,
      enum: ['pending', 'processing', 'completed', 'failed', 'refunded', 'cancelled'],
      default: 'pending'
    },
    paymentDetails: {
      transactionId: String,
      authorizationCode: String,
      installments: Number,
      cardLastDigits: String
    },
    processedAt: Date,
    completedAt: Date,
    failureReason: String,
    refundedAmount: {
      type: Number,
      default: 0
    },
    refundedAt: Date
  },
  {
    timestamps: true
  }
);

export default mongoose.model('Payment', paymentSchema);
