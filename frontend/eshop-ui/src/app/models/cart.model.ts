export interface CartItem {
  id: number;
  quantity: number;
  productId: number;
  product: {
    id: number;
    name: string;
    price: number;
    imageUrl: string;
  };
  userId: string;
}

export interface AddToCartDto {
  productId: number;
  quantity: number;
  userId: string;
}

export interface UpdateCartItemDto {
  quantity: number;
}
