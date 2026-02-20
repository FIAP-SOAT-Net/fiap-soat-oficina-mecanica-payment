import mongoose from 'mongoose';

const serviceOrderSchema = new mongoose.Schema(
  {
    orderId: {
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
    customerId: {
      type: String,
      required: true,
      index: true
    },
    status: {
      type: String,
      enum: ['pending_payment', 'in_progress', 'completed', 'cancelled'],
      default: 'pending_payment'
    },
    paymentId: String,
    syncedWithOrderService: {
      type: Boolean,
      default: false
    },
    lastSyncAt: Date,
    syncError: String,
    syncAttempts: {
      type: Number,
      default: 0
    }
  },
  {
    timestamps: true
  }
);

export default mongoose.model('ServiceOrder', serviceOrderSchema);
