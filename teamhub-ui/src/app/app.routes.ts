import { Routes } from '@angular/router';
import { TeamHubComponent } from './teamhub/teamhub.component';
export const routes:Routes=[{path:'',redirectTo:'dashboard',pathMatch:'full'},{path:':module',component:TeamHubComponent},{path:'**',redirectTo:'dashboard'}];
