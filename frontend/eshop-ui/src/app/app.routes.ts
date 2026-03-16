import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list';
import { CartComponent } from './components/cart/cart';
import { OrdersComponent } from './components/orders/orders';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { AdminDashboardComponent } from './components/admin/admin-dashboard/admin-dashboard';
import { AdminProductsComponent } from './components/admin/admin-products/admin-products';
import { AdminOrdersComponent } from './components/admin/admin-orders/admin-orders';
import { ProductDetailComponent } from './components/product-detail/product-detail';
import { PaymentSuccessComponent } from './components/payment-success/payment-success';
import { PaymentCancelComponent } from './components/payment-cancel/payment-cancel';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';
import { ProfileComponent } from './components/profile/profile';

export const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  { path: 'products', component: ProductListComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'cart', component: CartComponent, canActivate: [authGuard] },
  { path: 'orders', component: OrdersComponent, canActivate: [authGuard] },
  { path: 'admin', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/products', component: AdminProductsComponent, canActivate: [adminGuard] },
  { path: 'admin/orders', component: AdminOrdersComponent, canActivate: [adminGuard] },
  { path: 'payment/success', component: PaymentSuccessComponent, canActivate: [authGuard] },
  { path: 'payment/cancel', component: PaymentCancelComponent, canActivate: [authGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
  { path: 'products/:id', component: ProductDetailComponent }
];
