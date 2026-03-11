import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../services/order';
import { Order, OrderStatus } from '../../models/order.model';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, RouterLink],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;

  constructor(
    private orderService: OrderService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Błąd ładowania zamówień:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getStatusLabel(status: OrderStatus): string {
    const labels: Record<number, string> = {
      [OrderStatus.Pending]: '⏳ Oczekujące',
      [OrderStatus.PaymentReceived]: '💰 Opłacone',
      [OrderStatus.Processing]: '📦 W realizacji',
      [OrderStatus.Shipped]: '🚚 Wysłane',
      [OrderStatus.Delivered]: '✅ Dostarczone',
      [OrderStatus.Cancelled]: '❌ Anulowane'
    };
    return labels[status] || 'Nieznany status';
  }

  getStatusClass(status: OrderStatus): string {
    const classes: Record<number, string> = {
      [OrderStatus.Pending]: 'pending',
      [OrderStatus.PaymentReceived]: 'paid',
      [OrderStatus.Processing]: 'processing',
      [OrderStatus.Shipped]: 'shipped',
      [OrderStatus.Delivered]: 'delivered',
      [OrderStatus.Cancelled]: 'cancelled'
    };
    return classes[status] || '';
  }

  cancelOrder(orderId: number): void {
    if (!confirm('Czy na pewno chcesz anulować to zamówienie?')) return;
    this.orderService.cancelOrder(orderId).subscribe({
      next: () => this.loadOrders(),
      error: (err) => console.error('Błąd anulowania:', err)
    });
  }
}
