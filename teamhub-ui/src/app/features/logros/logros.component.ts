import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { Logro } from '../../core/models';

@Component({
  selector: 'app-logros',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule, MatInputModule,
    MatSelectModule, MatFormFieldModule, MatProgressBarModule,
    MatCardModule, MatChipsModule
  ],
  template: `
    <div class="page-header">
      <h1>Logros</h1>
      <button mat-raised-button color="primary">
        <mat-icon>add</mat-icon> Registrar Logro
      </button>
    </div>

    <div class="card filters-row">
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Buscar</mat-label>
        <input matInput [(ngModel)]="search" (ngModelChange)="applyFilter()" placeholder="Título, persona...">
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      <mat-form-field appearance="outline" class="filter-field">
        <mat-label>Año</mat-label>
        <mat-select [(ngModel)]="yearFilter" (ngModelChange)="applyFilter()">
          <mat-option value="">Todos</mat-option>
          @for (y of years; track y) { <mat-option [value]="y">{{ y }}</mat-option> }
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) { <mat-progress-bar mode="indeterminate"></mat-progress-bar> }

    <div class="logros-grid">
      @if (filtered().length === 0) {
        <div class="empty-card">
          <mat-icon>emoji_events</mat-icon>
          <p>Sin logros registrados</p>
        </div>
      }
      @for (l of filtered(); track l.id) {
        <mat-card class="logro-card">
          <mat-card-header>
            <mat-icon mat-card-avatar style="color: #f9a825; font-size: 40px; width: 40px; height: 40px">emoji_events</mat-icon>
            <mat-card-title>{{ l.titulo }}</mat-card-title>
            <mat-card-subtitle>{{ l.persona }} · {{ l.fecha | date:'dd/MM/yyyy' }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>{{ l.descripcion }}</p>
            @if (l.impacto) { <p class="text-muted"><strong>Impacto:</strong> {{ l.impacto }}</p> }
          </mat-card-content>
          <mat-card-actions>
            @if (l.evidenciaOLink) {
              <a mat-button color="primary" [href]="l.evidenciaOLink" target="_blank">
                <mat-icon>open_in_new</mat-icon> Ver evidencia
              </a>
            }
            <button mat-icon-button style="margin-left: auto"><mat-icon>edit</mat-icon></button>
          </mat-card-actions>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .filters-row { display: flex; gap: 12px; flex-wrap: wrap; padding: 16px 20px; }
    .filter-field { min-width: 180px; }
    .logros-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 16px; }
    .logro-card { border-left: 4px solid #f9a825; }
    .empty-card { grid-column: 1/-1; text-align: center; padding: 60px; color: var(--color-text-secondary); mat-icon { font-size: 64px; width: 64px; height: 64px; color: #ccc; display: block; margin: 0 auto 16px; } }
  `]
})
export class LogrosComponent implements OnInit {
  readonly loading = signal(false);
  readonly logros = signal<Logro[]>([]);
  readonly filtered = signal<Logro[]>([]);

  search = '';
  yearFilter = '';
  readonly years = [2026, 2025, 2024, 2023];

  ngOnInit(): void { /* TODO: connect to service */ }

  applyFilter(): void {
    const q = this.search.toLowerCase();
    this.filtered.set(this.logros().filter(l => {
      const matchSearch = !q || l.titulo.toLowerCase().includes(q) || l.persona.toLowerCase().includes(q);
      const matchYear = !this.yearFilter || l.fecha.startsWith(this.yearFilter);
      return matchSearch && matchYear;
    }));
  }
}
