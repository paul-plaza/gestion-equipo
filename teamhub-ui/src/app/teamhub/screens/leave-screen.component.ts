import { Component, computed, effect, inject, input, output, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Option, Row } from '../api';
import { ScreenAction, ScreenData, ScreenNavigation, dateLabel, personName, today } from './screen-support';
interface CalendarEvent {key:string; personId:number; name:string; type:string; start:string; end:string; detail:string; row?:Row;}
export function calendarDate(value:string,offset=0){const d=new Date(value+'T12:00:00Z');d.setUTCDate(d.getUTCDate()+offset);return d.toISOString().slice(0,10);}
@Component({selector:'app-leave-screen',standalone:true,imports:[FormsModule],templateUrl:'./leave-screen.component.html',styleUrl:'./leave-screen.component.scss'})
export class LeaveScreenComponent {
 readonly revision=input(0);readonly selectedId=input<number|string|null>(null);readonly action=output<ScreenAction>();readonly navigate=output<ScreenNavigation>();
 readonly data=inject(ScreenData);readonly api=this.data.api;readonly loading=signal(true);readonly error=signal('');readonly saving=signal(false);readonly feedback=signal('');readonly formError=signal('');
 readonly selected=signal(today());readonly month=signal(today().slice(0,7));readonly records=signal<Record<string,Row[]>>({});readonly enabled=signal(['permiso','vacaciones','cumpleanos','salida_temprana']);
 readonly dateLabel=dateLabel;readonly personName=personName;readonly days=['Lun','Mar','Mié','Jue','Vie','Sáb','Dom'];
 readonly types=[{id:'vacaciones',label:'Vacaciones',icon:'beach_access'},{id:'permiso',label:'Permiso',icon:'event_busy'},{id:'cumpleanos',label:'Cumpleaños',icon:'cake'},{id:'salida_temprana',label:'Salida temprana',icon:'schedule'}];
 form={persona_id:0,tipo:'permiso',fecha_inicio:today(),fecha_fin:today(),hora_salida:'',motivo:'',rfc_id:''};private loadId=0;
 rows(key:string){return this.records()[key]??[];}
 readonly year=computed(()=>Number(this.month().slice(0,4)));
 readonly monthLabel=computed(()=>new Intl.DateTimeFormat('es-EC',{month:'long',year:'numeric',timeZone:'UTC'}).format(new Date(this.month()+'-01T12:00:00Z')));
 readonly people=computed(()=>this.rows('persona').filter(p=>p['estado']==='activo'&&this.canWrite(p)));
 readonly rfcs=computed(()=>this.rows('rfc').filter(r=>r['es_nocturno']||String(r['label']).includes('PROD')));
 readonly events=computed(()=>{
  const result:CalendarEvent[]=this.rows('ausencia').filter(r=>['pendiente','aprobado'].includes(r['estado'])).map(r=>({key:'a'+r['id'],personId:r['persona_id'],name:this.name(r['persona_id']),type:r['tipo'],start:String(r['fecha_inicio']).slice(0,10),end:String(r['fecha_fin']).slice(0,10),detail:[r['estado'],r['hora_salida'],r['motivo']].filter(Boolean).join(' · '),row:r}));
  for(const b of this.rows('cumpleanos')){const day=String(b['fecha']).slice(0,10);if(!b['fecha'])continue;result.push({key:'b'+b['persona_id'],personId:b['persona_id'],name:b['persona'],type:'cumpleanos',start:day,end:day,detail:'Cumpleaños · fecha de nacimiento'});}
  return result;
 });
 readonly cells=computed(()=>{const first=this.month()+'-01';const weekday=(new Date(first+'T12:00:00Z').getUTCDay()+6)%7;const next=new Date(first+'T12:00:00Z');next.setUTCMonth(next.getUTCMonth()+1);next.setUTCDate(0);const count=Math.ceil((weekday+next.getUTCDate())/7)*7;return Array.from({length:count},(_,i)=>{const date=calendarDate(first,i-weekday);return {date,n:Number(date.slice(-2)),outside:date.slice(0,7)!==this.month(),today:date===today(),events:this.eventsOn(date)};});});
 readonly selectedEvents=computed(()=>this.eventsOn(this.selected()));
 readonly monthEvents=computed(()=>this.events().filter(e=>e.start.slice(0,7)<=this.month()&&e.end.slice(0,7)>=this.month()));
 readonly kpis=computed(()=>this.types.map(t=>({...t,value:this.monthEvents().filter(e=>e.type===t.id&&(t.id!=='cumpleanos'||!e.row)).length})));
 constructor(){effect(()=>{this.revision();this.year();untracked(()=>void this.load());});effect(()=>{const id=this.selectedId();if(id!==null)this.selected.set(String(id).slice(0,10));});}
 canWrite(p:Row){const a=this.api.actor();return !!a&&(a.role==='jefe_desarrollo'||a.id===p['id']||(a.role==='jefe_area'&&p['jefe_directo_id']===a.id));}
 name(id:number){const p=this.rows('persona').find(p=>p['id']===id);return p?personName(p):'Persona '+id;}
 label(type:string){return this.types.find(t=>t.id===type)?.label??type;}
 icon(type:string){return this.types.find(t=>t.id===type)?.icon??'event';}
 eventsOn(date:string){return this.events().filter(e=>this.enabled().includes(e.type)&&e.start<=date&&e.end>=date);}
 toggle(type:string){this.enabled.update(v=>v.includes(type)?v.filter(t=>t!==type):[...v,type]);}
 changeMonth(amount:number){const d=new Date(this.month()+'-01T12:00:00Z');d.setUTCMonth(d.getUTCMonth()+amount);const date=d.toISOString().slice(0,10);this.month.set(date.slice(0,7));this.selected.set(date);}
 goToday(){this.month.set(today().slice(0,7));this.selected.set(today());}
 selectDay(date:string){this.selected.set(date);if(date.slice(0,7)!==this.month())this.month.set(date.slice(0,7));}
 setType(type:string){this.form.tipo=type;this.formError.set('');this.feedback.set('');this.adjustDates();}
 adjustDates(){if(!this.form.fecha_inicio)return;if(this.form.tipo==='vacaciones')this.form.fecha_fin=calendarDate(this.form.fecha_inicio,4);else if(['cumpleanos','salida_temprana'].includes(this.form.tipo))this.form.fecha_fin=this.form.fecha_inicio;}
 person(id:number){this.navigate.emit({module:'equipo',id});}
 detail(row:Row){const real=this.rows('ausencia').find(r=>r['id']===row['id'])??row;this.action.emit({resource:'ausencia',row:real,detail:true});}
 balanceWidth(row:Row){const used=Number(row['semanas_tomadas'])||0;const available=Number(row['semanas_disponibles'])||0;return used+available>0?Math.min(100,used/(used+available)*100):0;}
 async load(){const request=++this.loadId;this.loading.set(true);this.error.set('');try{const actor=this.api.actor();if(!actor)throw new Error('Inicia sesión para consultar el calendario.');const [people,absences,rfcs,birthdays,balances,suggestions]=await Promise.all([actor.role==='jefe_desarrollo'?this.data.list('persona'):this.api.get<Option[]>('/directory/persona').then(rows=>rows.map(r=>({id:r.id,nombre:r.label,estado:'activo'} as Row))),this.data.list('ausencia'),this.api.get<Option[]>('/directory/rfc').then(rows=>rows.map(r=>({id:r.id,label:r.label,numero:r.label,es_nocturno:true} as Row))),this.data.report('cumpleanos',this.year()),this.data.report('vacaciones',this.year()),this.data.report('salidas',this.year())]);if(request!==this.loadId)return;this.records.set({persona:people,ausencia:absences,rfc:rfcs,cumpleanos:birthdays,vacaciones:balances,salidas:suggestions});if(!this.people().some(p=>p['id']===Number(this.form.persona_id)))this.form.persona_id=this.people().find(p=>p['id']===actor.id)?.['id']??this.people()[0]?.['id']??0;}catch(e){if(request===this.loadId)this.error.set(e instanceof Error?e.message:'No se pudo cargar el calendario.');}finally{if(request===this.loadId)this.loading.set(false);}}
 suggest(row:Row){this.form.persona_id=row['persona_id'];this.form.tipo='salida_temprana';this.form.fecha_inicio=String(row['fecha_sugerida']).slice(0,10);this.form.rfc_id=String(row['rfc_id']);this.adjustDates();this.formError.set('');this.feedback.set('');}
 async save(){if(this.saving())return;this.feedback.set('');this.formError.set('');this.adjustDates();if(!this.form.persona_id||!this.form.fecha_inicio||!this.form.fecha_fin){this.formError.set('Selecciona la persona y las fechas.');return;}if(this.form.fecha_fin<this.form.fecha_inicio){this.formError.set('La fecha final debe ser igual o posterior a la inicial.');return;}if(this.form.tipo==='vacaciones'&&new Date(this.form.fecha_inicio+'T12:00:00Z').getUTCDay()!==1){this.formError.set('Las vacaciones se registran por semanas de lunes a viernes.');return;}this.saving.set(true);try{await this.api.save('ausencia',null,{persona_id:Number(this.form.persona_id),tipo:this.form.tipo,fecha_inicio:this.form.fecha_inicio,fecha_fin:this.form.fecha_fin,motivo:this.form.motivo.trim()||null,hora_salida:this.form.tipo==='salida_temprana'?(this.form.hora_salida||null):null,rfc_id:this.form.tipo==='salida_temprana'&&this.form.rfc_id?Number(this.form.rfc_id):null});const oldYear=this.year();this.selected.set(this.form.fecha_inicio);this.month.set(this.form.fecha_inicio.slice(0,7));if(oldYear===this.year())await this.load();this.feedback.set('Permiso registrado. Pendiente de aprobación.');this.form.motivo='';}catch(e){this.formError.set(e instanceof Error?e.message:'No se pudo registrar el permiso.');}finally{this.saving.set(false);}}
}
