import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

interface PlanSemana {
  personaId: number; persona: string;
  lunes: boolean; martes: boolean; miercoles: boolean; jueves: boolean; viernes: boolean;
}

interface CapacidadDia { dia: string; enOficina: number; capacidadMax: number; }

@Component({
  selector: 'app-teletrabajo',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTableModule, MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <h1>Teletrabajo</h1>
      <div class="flex-row">
        <button mat-icon-button (click)="prevWeek()"><mat-icon>chevron_left</mat-icon></button>
        <span class="week-label">Semana {{ weekStart | date:'dd/MM' }} — {{ weekEnd | date:'dd/MM/yyyy' }}</span>
        <button mat-icon-button (click)="nextWeek()"><mat-icon>chevron_right</mat-icon></button>
        <button mat-raised-button color="primary" style="margin-left: 16px">
          <mat-icon>save</mat-icon> Guardar Plan
        </button>
      </div>
    </div>

    <!-- Capacidad por día -->
    <div class="capacidad-row">
      @for (c of capacidad(); track c.dia) {
        <div class="capacidad-card" [class.over-capacity]="c.enOficina > c.capacidadMax">
          <div class="cap-dia">{{ c.dia }}</div>
          <div class="cap-value">{{ c.enOficina }}/{{ c.capacidadMax }}</div>
          <div class="cap-label" [class.text-danger]="c.enOficina > c.capacidadMax">
            {{ c.enOficina > c.capacidadMax ? 'Excede límite' : 'En oficina' }}
          </div>
        </div>
      }
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="card table-card">
      <table mat-table [dataSource]="planes()" class="full-width">
        <ng-container matColumnDef="persona">
          <th mat-header-cell *matHeaderCellDef>Persona</th>
          <td mat-cell *matCellDef="let p">{{ p.persona }}</td>
        </ng-container>
        @for (dia of dias; track dia.key) {
          <ng-container [matColumnDef]="dia.key">
            <th mat-header-cell *matHeaderCellDef>{{ dia.label }}</th>
            <td mat-cell *matCellDef="let p">
              <button mat-icon-button
                [color]="p[dia.key] ? 'warn' : 'primary'"
                [matTooltip]="p[dia.key] ? 'Teletrabajo' : 'Oficina'"
                (click)="toggleDia(p, dia.key)">
                <mat-icon>{{ p[dia.key] ? 'home_work' : 'business' }}</mat-icon>
              </button>
            </td>
          </ng-container>
        }
        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns;"></tr>
        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell empty-state" [attr.colspan]="columns.length">
            <mat-icon>home_work</mat-icon><p>Sin plan registrado</p>
          </td>
        </tr>
      </table>
    </div>
  `,
  styles: [`
    .week-label { font-weight: 500; padding: 0 8px; }
    .capacidad-row { display: flex; gap: 12px; margin-bottom: 20px; }
    .capacidad-card { flex: 1; min-width: 80px; background: white; border-radius: var(--radius); padding: 12px; text-align: center; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
    .over-capacity { background: #ffebee; }
    .cap-dia { font-size: .8rem; color: var(--color-text-secondary); text-transform: uppercase; }
    .cap-value { font-size: 1.4rem; font-weight: 700; color: var(--color-primary); }
    .cap-label { font-size: .75rem; }
    .table-card { padding: 0; }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class TeletrabajoComponent implements OnInit {
  readonly loading = signal(false);
  readonly planes = signal<PlanSemana[]>([]);
  readonly capacidad = signal<CapacidadDia[]>([
    { dia: 'Lun', enOficina: 0, capacidadMax: 5 },
    { dia: 'Mar', enOficina: 0, capacidadMax: 5 },
    { dia: 'Mié', enOficina: 0, capacidadMax: 5 },
    { dia: 'Jue', enOficina: 0, capacidadMax: 5 },
    { dia: 'Vie', enOficina: 0, capacidadMax: 5 },
  ]);

  weekStart = this.getMonday(new Date());
  get weekEnd(): Date {
    const d = new Date(this.weekStart);
    d.setDate(d.getDate() + 4);
    return d;
  }

  readonly dias = [
    { key: 'lunes', label: 'Lun' },
    { key: 'martes', label: 'Mar' },
    { key: 'miercoles', label: 'Mié' },
    { key: 'jueves', label: 'Jue' },
    { key: 'viernes', label: 'Vie' },
  ];
  readonly columns = ['persona', 'lunes', 'martes', 'miercoles', 'jueves', 'viernes'];

  ngOnInit(): void { /* TODO: connect to service */ }

  prevWeek(): void { this.weekStart = new Date(this.weekStart.getTime() - 7 * 86400000); }
  nextWeek(): void { this.weekStart = new Date(this.weekStart.getTime() + 7 * 86400000); }

  toggleDia(plan: PlanSemana, dia: string): void {
    (plan as unknown as Record<string, boolean>)[dia] = !(plan as unknown as Record<string, boolean>)[dia];
  }

  private getMonday(d: Date): Date {
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
  }
}
