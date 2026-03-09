import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../services/admin';

@Component({
  selector: 'app-admin-orders',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './admin-orders.html',
  styleUrl: './admin-orders.scss'
})
export class AdminOrdersComponent implements OnInit {
  orders: any[] = [];
  loading = true;
  statuses =[
    { value: 0, label: '⏳ Oczekujące' },
    { value: 1, label: '💰 Opłacone' },
    { value: 2, label: '📦 W realizacji' },
    { value: 3, label: '🚚 Wysłane' },
    { value: 4, label: '✅ Dostarczone' },
    { value: 5, label: '❌ Anulowane' }
  ];

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.adminService.getOrders().subscribe({
      next: (orders) => { this.orders = orders; this.loading = false; },
      error: () => this.loading = false
    });
  }

  changeStatus(orderId: number, newStatus:string): void {
    this.adminService.updateOrderStatus(orderId, +newStatus).subscribe({
      next: () => this.loadOrders()
    });
  }

  getStatusLabel(status: number): string {
    return this.statuses.find(s => s.value === status)?.label || 'Nieznany';
  }
}