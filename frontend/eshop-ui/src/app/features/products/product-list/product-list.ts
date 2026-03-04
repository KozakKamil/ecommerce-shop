import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { Product, Category } from '../../../shared/models/product.model';
import { ProductCard } from '../product-card/product-card';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [ProductCard, FormsModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss'
})

export class ProductList implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  searchTerm = '';
  selectedCategory: number | null = null;

  constructor(
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

loadProducts(): void {
    this.productService
      .getProducts(this.selectedCategory ?? undefined, this.searchTerm || undefined)
      .subscribe({
        next: (products) => {
          console.log('Otrzymane produkty:', products);
          this.products = [...products];
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Błąd pobierania produktów:', err);
        }
      });
  }

  loadCategories() {
    this.productService
      .getCategories()
      .subscribe(categories => {
        this.categories = [...categories];
        this.cdr.detectChanges();
      });
  }

  onSearch() {
    this.loadProducts();
  }

  onCategoryChange() {
    this.loadProducts();
  }
}
