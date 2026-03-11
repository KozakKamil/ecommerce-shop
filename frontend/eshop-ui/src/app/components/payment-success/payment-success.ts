import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-payment-success',
  imports: [CommonModule, RouterLink],
  templateUrl: './payment-success.html',
  styleUrl: './payment-success.scss'
})

export class PaymentSuccessComponent implements OnInit {
  order: Order | null = null;
  loading = true;
  orderId: number | null = null;

  constructor(private route: ActivatedRoute, private orderService: OrderService) {}

  ngOnInit(): void {
      this.orderId = Number(this.route.snapshot.paramMap.get('orderId'));

      if(this.orderId){
        this.orderService.getOrder(this.orderId).subscribe({
          next: (order) => {
            this.order = order;
            this.loading = false;
          },
          error: () => {
            this.loading = false;
          }
        });
      } else {
        this.loading = false;
      }
    }
}