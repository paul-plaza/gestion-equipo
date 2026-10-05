import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { CommonModule } from '@angular/common';

interface NavItem { label: string; icon: string; route: string; }

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatBadgeModule
  ],
  template: `
    <mat-sidenav-container class="layout-container">
      <mat-sidenav #sidenav [mode]="'side'" [opened]="true" class="sidebar" [class.collapsed]="collapsed()">
        <div class="sidebar-header">
          @if (!collapsed()) {
            <div class="brand">
              <mat-icon class="brand-icon">hub</mat-icon>
              <span class="brand-name">TeamHub</span>
            </div>
          } @else {
            <mat-icon class="brand-icon">hub</mat-icon>
          }
        </div>

        <mat-nav-list class="nav-list">
          @for (item of navItems; track item.route) {
            <a mat-list-item
               [routerLink]="item.route"
               routerLinkActive="active-link"
               [matTooltip]="collapsed() ? item.label : ''"
               matTooltipPosition="right">
              <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
              @if (!collapsed()) {
                <span matListItemTitle>{{ item.label }}</span>
              }
            </a>
          }
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content class="main-content">
        <mat-toolbar class="app-toolbar" color="primary">
          <button mat-icon-button (click)="toggleSidebar()" aria-label="Menú">
            <mat-icon>{{ collapsed() ? 'menu_open' : 'menu' }}</mat-icon>
          </button>
          <span class="toolbar-title">TeamHub Dev</span>
          <span class="spacer"></span>
          <button mat-icon-button matTooltip="Alertas">
            <mat-icon>notifications</mat-icon>
          </button>
          <button mat-icon-button matTooltip="Perfil">
            <mat-icon>account_circle</mat-icon>
          </button>
        </mat-toolbar>

        <div class="content-area">
          <router-outlet />
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .layout-container { height: 100vh; }

    .sidebar {
      width: var(--sidebar-width);
      background: var(--color-primary-dark, #1a237e);
      color: white;
      transition: width 0.2s ease;
      border-right: none;

      &.collapsed { width: 64px; }
    }

    .sidebar-header {
      display: flex;
      align-items: center;
      justify-content: center;
      height: var(--header-height);
      padding: 0 16px;
      border-bottom: 1px solid rgba(255,255,255,.12);
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 10px;
      .brand-icon { color: #f9a825; font-size: 28px; width: 28px; height: 28px; }
      .brand-name { font-size: 1.1rem; font-weight: 700; color: white; white-space: nowrap; }
    }

    .brand-icon { color: #f9a825; }

    .nav-list {
      padding: 8px 0;

      a { color: rgba(255,255,255,.8); border-radius: 6px; margin: 2px 8px; transition: background .2s; }
      a:hover { background: rgba(255,255,255,.12); color: white; }
      a.active-link { background: rgba(255,255,255,.2); color: white; font-weight: 500; }

      mat-icon { color: inherit; }
    }

    .app-toolbar {
      position: sticky;
      top: 0;
      z-index: 100;
      box-shadow: 0 2px 4px rgba(0,0,0,.15);
    }

    .toolbar-title { font-size: 1rem; font-weight: 500; margin-left: 8px; }

    .main-content { background: var(--color-bg); }

    .content-area { padding: 24px; min-height: calc(100vh - var(--header-height)); }
  `]
})
export class MainLayoutComponent {
  readonly collapsed = signal(false);

  readonly navItems: NavItem[] = [
    { label: 'Dashboard',    icon: 'dashboard',       route: '/dashboard' },
    { label: 'Equipo',       icon: 'people',          route: '/equipo' },
    { label: 'Proyectos',    icon: 'folder_special',  route: '/proyectos' },
    { label: 'Horas Extra',  icon: 'schedule',        route: '/horas-extra' },
    { label: 'Permisos',     icon: 'event_available', route: '/permisos' },
    { label: 'Teletrabajo',  icon: 'home_work',       route: '/teletrabajo' },
    { label: 'Logros',       icon: 'emoji_events',    route: '/logros' },
    { label: 'Objetivos',    icon: 'track_changes',   route: '/objetivos' },
    { label: 'Evolutivos',   icon: 'trending_up',     route: '/evolutivos' },
    { label: 'Métricas',     icon: 'bar_chart',       route: '/metricas' },
    { label: 'Catálogos',    icon: 'settings',        route: '/catalogos' },
  ];

  toggleSidebar(): void {
    this.collapsed.update(v => !v);
  }
}
