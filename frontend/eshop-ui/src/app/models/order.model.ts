export interface Order {
  id: number;
  userId: string;
  orderDate: string;
  totalAmount: number;
  status: OrderStatus;
  stripePaymentIntentId?: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: number;
  quantity: number;
  unitPrice: number;
  orderId: number;
  productId: number;
  product: {
    id: number;
    name: string;
    imageUrl: string;
  };
}

export enum OrderStatus {
  Pending = 0,
  PaymentReceived = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Cancelled = 5
}

export interface CreateOrderDto {
  userId: string;
}
