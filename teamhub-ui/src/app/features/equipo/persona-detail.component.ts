import { Component, OnInit, signal, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { PersonasService } from '../../core/services/personas.service';
import { PersonaDetalle } from '../../core/models';

@Component({
  selector: 'app-persona-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatTabsModule, MatCardModule, MatButtonModule,
    MatIconModule, MatProgressBarModule, MatChipsModule, MatDividerModule
  ],
  template: `
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    } @else if (persona()) {
      <div class="page-header">
        <div class="flex-row">
          <button mat-icon-button routerLink="/equipo"><mat-icon>arrow_back</mat-icon></button>
          <div class="avatar-lg">{{ initials() }}</div>
          <div>
            <h1>{{ persona()!.nombres }} {{ persona()!.apellidos }}</h1>
            <div class="text-muted flex-row">
              <span class="badge" [class]="persona()!.tipo === 'Externo' ? 'badge-warning' : 'badge-info'">
                {{ persona()!.tipo }}
              </span>
              <span>{{ persona()!.area }} — {{ persona()!.cargo }}</span>
            </div>
          </div>
        </div>
        <button mat-raised-button color="primary">
          <mat-icon>edit</mat-icon> Editar
        </button>
      </div>

      <mat-tab-group animationDuration="200ms">
        <!-- Info general -->
        <mat-tab label="General">
          <div class="tab-content">
            <div class="info-grid">
              <div class="info-item">
                <label>Correo</label>
                <span>{{ persona()!.correoInstitucional }}</span>
              </div>
              @if (persona()!.celular) {
                <div class="info-item"><label>Celular</label><span>{{ persona()!.celular }}</span></div>
              }
              @if (persona()!.fechaCumpleanos) {
                <div class="info-item"><label>Cumpleaños</label><span>{{ persona()!.fechaCumpleanos | date:'dd/MM' }}</span></div>
              }
              @if (persona()!.tipo === 'Externo') {
                <div class="info-item">
                  <label>Fin de contrato</label>
                  <span [class]="contratoClass()">{{ persona()!.fechaFinContrato | date:'dd/MM/yyyy' }}</span>
                </div>
              }
              <div class="info-item"><label>Estado</label>
                <span class="badge" [class]="persona()!.estado === 'Activo' ? 'badge-success' : 'badge-neutral'">{{ persona()!.estado }}</span>
              </div>
            </div>
          </div>
        </mat-tab>

        <!-- Conocimientos -->
        <mat-tab label="Conocimientos">
          <div class="tab-content">
            <h3>Tecnologías</h3>
            @if (persona()!.tecnologias.length === 0) {
              <p class="text-muted">Sin tecnologías registradas</p>
            }
            <div class="chips-group">
              @for (t of persona()!.tecnologias; track t.id) {
                <mat-chip>{{ t.nombre }} <small class="text-muted">({{ t.nivel }})</small></mat-chip>
              }
            </div>
            <mat-divider></mat-divider>
            <h3>Aplicaciones Especializadas</h3>
            @if (persona()!.aplicaciones.length === 0) {
              <p class="text-muted">Sin aplicaciones registradas</p>
            }
            <div class="chips-group">
              @for (a of persona()!.aplicaciones; track a.id) {
                <mat-chip>{{ a.nombre }}</mat-chip>
              }
            </div>
          </div>
        </mat-tab>

        <!-- Historial contrato (solo externos) -->
        @if (persona()!.tipo === 'Externo') {
          <mat-tab label="Contrato">
            <div class="tab-content">
              <div class="flex-row" style="justify-content: space-between; margin-bottom: 16px">
                <h3 style="margin: 0">Historial de Renovaciones</h3>
                <button mat-stroked-button color="primary">
                  <mat-icon>autorenew</mat-icon> Renovar
                </button>
              </div>
              @for (r of persona()!.historialContrato; track $index) {
                <div class="contrato-item">
                  <div>Anterior: {{ r.fechaFinAnterior | date:'dd/MM/yyyy' }}</div>
                  <mat-icon>arrow_forward</mat-icon>
                  <div>Nueva: <strong>{{ r.fechaFinNueva | date:'dd/MM/yyyy' }}</strong></div>
                  <div class="text-muted spacer" style="text-align: right">{{ r.fechaRenovacion | date:'dd/MM/yyyy HH:mm' }}</div>
                </div>
              }
            </div>
          </mat-tab>
        }
      </mat-tab-group>
    }
  `,
  styles: [`
    .avatar-lg { width: 52px; height: 52px; border-radius: 50%; background: var(--color-primary); color: white; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; font-weight: 700; flex-shrink: 0; }
    .tab-content { padding: 20px 0; }
    .info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
    .info-item { label { display: block; font-size: .75rem; color: var(--color-text-secondary); text-transform: uppercase; letter-spacing: .5px; margin-bottom: 4px; } }
    .chips-group { display: flex; flex-wrap: wrap; gap: 8px; margin: 12px 0; }
    .contrato-item { display: flex; align-items: center; gap: 12px; padding: 10px; background: #f5f5f5; border-radius: 6px; margin-bottom: 8px; }
  `]
})
export class PersonaDetailComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly svc = inject(PersonasService);
  readonly loading = signal(true);
  readonly persona = signal<PersonaDetalle | null>(null);

  ngOnInit(): void {
    this.svc.getById(+this.id()).subscribe({
      next: p => { this.persona.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  initials(): string {
    const p = this.persona();
    if (!p) return '';
    return `${p.nombres[0]}${p.apellidos[0]}`.toUpperCase();
  }

  contratoClass(): string {
    const fecha = this.persona()?.fechaFinContrato;
    if (!fecha) return '';
    const dias = Math.ceil((new Date(fecha).getTime() - Date.now()) / 86400000);
    if (dias <= 7) return 'text-danger';
    if (dias <= 15) return 'badge badge-warning';
    return '';
  }
}
