import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
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

  selectedFile: File | null = null;
  uploadingImage = false;
  imagePreview: string | null = null;
  uploadError: string | null = null;

  constructor(
    private adminService: AdminService,
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.productService.getCategories().subscribe(c => {
      this.categories = c;
      this.cdr.detectChanges();
    });
  }

  loadProducts(): void {
    this.loading = true;
    this.adminService.getProducts().subscribe({
      next: (p) => { this.products = p; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  openCreateForm(): void {
    this.editingId = null;
    this.form = { name: '', description: '', price: 0, imageUrl: '', stock: 0, categoryId: 1 };
    this.selectedFile = null;
    this.imagePreview = null;
    this.uploadError = null;
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
    this.selectedFile = null;
    this.imagePreview = null;
    this.uploadError = null;
    this.showForm = true;
  }

  onFileSelected(event: any): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      const file = input.files[0];
      const allowedExtensions = ['.jpg', '.jpeg', '.png', '.webp'];
      const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      if (!allowedExtensions.includes(ext)) {
        this.uploadError = 'Nieobsługiwany format pliku. Dozwolone: JPG, PNG, WEBP.';
        this.selectedFile = null;
        this.imagePreview = null;
        input.value = '';
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        this.uploadError = 'Plik jest zbyt duży (max 5 MB).';
        this.selectedFile = null;
        this.imagePreview = null;
        input.value = '';
        return;
      }
      this.uploadError = null;
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => this.imagePreview = reader.result as string;
      reader.readAsDataURL(this.selectedFile);
    }
  }

  saveProduct(): void {
    const save$ = this.editingId
      ? this.adminService.updateProduct(this.editingId, this.form)
      : this.adminService.createProduct(this.form);

    save$.subscribe({
      next: (product: any) => {
        const productId = this.editingId ?? product.id;
        if(this.selectedFile){
          this.uploadingImage = true;
          this.adminService.uploadProductImage(productId, this.selectedFile).subscribe({
            next: () => {
              this.uploadingImage = false;
              this.selectedFile = null;
              this.imagePreview = null;
              this.uploadError = null;
              this.showForm = false;
              this.loadProducts();
            },
            error: (err) => {
              this.uploadingImage = false;
              this.uploadError = err?.error?.message ?? 'Błąd podczas przesyłania obrazka.';
            }
          });
        } else {
          this.showForm = false;
          this.loadProducts();
        }
      }
    });
  }

  deleteProduct(id: number): void {
    if (confirm('Czy na pewno chcesz usunąć ten produkt?')) {
      this.adminService.deleteProduct(id).subscribe(() => { this.loadProducts(); this.cdr.detectChanges(); });
    }
  }

  cancelForm(): void {
    this.showForm = false;
    this.selectedFile = null;
    this.imagePreview = null;
    this.uploadError = null;
  }
}
