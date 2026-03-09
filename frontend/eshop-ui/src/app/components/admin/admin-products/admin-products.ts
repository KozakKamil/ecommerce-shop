import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminService, CreateProductDto } from '../../../services/admin';
import { ProductService } from '../../../services/product';
import { Category } from '../../../models/product.model';

@Component({
  selector: 'app-admin-products',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './admin-products.html',
  styleUrl: './admin-products.scss'
})
export class AdminProductsComponent implements OnInit {
  products: any[] = [];
  categories: Category[] = [];
  loading = true;
  showForm = false;
  editingId: number | null = null;

  form: CreateProductDto = {
    name: '', description: '', price: 0,
    imageUrl: '', stock: 0, categoryId: 1
  };

  constructor(
    private adminService: AdminService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.productService.getCategories().subscribe(c => this.categories = c);
  }

  loadProducts(): void {
    this.loading = true;
    this.adminService.getProducts().subscribe({
      next: (p) => { this.products = p; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openCreateForm(): void {
    this.editingId = null;
    this.form = { name: '', description: '', price: 0, imageUrl: '', stock: 0, categoryId: 1 };
    this.showForm = true;
  }

  openEditForm(product: any): void {
    this.editingId = product.id;
    this.form = {
      name: product.name,
      description: product.description,
      price: product.price,
      imageUrl: product.imageUrl,
      stock: product.stock,
      categoryId: product.categoryId
    };
    this.showForm = true;
  }

  saveProduct(): void {
    if (this.editingId) {
      this.adminService.updateProduct(this.editingId, this.form).subscribe({
        next: () => { this.showForm = false; this.loadProducts(); }
      });
    } else {
      this.adminService.createProduct(this.form).subscribe({
        next: () => { this.showForm = false; this.loadProducts(); }
      });
    }
  }

  deleteProduct(id: number): void {
    if (confirm('Czy na pewno chcesz usunąć ten produkt?')) {
      this.adminService.deleteProduct(id).subscribe(() => this.loadProducts());
    }
  }

  cancelForm(): void {
    this.showForm = false;
  }
}