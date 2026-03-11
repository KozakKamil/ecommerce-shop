import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product';
import { CartService } from '../../services/cart';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss'
})

export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  loading = true;
  notFound = false;
  adding = false;
  added = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      if(!id) {
        this.router.navigate(['/products']);
        return;
      }

      this.productService.getProduct(id).subscribe({
        next: (product) => {
          this.product = product;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.loading = false;
          this.notFound = err.status === 404;
          this.cdr.detectChanges();
        }
      });
  }

  addToCart(): void {
    if(!this.product || this.adding) return;
    this.adding = true;

    this.cartService.addToCart(this.product.id).subscribe({
      next: () => {
        this.adding = false;
        this.added = true;
        this.cdr.detectChanges();
        setTimeout(() => {
          this.added = false;
          this.cdr.detectChanges();
        }, 2000);
      },
      error: (err) => {
        this.adding = false;
        console.error('Błąd dodawania do koszyka:', err);
        this.cdr.detectChanges();
      }
    });
  }
}
