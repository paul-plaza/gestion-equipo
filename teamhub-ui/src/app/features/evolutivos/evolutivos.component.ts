import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { Evolutivo, ProgresoEvolutivo } from '../../core/models';

@Component({
  selector: 'app-evolutivos',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTableModule
  ],
  template: `
    <div class="page-header">
      <h1>Horas Evolutivas</h1>
      <button mat-raised-button color="primary">
        <mat-icon>add</mat-icon> Registrar Horas
      </button>
    </div>

    <!-- Ranking de progreso -->
    <div class="card" style="margin-bottom: 20px">
      <h3 style="margin: 0 0 16px">Progreso hacia meta de 300h</h3>
      @if (progreso().length === 0) {
        <p class="text-muted">Sin datos de progreso</p>
      }
      @for (p of progreso(); track p.personaId) {
        <div class="progress-row">
          <div class="progress-persona">{{ p.personaId }}</div>
          <div class="progress-bar-wrap">
            <mat-progress-bar mode="determinate" [value]="p.porcentaje"></mat-progress-bar>
          </div>
          <div class="progress-label">{{ p.horasCompletadas }}/{{ p.metaHoras }}h ({{ p.porcentaje | number:'1.0-0' }}%)</div>
        </div>
      }
    </div>

    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Persona</mat-label>
        <mat-select [(ngModel)]="personaFilter" (ngModelChange)="load()">
          <mat-option value="">Todos</mat-option>
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="aprobadoFilter" (ngModelChange)="load()">
          <mat-option value="">Todos</mat-option>
          <mat-option value="true">Aprobados</mat-option>
          <mat-option value="false">Pendientes</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="card table-card">
      <table mat-table [dataSource]="evolutivos()" class="full-width">
        <ng-container matColumnDef="persona">
          <th mat-header-cell *matHeaderCellDef>Persona</th>
          <td mat-cell *matCellDef="let e">{{ e.persona }}</td>
        </ng-container>
        <ng-container matColumnDef="fecha">
          <th mat-header-cell *matHeaderCellDef>Fecha</th>
          <td mat-cell *matCellDef="let e">{{ e.fecha | date:'dd/MM/yyyy' }}</td>
        </ng-container>
        <ng-container matColumnDef="aplicacion">
          <th mat-header-cell *matHeaderCellDef>Aplicación</th>
          <td mat-cell *matCellDef="let e">{{ e.aplicacion }}</td>
        </ng-container>
        <ng-container matColumnDef="actividad">
          <th mat-header-cell *matHeaderCellDef>Actividad</th>
          <td mat-cell *matCellDef="let e">{{ e.actividad }}</td>
        </ng-container>
        <ng-container matColumnDef="horas">
          <th mat-header-cell *matHeaderCellDef>Horas</th>
          <td mat-cell *matCellDef="let e">{{ e.horas }}h</td>
        </ng-container>
        <ng-container matColumnDef="estado">
          <th mat-header-cell *matHeaderCellDef>Estado</th>
          <td mat-cell *matCellDef="let e">
            <span class="badge" [class]="e.aprobado ? 'badge-success' : 'badge-warning'">
              {{ e.aprobado ? 'Aprobado' : 'Pendiente' }}
            </span>
          </td>
        </ng-container>
        <ng-container matColumnDef="acciones">
          <th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let e">
            @if (!e.aprobado) {
              <button mat-icon-button color="primary"><mat-icon>check_circle</mat-icon></button>
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell empty-state" [attr.colspan]="columns.length">
            <mat-icon>trending_up</mat-icon><p>Sin registros evolutivos</p>
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; margin-bottom: 0; border-radius: var(--radius) var(--radius) 0 0; }
    .filter-field { min-width: 180px; }
    .table-card { padding: 0; border-radius: 0 0 var(--radius) var(--radius); }
    .progress-row { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
    .progress-persona { min-width: 140px; font-weight: 500; }
    .progress-bar-wrap { flex: 1; }
    .progress-label { min-width: 120px; font-size: .85rem; color: var(--color-text-secondary); }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class EvolutivosComponent implements OnInit {
  readonly loading = signal(false);
  readonly evolutivos = signal<Evolutivo[]>([]);
  readonly progreso = signal<ProgresoEvolutivo[]>([]);

  personaFilter = '';
  aprobadoFilter = '';
  readonly columns = ['persona', 'fecha', 'aplicacion', 'actividad', 'horas', 'estado', 'acciones'];

  ngOnInit(): void { /* TODO: connect to service */ }
  load(): void { /* TODO: connect to service */ }
}
