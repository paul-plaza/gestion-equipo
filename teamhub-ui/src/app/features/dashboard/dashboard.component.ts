import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DashboardService, DashboardGeneral } from '../../core/services/dashboard.service';
import { Alerta } from '../../core/models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatChipsModule, MatDividerModule, MatProgressBarModule
  ],
  template: `
    <div class="page-header">
      <h1>Dashboard</h1>
      <span class="text-muted">{{ today | date:'EEEE, d MMMM yyyy':'':'es' }}</span>
    </div>

    <!-- KPIs -->
    @if (data()) {
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-value">{{ data()!.kpIs.personasActivas }}</div>
          <div class="kpi-label">Personas Activas</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-value">{{ data()!.kpIs.internos }}</div>
          <div class="kpi-label">Internos</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-value">{{ data()!.kpIs.externos }}</div>
          <div class="kpi-label">Externos</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-value">{{ data()!.kpIs.proyectosActivos }}</div>
          <div class="kpi-label">Proyectos Activos</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-value">{{ data()!.kpIs.proyectosPorIniciar }}</div>
          <div class="kpi-label">Por Iniciar</div>
        </div>
        <div class="kpi-card" [class.badge-danger]="data()!.kpIs.horasExtraPendientes > 0">
          <div class="kpi-value" [style.color]="data()!.kpIs.horasExtraPendientes > 0 ? 'var(--color-danger)' : 'var(--color-primary)'">
            {{ data()!.kpIs.horasExtraPendientes }}
          </div>
          <div class="kpi-label">HE Pendientes</div>
        </div>
      </div>

      <div class="dashboard-grid">
        <!-- Alertas -->
        <div class="card">
          <h3 class="section-title">
            <mat-icon color="warn">warning</mat-icon> Alertas
          </h3>
          @if (alertas().length === 0) {
            <p class="text-muted">Sin alertas activas</p>
          }
          @for (alerta of alertas(); track $index) {
            <div class="alert-item alert-{{ alerta.criticidad }}">
              <mat-icon>{{ iconForAlerta(alerta.tipo) }}</mat-icon>
              <span>{{ alerta.mensaje }}</span>
            </div>
          }
        </div>

        <!-- Contratos por vencer -->
        @if (data()!.contratosPorVencer.length > 0) {
          <div class="card">
            <h3 class="section-title">
              <mat-icon color="warn">assignment_late</mat-icon> Contratos por Vencer
            </h3>
            @for (c of data()!.contratosPorVencer; track c.id) {
              <div class="list-item">
                <div class="list-item-main">
                  <strong>{{ c.nombreCompleto }}</strong>
                  <span class="text-muted">{{ c.fechaFinContrato | date:'dd/MM/yyyy' }}</span>
                </div>
                <span class="badge" [class]="badgeForDias(c.diasRestantes)">{{ c.diasRestantes }}d</span>
              </div>
            }
          </div>
        }

        <!-- Cumpleaños del mes -->
        @if (data()!.cumpleanosDelMes.length > 0) {
          <div class="card">
            <h3 class="section-title">
              <mat-icon style="color: #f06292">cake</mat-icon> Cumpleaños del Mes
            </h3>
            @for (c of data()!.cumpleanosDelMes; track c.id) {
              <div class="list-item">
                <mat-icon style="color: #f06292">cake</mat-icon>
                <span>{{ c.nombreCompleto }}</span>
                <span class="text-muted spacer">día {{ c.dia }}</span>
              </div>
            }
          </div>
        }
      </div>
    } @else if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
  `,
  styles: [`
    .dashboard-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 16px; }
    .section-title { display: flex; align-items: center; gap: 8px; font-size: 1rem; font-weight: 600; margin: 0 0 16px; }
    .list-item { display: flex; align-items: center; gap: 10px; padding: 8px 0; border-bottom: 1px solid var(--color-border); &:last-child { border: none; } .list-item-main { display: flex; flex-direction: column; flex: 1; } }
  `]
})
export class DashboardComponent implements OnInit {
  private readonly svc = inject(DashboardService);

  readonly data = signal<DashboardGeneral | null>(null);
  readonly alertas = signal<Alerta[]>([]);
  readonly loading = signal(true);
  readonly today = new Date();

  ngOnInit(): void {
    forkJoin({
      general: this.svc.getGeneral(),
      alertas: this.svc.getAlertas()
    }).subscribe({
      next: ({ general, alertas }) => {
        this.data.set(general);
        this.alertas.set(alertas);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  iconForAlerta(tipo: string): string {
    const icons: Record<string, string> = {
      contrato_por_vencer: 'assignment_late',
      horas_extra_pendientes: 'schedule',
      objetivos_incompletos: 'track_changes',
      proyectos_atrasados: 'warning'
    };
    return icons[tipo] ?? 'info';
  }

  badgeForDias(dias: number): string {
    if (dias <= 7) return 'badge badge-danger';
    if (dias <= 15) return 'badge badge-warning';
    return 'badge badge-info';
  }
}
