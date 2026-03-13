export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  stock: number;
  createdAt: string;
  categoryId: number;
  category?: Category;
  avgRating: number;
  totalReviews: number;
}

export interface Category {
  id: number;
  name: string;
  description: string;
  products?: Product[];
}

export interface ProductsPage {
  items: Product[];
  totalCount: number;
}

export interface Review {
  id: number;
  rating: number;
  comment: string;
  createdAt: string;
  userName: string;
}

export interface ReviewsResponse {
  reviews: Review[];
  avgRating: number;
  totalReviews: number;
}
