import { Routes } from '@angular/router';

import { LoginComponent } from './components/login/login';
import { DashboardComponent } from './components/dashboard/dashboard';
import { LeadsComponent } from './components/leads/leads';
import { CompanyComponent } from './components/company/company';
import { UsersComponent } from './components/users/users';
import { SearchComponent } from './components/search/search';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [

  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },

  {
    path: 'login',
    component: LoginComponent
  },

  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },

  {
    path: 'leads',
    component: LeadsComponent,
    canActivate: [authGuard]
  },

  {
    path: 'company',
    component: CompanyComponent,
    canActivate: [authGuard]
  },

  {
    path: 'users',
    component: UsersComponent,
    canActivate: [authGuard]
  },
 {
    path:'search',
    component: SearchComponent,
    canActivate:[authGuard]
} 

];