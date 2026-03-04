export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  stock: number;
  categoryId: number;
  categoryName?: string;
}

export interface Category {
  id: number;
  name: string;
  description: string;
}
