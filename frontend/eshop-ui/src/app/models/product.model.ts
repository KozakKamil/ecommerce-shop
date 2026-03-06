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
}

export interface Category {
  id: number;
  name: string;
  description: string;
  products?: Product[];
}
