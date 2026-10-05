import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';

interface MetricaSprint {
  id: number; proyectoId: number; proyecto: string; sprint: number;
  frecuenciaDespliegue: number; tiempoEntrega: number;
  tasaCambioFallido: number; tiempoRestauracion: number;
  fecha: string;
}

@Component({
  selector: 'app-metricas',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTableModule, MatCardModule
  ],
  template: `
    <div class="page-header">
      <h1>Métricas DORA</h1>
      <button mat-raised-button color="primary">
        <mat-icon>add</mat-icon> Registrar Sprint
      </button>
    </div>

    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Proyecto</mat-label>
        <mat-select [(ngModel)]="proyectoFilter" (ngModelChange)="load()">
          <mat-option value="">Todos</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

    <!-- DORA KPIs -->
    <div class="dora-grid">
      <div class="dora-card">
        <div class="dora-icon"><mat-icon>rocket_launch</mat-icon></div>
        <div class="dora-metric">{{ promedio('frecuenciaDespliegue') | number:'1.1-1' }}</div>
        <div class="dora-label">Deployment Frequency</div>
        <div class="dora-sub">deploys/semana</div>
        <div class="dora-badge" [class]="doraClass('df', promedio('frecuenciaDespliegue'))">{{ doraLevel('df', promedio('frecuenciaDespliegue')) }}</div>
      </div>
      <div class="dora-card">
        <div class="dora-icon"><mat-icon>timer</mat-icon></div>
        <div class="dora-metric">{{ promedio('tiempoEntrega') | number:'1.0-0' }}h</div>
        <div class="dora-label">Lead Time for Changes</div>
        <div class="dora-sub">horas promedio</div>
        <div class="dora-badge" [class]="doraClass('lt', promedio('tiempoEntrega'))">{{ doraLevel('lt', promedio('tiempoEntrega')) }}</div>
      </div>
      <div class="dora-card">
        <div class="dora-icon"><mat-icon>bug_report</mat-icon></div>
        <div class="dora-metric">{{ promedio('tasaCambioFallido') | number:'1.1-1' }}%</div>
        <div class="dora-label">Change Failure Rate</div>
        <div class="dora-sub">% de cambios fallidos</div>
        <div class="dora-badge" [class]="doraClass('cfr', promedio('tasaCambioFallido'))">{{ doraLevel('cfr', promedio('tasaCambioFallido')) }}</div>
      </div>
      <div class="dora-card">
        <div class="dora-icon"><mat-icon>healing</mat-icon></div>
        <div class="dora-metric">{{ promedio('tiempoRestauracion') | number:'1.0-0' }}h</div>
        <div class="dora-label">Mean Time to Restore</div>
        <div class="dora-sub">horas para recuperar</div>
        <div class="dora-badge" [class]="doraClass('mttr', promedio('tiempoRestauracion'))">{{ doraLevel('mttr', promedio('tiempoRestauracion')) }}</div>
      </div>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="card table-card">
      <table mat-table [dataSource]="metricas()" class="full-width">
        <ng-container matColumnDef="proyecto">
          <th mat-header-cell *matHeaderCellDef>Proyecto</th>
          <td mat-cell *matCellDef="let m">{{ m.proyecto }}</td>
        </ng-container>
        <ng-container matColumnDef="sprint">
          <th mat-header-cell *matHeaderCellDef>Sprint</th>
          <td mat-cell *matCellDef="let m">Sprint {{ m.sprint }}</td>
        </ng-container>
        <ng-container matColumnDef="df">
          <th mat-header-cell *matHeaderCellDef>Deploy Freq.</th>
          <td mat-cell *matCellDef="let m">{{ m.frecuenciaDespliegue }}/sem</td>
        </ng-container>
        <ng-container matColumnDef="lt">
          <th mat-header-cell *matHeaderCellDef>Lead Time</th>
          <td mat-cell *matCellDef="let m">{{ m.tiempoEntrega }}h</td>
        </ng-container>
        <ng-container matColumnDef="cfr">
          <th mat-header-cell *matHeaderCellDef>CFR</th>
          <td mat-cell *matCellDef="let m">{{ m.tasaCambioFallido }}%</td>
        </ng-container>
        <ng-container matColumnDef="mttr">
          <th mat-header-cell *matHeaderCellDef>MTTR</th>
          <td mat-cell *matCellDef="let m">{{ m.tiempoRestauracion }}h</td>
        </ng-container>
        <ng-container matColumnDef="fecha">
          <th mat-header-cell *matHeaderCellDef>Fecha</th>
          <td mat-cell *matCellDef="let m">{{ m.fecha | date:'dd/MM/yyyy' }}</td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell empty-state" [attr.colspan]="columns.length">
            <mat-icon>bar_chart</mat-icon><p>Sin métricas registradas</p>
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; }
    .filter-field { min-width: 180px; }
    .table-card { padding: 0; margin-top: 0; }
    .dora-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 20px; }
    @media (max-width: 900px) { .dora-grid { grid-template-columns: repeat(2, 1fr); } }
    .dora-card { background: white; border-radius: var(--radius); padding: 20px; box-shadow: 0 1px 3px rgba(0,0,0,.1); text-align: center; }
    .dora-icon { color: var(--color-primary); mat-icon { font-size: 32px; width: 32px; height: 32px; } }
    .dora-metric { font-size: 2rem; font-weight: 700; color: var(--color-primary); margin: 4px 0; }
    .dora-label { font-size: .85rem; font-weight: 600; }
    .dora-sub { font-size: .75rem; color: var(--color-text-secondary); margin-bottom: 8px; }
    .dora-badge { display: inline-block; padding: 2px 10px; border-radius: 12px; font-size: .75rem; font-weight: 600; }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class MetricasComponent implements OnInit {
  readonly loading = signal(false);
  readonly metricas = signal<MetricaSprint[]>([]);

  proyectoFilter = '';
  readonly columns = ['proyecto', 'sprint', 'df', 'lt', 'cfr', 'mttr', 'fecha'];

  ngOnInit(): void { /* TODO: connect to service */ }
  load(): void { /* TODO: connect to service */ }

  promedio(campo: keyof MetricaSprint): number {
    const m = this.metricas();
    if (!m.length) return 0;
    return m.reduce((acc, x) => acc + (x[campo] as number), 0) / m.length;
  }

  doraLevel(metric: string, value: number): string {
    if (metric === 'df') return value >= 7 ? 'Elite' : value >= 1 ? 'High' : value >= 0.14 ? 'Medium' : 'Low';
    if (metric === 'lt') return value <= 24 ? 'Elite' : value <= 168 ? 'High' : value <= 720 ? 'Medium' : 'Low';
    if (metric === 'cfr') return value <= 5 ? 'Elite' : value <= 10 ? 'High' : value <= 15 ? 'Medium' : 'Low';
    if (metric === 'mttr') return value <= 1 ? 'Elite' : value <= 24 ? 'High' : value <= 168 ? 'Medium' : 'Low';
    return '';
  }

  doraClass(metric: string, value: number): string {
    const level = this.doraLevel(metric, value);
    return level === 'Elite' ? 'badge-success dora-badge' :
           level === 'High' ? 'badge-info dora-badge' :
           level === 'Medium' ? 'badge-warning dora-badge' : 'badge-danger dora-badge';
  }
}
