import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../models/product.model';
import { CartService } from '../../services/cart';

@Component({
  selector: 'app-product-card',
  imports: [CommonModule],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss'
})
export class ProductCardComponent {
  @Input() product!: Product;
  adding = false;
  added = false;

  constructor(private cartService: CartService) {}

  addToCart(): void {
    if (this.adding) return;
    this.adding = true;

    this.cartService.addToCart(this.product.id).subscribe({
      next: () => {
        this.adding = false;
        this.added = true;
        setTimeout(() => this.added = false, 2000);
      },
      error: (err) => {
        this.adding = false;
        console.error('Błąd dodawania do koszyka:', err);
      }
    });
  }
}
