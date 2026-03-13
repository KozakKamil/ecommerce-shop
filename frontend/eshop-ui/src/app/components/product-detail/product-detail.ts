import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product';
import { CartService } from '../../services/cart';
import { Product, Review } from '../../models/product.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss'
})
export class ProductDetailComponent implements OnInit {
  product = signal<Product | null>(null);
  loading = signal(true);
  notFound = signal(false);
  adding = signal(false);
  added = signal(false);
  Math = Math;

  reviews = signal<Review[]>([]);
  avgRating = signal(0);
  totalReviews = signal(0);
  loadingReviews = signal(true);

  newRating = 5;
  newComment = '';
  submittingReview = false;
  reviewError = '';
  isLoggedIn = !!localStorage.getItem('token');

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService,
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.router.navigate(['/products']); return; }

    this.productService.getProduct(id).subscribe({
      next: (product) => {
        this.product.set(product);
        this.loading.set(false);
        setTimeout(() => this.loadReviews(id));
      },
      error: (err) => {
        this.loading.set(false);
        this.notFound.set(err.status === 404);
      }
    });
  }

  loadReviews(productId: number): void {
    this.loadingReviews.set(true);
    this.productService.getReviews(productId).subscribe({
      next: (data) => {
        this.reviews.set(data.reviews);
        this.avgRating.set(data.avgRating);
        this.totalReviews.set(data.totalReviews);
        this.loadingReviews.set(false);
      },
      error: () => { this.loadingReviews.set(false); }
    });
  }

  submitReview(): void {
    const prod = this.product();
    if (!prod || this.submittingReview) return;
    this.submittingReview = true;
    this.reviewError = '';

    this.productService.addReview(prod.id, this.newRating, this.newComment).subscribe({
      next: () => {
        this.newRating = 5;
        this.newComment = '';
        this.submittingReview = false;
        this.loadReviews(prod.id);
      },
      error: (err) => {
        this.submittingReview = false;
        this.reviewError = err.error || 'Błąd dodawania recenzji';
      }
    });
  }

  deleteReview(reviewId: number): void {
    const prod = this.product();
    if (!prod) return;
    this.productService.deleteReview(prod.id, reviewId).subscribe({
      next: () => this.loadReviews(prod.id)
    });
  }

  getStars(rating: number): string {
    return '★'.repeat(rating) + '☆'.repeat(5 - rating);
  }

  addToCart(): void {
    const prod = this.product();
    if (!prod || this.adding()) return;
    this.adding.set(true);

    this.cartService.addToCart(prod.id).subscribe({
      next: () => {
        this.adding.set(false);
        this.added.set(true);
        setTimeout(() => this.added.set(false), 2000);
      },
      error: (err) => {
        this.adding.set(false);
        console.error(err);
      }
    });
  }
}
