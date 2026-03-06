import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../services/cart';
import { OrderService } from '../../services/order';
import { CartItem } from '../../models/cart.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cart',
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.scss'
})
export class CartComponent implements OnInit {
  cartItems: CartItem[] = [];
  loading = true;
  placingOrder = false;

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading = true;
    this.cartService.getCartItems().subscribe({
      next: (items) => {
        this.cartItems = items;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Błąd ładowania koszyka:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get totalPrice(): number {
    return this.cartItems.reduce((sum, item) => sum + (item.product.price * item.quantity), 0);
  }

  increaseQuantity(item: CartItem): void {
    this.cartService.updateQuantity(item.id, item.quantity + 1).subscribe({
      next: () => this.loadCart(),
      error: (err) => console.error(err)
    });
  }

  decreaseQuantity(item: CartItem): void {
    if (item.quantity <= 1) {
      this.removeItem(item);
      return;
    }
    this.cartService.updateQuantity(item.id, item.quantity - 1).subscribe({
      next: () => this.loadCart(),
      error: (err) => console.error(err)
    });
  }

  removeItem(item: CartItem): void {
    this.cartService.removeItem(item.id).subscribe({
      next: () => this.loadCart(),
      error: (err) => console.error(err)
    });
  }

  clearCart(): void {
    this.cartService.clearCart().subscribe({
      next: () => this.loadCart(),
      error: (err) => console.error(err)
    });
  }

  placeOrder(): void {
    if (this.placingOrder) return;
    this.placingOrder = true;

    this.orderService.createOrder().subscribe({
      next: (order) => {
        this.placingOrder = false;
        this.loadCart();
        this.router.navigate(['/orders']);
      },
      error: (err) => {
        this.placingOrder = false;
        console.error('Błąd składania zamówienia:', err);
        this.cdr.detectChanges();
      }
    });
  }
}
