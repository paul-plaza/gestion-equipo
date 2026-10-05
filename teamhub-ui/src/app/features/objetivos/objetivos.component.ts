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
import { Objetivo } from '../../core/models';

interface ObjetivoPersona {
  personaId: number; persona: string;
  objetivos: Objetivo[];
  promedioPorcentaje: number;
}

@Component({
  selector: 'app-objetivos',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule, MatSelectModule, MatFormFieldModule,
    MatProgressBarModule, MatTableModule, MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <h1>Objetivos</h1>
      <button mat-raised-button color="primary">
        <mat-icon>add</mat-icon> Nuevo Objetivo
      </button>
    </div>

    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Persona</mat-label>
        <mat-select [(ngModel)]="personaFilter" (ngModelChange)="load()">
          <mat-option value="">Todos</mat-option>
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Año</mat-label>
        <mat-select [(ngModel)]="yearFilter" (ngModelChange)="load()">
          @for (y of years; track y) { <mat-option [value]="y">{{ y }}</mat-option> }
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="personas-grid">
      @if (objetivosPorPersona().length === 0) {
        <div class="card empty-state">
          <mat-icon>track_changes</mat-icon><p>Sin objetivos registrados</p>
        </div>
      }
      @for (op of objetivosPorPersona(); track op.personaId) {
        <div class="card persona-card">
          <div class="persona-header">
            <div>
              <div class="persona-name">{{ op.persona }}</div>
              <div class="text-muted">{{ op.objetivos.length }}/7 objetivos</div>
            </div>
            <div class="progress-circle">{{ op.promedioPorcentaje | number:'1.0-0' }}%</div>
          </div>
          <mat-progress-bar mode="determinate" [value]="op.promedioPorcentaje" style="margin: 8px 0 16px"></mat-progress-bar>
          @for (obj of op.objetivos; track obj.id) {
            <div class="objetivo-row">
              <div class="obj-numero">O{{ obj.numeroObjetivo }}</div>
              <div class="obj-info">
                <div class="obj-alcance">{{ obj.alcance }}</div>
                @if (obj.observaciones) { <div class="text-muted text-sm">{{ obj.observaciones }}</div> }
              </div>
              <div class="obj-pct" [class.completed]="obj.porcentajeAvance === 100">
                {{ obj.porcentajeAvance }}%
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; }
    .filter-field { min-width: 180px; }
    .personas-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 16px; }
    .persona-card { padding: 16px; }
    .persona-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 4px; }
    .persona-name { font-weight: 600; font-size: 1rem; }
    .progress-circle { width: 48px; height: 48px; border-radius: 50%; background: var(--color-primary); color: white; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: .85rem; }
    .objetivo-row { display: flex; align-items: flex-start; gap: 10px; padding: 8px 0; border-bottom: 1px solid var(--color-border); &:last-child { border: none; } }
    .obj-numero { min-width: 28px; height: 28px; border-radius: 50%; background: var(--color-primary); color: white; display: flex; align-items: center; justify-content: center; font-size: .75rem; font-weight: 700; flex-shrink: 0; }
    .obj-info { flex: 1; }
    .obj-alcance { font-size: .9rem; }
    .obj-pct { font-weight: 600; color: var(--color-primary); &.completed { color: var(--color-success); } }
    .text-sm { font-size: .8rem; }
    .empty-state { text-align: center; padding: 40px; mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; } p { color: var(--color-text-secondary); } }
  `]
})
export class ObjetivosComponent implements OnInit {
  readonly loading = signal(false);
  readonly objetivosPorPersona = signal<ObjetivoPersona[]>([]);

  personaFilter = '';
  yearFilter = 2026;
  readonly years = [2026, 2025, 2024];

  ngOnInit(): void { /* TODO: connect to service */ }
  load(): void { /* TODO: connect to service */ }
}
