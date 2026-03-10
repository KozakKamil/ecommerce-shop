import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { CartItem, AddToCartDto, UpdateCartItemDto } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = '/api/cart';
  private userId = 'user-1';

  private cartCountSubject = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCountSubject.asObservable();

  constructor(private http: HttpClient, private ngZone: NgZone) {
    this.refreshCartCount();
  }

  getCartItems(): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(`${this.apiUrl}/${this.userId}`);
  }

  addToCart(productId: number, quantity: number = 1): Observable<CartItem> {
    const dto: AddToCartDto = {
      productId,
      quantity,
      userId: this.userId
    };
    return this.http.post<CartItem>(this.apiUrl, dto).pipe(
      tap(() => this.refreshCartCount())
    );
  }

  updateQuantity(itemId: number, quantity: number): Observable<void> {
    const dto: UpdateCartItemDto = { quantity };
    return this.http.put<void>(`${this.apiUrl}/${itemId}`, dto).pipe(
      tap(() => this.refreshCartCount())
    );
  }

  removeItem(itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${itemId}`).pipe(
      tap(() => this.refreshCartCount())
    );
  }

  clearCart(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${this.userId}`).pipe(
      tap(() => this.refreshCartCount())
    );
  }

  private refreshCartCount(): void {
    this.http.get<CartItem[]>(`${this.apiUrl}/${this.userId}`).subscribe({
      next: (items) => {
        const count = items.reduce((sum, item) => sum + item.quantity, 0);
        this.ngZone.run(() => {
          this.cartCountSubject.next(count);
        });
      },
      error: () => {
        this.ngZone.run(() => {
          this.cartCountSubject.next(0);
        });
      }
    });
  }
}
