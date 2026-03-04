import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { Product } from '../../../shared/models/product.model';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [RouterLink, CurrencyPipe],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss'
})

export class ProductCard{
  @Input({ required: true }) product!: Product;

  onAddToCart(){
    console.log('Dodano do koszyka:', this.product.name);
  }
}
