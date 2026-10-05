import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Row } from '../api';
import { ScreenAction, ScreenData, ScreenNavigation, today, dateLabel, personName, initials } from './screen-support';
const DAY = 86400000;
const stamp = (value: unknown) => new Date(String(value).slice(0, 10) + 'T12:00:00Z').getTime();
@Component({ selector: 'app-capacity-screen', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './capacity-screen.component.html', styleUrl: './capacity-screen.component.scss' })
export class CapacityScreenComponent {
    readonly data = inject(ScreenData);
    readonly revision = input(0);
    readonly action = output<ScreenAction>();
    readonly navigate = output<ScreenNavigation>();
    readonly loading = signal(true); readonly error = signal('');
    readonly people = signal<Row[]>([]); readonly assignments = signal<Row[]>([]); readonly projects = signal<Row[]>([]); readonly areas = signal<Row[]>([]); readonly roles = signal<Row[]>([]);
    readonly mode = signal<'weeks' | 'months'>('weeks'); readonly area = signal(''); readonly query = signal(''); readonly offset = signal(0);
    readonly String = String; readonly increment = (n: number) => n + 1; readonly decrement = (n: number) => n - 1;
    newDate(timestamp: number) { return new Date(timestamp).toISOString().slice(0, 10); }
    assignmentPerson(a: Row) { const p = this.people().find(p => p['id'] === a['persona_id']); return p ? personName(p) : a['persona_id_label'] ?? 'Persona'; }
    roleLabel(role: string) { return ({ scrum_master: 'Scrum Master', desarrollador: 'Desarrollador', arquitecto: 'Arquitecto' } as Record<string, string>)[role] ?? role; }
    readonly date = dateLabel; readonly name = personName; readonly initials = initials;
    readonly now = stamp(today());
    private request = 0;
    constructor() { effect(() => { this.revision(); void this.load(); }); }
    async load() {
        const request = ++this.request; this.loading.set(true); this.error.set(''); try {
            const [people, assignments, projects, areas, roles] = await Promise.all(['persona', 'asignacion', 'proyecto', 'area', 'rol'].map(r => this.data.list(r)));
            if (request !== this.request) return; this.people.set(people.filter(p => p['estado'] !== 'inactivo')); this.assignments.set(assignments); this.projects.set(projects); this.areas.set(areas); this.roles.set(roles);
        } catch (e) { if (request === this.request) this.error.set(e instanceof Error ? e.message : 'No se pudo cargar la capacidad.'); } finally { if (request === this.request) this.loading.set(false); }
    }
    canManage() { return ['jefe_desarrollo', 'jefe_area'].includes(this.data.api.actor()?.role ?? ''); }
    readonly periods = computed(() => {
        const d = new Date(this.now), periods: { start: number; end: number; label: string; sub: string; group: string }[] = [];
        if (this.mode() === 'weeks') {
            const start = this.now - ((d.getUTCDay() + 6) % 7) * DAY + this.offset() * 18 * 7 * DAY;
            for (let i = 0; i < 18; i++) { const s = start + i * 7 * DAY; periods.push({ start: s, end: s + 6 * DAY, label: `${new Date(s).getUTCDate()}`, sub: new Intl.DateTimeFormat('es', { month: 'short', timeZone: 'UTC' }).format(s), group: new Intl.DateTimeFormat('es', { month: 'long', year: 'numeric', timeZone: 'UTC' }).format(s) }); }
        } else { for (let i = 0; i < 9; i++) { const s = Date.UTC(d.getUTCFullYear(), d.getUTCMonth() + this.offset() * 9 + i, 1, 12); periods.push({ start: s, end: Date.UTC(new Date(s).getUTCFullYear(), new Date(s).getUTCMonth() + 1, 0, 12), label: new Intl.DateTimeFormat('es', { month: 'short', timeZone: 'UTC' }).format(s), sub: String(new Date(s).getUTCFullYear()), group: String(new Date(s).getUTCFullYear()) }); } }
        return periods;
    });
    readonly groups = computed(() => { const groups: { label: string; count: number }[] = []; for (const p of this.periods()) { const last = groups.at(-1); if (last?.label === p.group) last.count++; else groups.push({ label: p.group, count: 1 }); } return groups; });
    readonly filtered = computed(() => this.people().filter(p => (!this.area() || String(p['area_id']) === this.area()) && String(personName(p)).toLocaleLowerCase().includes(this.query().toLocaleLowerCase())));
    forPerson(p: Row) { return this.assignments().filter(a => a['persona_id'] === p['id']); }
    loadAt(assignments: Row[], day: number) { return assignments.reduce((sum, a) => sum + (stamp(a['fecha_inicio']) <= day && stamp(a['fecha_fin']) >= day ? Number(a['porcentaje']) : 0), 0); }
    maximum(assignments: Row[], start: number, end: number) { const times = [start, ...assignments.map(a => stamp(a['fecha_inicio'])).filter(t => t >= start && t <= end)]; return Math.max(0, ...times.map(t => this.loadAt(assignments, t))); }
    tone(value: number) { return value > 100 ? 'over' : value === 100 ? 'full' : value > 0 ? 'partial' : 'free'; }
    role(p: Row) { return this.roles().find(r => r['id'] === p['rol_id'])?.['nombre'] ?? 'Sin rol'; }
    project(a: Row) { return this.projects().find(p => p['id'] === a['proyecto_id']); }
    projectName(a: Row) { return this.project(a)?.['nombre'] ?? a['proyecto_id_label'] ?? 'Proyecto'; }
    position(time: number) { const periods = this.periods(); return Math.max(0, Math.min(100, (time - periods[0].start) / (periods.at(-1)!.end + DAY - periods[0].start) * 100)); }
    readonly todayPosition = computed(() => { this.periods(); return this.position(this.now); });
    readonly showToday = computed(() => this.now >= this.periods()[0].start && this.now <= this.periods().at(-1)!.end);
    readonly rows = computed(() => this.filtered().map(person => { const assignments = this.forPerson(person); return { person, now: this.loadAt(assignments, this.now), lanes: assignments.filter(a => stamp(a['fecha_fin']) >= this.periods()[0].start && stamp(a['fecha_inicio']) <= this.periods().at(-1)!.end).map(a => ({ assignment: a, left: this.position(stamp(a['fecha_inicio'])), width: this.position(stamp(a['fecha_fin']) + DAY) - this.position(stamp(a['fecha_inicio'])), hue: (Number(a['proyecto_id']) * 67 + 205) % 360 })), load: this.periods().map(p => this.maximum(assignments, p.start, p.end)) }; }));
    readonly soon = computed(() => this.filtered().flatMap(person => { const assignments = this.forPerson(person), ends = assignments.map(a => stamp(a['fecha_fin'])).filter(t => t >= this.now && t <= this.now + 60 * DAY); if (!ends.length) return []; const end = Math.min(...ends), free = Math.max(0, 100 - this.loadAt(assignments, end + DAY)); if (!free) return []; return [{ person, end, free, from: new Date(end + DAY).toISOString().slice(0, 10), days: Math.round((end - this.now) / DAY), ending: assignments.filter(a => stamp(a['fecha_fin']) === end).map(a => this.projectName(a)).join(', ') }]; }).sort((a, b) => a.end - b.end));
    readonly kpis = computed(() => {
        const loads = this.filtered().map(p => this.loadAt(this.forPerson(p), this.now)), assigned = loads.reduce((s, v) => s + Math.min(100, v), 0) / 100, free = loads.reduce((s, v) => s + Math.max(0, 100 - v), 0) / 100; return [
            { label: 'Capacidad total', value: String(loads.length), unit: 'FTE', note: `${loads.length} personas activas`, icon: 'groups', tone: 'partial' },
            { label: 'Asignado hoy', value: assigned.toLocaleString('es', { maximumFractionDigits: 2 }), unit: 'FTE', note: `${loads.length ? Math.round(assigned / loads.length * 100) : 0}% de la capacidad`, icon: 'pie_chart', tone: 'full' },
            { label: 'Sobreasignados', value: String(loads.filter(v => v > 100).length), unit: 'personas', note: 'Dedicación total > 100%', icon: 'warning', tone: 'over' },
            { label: 'Disponible hoy', value: free.toLocaleString('es', { maximumFractionDigits: 2 }), unit: 'FTE', note: 'Capacidad sin asignar', icon: 'event_available', tone: 'partial' },
            { label: 'Se liberan en 60 días', value: String(this.soon().length), unit: 'personas', note: 'Finalizan una asignación', icon: 'schedule', tone: 'partial' }];
    });
    readonly upcoming = computed(() => this.projects().filter(p => p['estado'] === 'por_iniciar').sort((a, b) => stamp(a['fecha_inicio']) - stamp(b['fecha_inicio'])));
    projectTeam(p: Row) { return this.assignments().filter(a => a['proyecto_id'] === p['id']); }
    assign(project?: Row, person?: Row) { if (!this.canManage()) return; this.action.emit({ resource: 'asignacion', defaults: { ...(project ? { proyecto_id: project['id'], fecha_inicio: project['fecha_inicio'], fecha_fin: project['fecha_fin'] } : {}), ...(person ? { persona_id: person['id'] } : {}) } }); }
    switchMode(mode: 'weeks' | 'months') { this.mode.set(mode); this.offset.set(0); }
}
