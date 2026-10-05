import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Alerta, KpiDashboard } from '../models';

export interface DashboardGeneral {
  kpIs: KpiDashboard;
  cumpleanosDelMes: Array<{ id: number; nombreCompleto: string; dia: number }>;
  contratosPorVencer: Array<{ id: number; nombreCompleto: string; fechaFinContrato: string; diasRestantes: number }>;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly api = inject(ApiService);

  getGeneral(): Observable<DashboardGeneral> {
    return this.api.get<DashboardGeneral>('/api/dashboard/general');
  }

  getCapacidad(): Observable<unknown[]> {
    return this.api.get<unknown[]>('/api/dashboard/capacidad');
  }

  getConocimientos(): Observable<unknown> {
    return this.api.get('/api/dashboard/conocimientos');
  }

  getAlertas(): Observable<Alerta[]> {
    return this.api.get<Alerta[]>('/api/dashboard/alertas');
  }

  getDisponiblesPronto(): Observable<unknown> {
    return this.api.get('/api/dashboard/disponibles-pronto');
  }
}
