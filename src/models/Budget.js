import mongoose from 'mongoose';

const budgetSchema = new mongoose.Schema(
  {
    budgetId: {
      type: String,
      required: true,
      unique: true,
      index: true
    },
    customerId: {
      type: String,
      required: true,
      index: true
    },
    customerEmail: {
      type: String,
      required: true
    },
    customerName: {
      type: String,
      required: true
    },
    vehicleInfo: {
      licensePlate: String,
      model: String,
      year: Number,
      brand: String
    },
    items: [{
      description: String,
      quantity: Number,
      unitPrice: Number,
      total: Number
    }],
    totalAmount: {
      type: Number,
      required: true
    },
    taxAmount: Number,
    discountAmount: {
      type: Number,
      default: 0
    },
    status: {
      type: String,
      enum: ['pending', 'sent', 'approved', 'rejected', 'expired'],
      default: 'pending'
    },
    sentAt: Date,
    approvedAt: Date,
    rejectedAt: Date,
    expiresAt: {
      type: Date,
      required: true
    },
    notes: String
  },
  {
    timestamps: true
  }
);

export default mongoose.model('Budget', budgetSchema);
