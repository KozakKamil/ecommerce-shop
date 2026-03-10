import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardStats {
  totalProducts: number;
  totalOrders: number;
  totalRevenue: number;
  pendingOrders: number;
}

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  stock: number;
  categoryId: number;
}

@Injectable({
  providedIn: 'root'
})

export class AdminService {
  private apiUrl = '/api/admin';

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<DashboardStats>{
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }

  getProducts(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/products`);
  }

  createProduct(dto: CreateProductDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/products`, dto);
  }

  updateProduct(id: number, dto: CreateProductDto): Observable<any>{
    return this.http.put<void>(`${this.apiUrl}/products/${id}`, dto);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/products/${id}`);
  }

  getOrders(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/orders`);
  }

  updateOrderStatus(id: number, status: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/orders/${id}/status`, { status });
  }
}
