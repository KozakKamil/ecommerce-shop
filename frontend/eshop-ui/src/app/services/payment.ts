import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CheckoutSessionResponse {
  sessionId: string;
  url: string;
}

@Injectable({
  providedIn: 'root'
})

export class PaymentService {
  private apiUrl = '/api/payments';

  constructor(private http: HttpClient) {}

  createCheckoutSession(): Observable<CheckoutSessionResponse> {
    return this.http.post<CheckoutSessionResponse>(
      `${this.apiUrl}/create-checkout-session`,
      {}
    );
  }
}
