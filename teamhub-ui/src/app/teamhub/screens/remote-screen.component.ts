import { Component, ElementRef, ViewChild, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Option, Row } from '../api';
import { ScreenAction, ScreenData, ScreenNavigation, dateLabel, initials, personName, today } from './screen-support';
const dayKey=(date:Date)=>date.toISOString().slice(0,10);
const monday=(value:string)=>{const d=new Date(value+'T12:00:00Z');d.setUTCDate(d.getUTCDate()-(d.getUTCDay()+6)%7);return dayKey(d);};
@Component({selector:'app-remote-screen',standalone:true,imports:[FormsModule],templateUrl:'./remote-screen.component.html',styleUrl:'./remote-screen.component.scss'})
export class RemoteScreenComponent {
 readonly selectedId=input<number|string|null>(null);readonly revision=input(0);readonly action=output<ScreenAction>();readonly navigate=output<ScreenNavigation>();
 readonly data=inject(ScreenData);readonly api=this.data.api;readonly loading=signal(true);readonly error=signal('');readonly notice=signal('');readonly saving=signal('');readonly records=signal<Record<string,Row[]>>({});readonly week=signal(monday(today()));readonly floor=signal('');readonly selected=signal<number|null>(null);readonly personName=personName;readonly initials=initials;readonly dateLabel=dateLabel;
 @ViewChild('extensions') extensions?:ElementRef<HTMLDialogElement>;
 readonly resources=['persona','area','teletrabajo_plan','logro','ausencia'];
 rows(key:string){return this.records()[key]??[];}
 readonly people=computed(()=>this.rows('persona').filter(p=>p['estado']==='activo'&&(this.api.actor()?.role!=='miembro'||p['id']===this.api.actor()?.id)&&(!this.floor()||String(p['piso'])===this.floor())).sort((a,b)=>personName(a).localeCompare(personName(b))));
 readonly days=computed(()=>Array.from({length:5},(_,i)=>{const d=new Date(this.week()+'T12:00:00Z');d.setUTCDate(d.getUTCDate()+i);return {key:dayKey(d),label:['Lunes','Martes','Miércoles','Jueves','Viernes'][i],short:new Intl.DateTimeFormat('es-EC',{day:'numeric',month:'short',timeZone:'UTC'}).format(d)};}));
 readonly capacity=computed(()=>this.rows('config_oficina').filter(r=>!this.floor()||String(r['piso'])===this.floor()).reduce((sum,r)=>sum+Number(r['cupo']),0));
 readonly candidates=computed(()=>this.people().filter(p=>this.achievements(p).length||this.extensionsFor(p).length));
 readonly selectedPerson=computed(()=>this.people().find(p=>p['id']===this.selected()));
 readonly expanded=computed(()=>this.people().filter(p=>this.activeExtensions(p).length).length);
 constructor(){effect(()=>{this.revision();void this.load();});effect(()=>{const id=this.selectedId();if(id!==null)this.selected.set(Number(id));});}
 async load(){this.loading.set(true);this.error.set('');try{const actor=this.api.actor();if(!actor)throw new Error('Inicia sesión para abrir el planificador.');const [values,office,areas]=await Promise.all([Promise.all(this.resources.map(r=>r==='persona'&&actor.role!=='jefe_desarrollo'?this.api.get<Option[]>('/directory/persona').then(rows=>rows.map(o=>({id:o.id,nombre:o.label,estado:'activo'} as Row))):r==='area'?this.api.get<Option[]>('/directory/area').then(rows=>rows.map(o=>({id:o.id,nombre:o.label} as Row))):this.data.list(r))),this.data.report('oficina'),this.api.get<Option[]>('/directory/area').then(rows=>rows.map(o=>({id:o.id,nombre:o.label} as Row)))]);const manager=actor.role!=='miembro';const extra=manager?await Promise.all(['teletrabajo_ampliacion','teletrabajo_ampliacion_logro'].map(r=>this.data.list(r))):[[],[]];this.records.set({...Object.fromEntries(this.resources.map((r,i)=>[r,r==='area'?areas:values[i]])),config_oficina:office,teletrabajo_ampliacion:extra[0],teletrabajo_ampliacion_logro:extra[1],parametro_sistema:[]});}catch(e){this.error.set(e instanceof Error?e.message:'No se pudo cargar el planificador.');}finally{this.loading.set(false);}}
 shift(offset:number){const date=new Date(this.week()+'T12:00:00Z');date.setUTCDate(date.getUTCDate()+offset*7);this.week.set(dayKey(date));this.notice.set('');}
 currentWeek(){this.week.set(monday(today()));}
 area(p:Row){return this.rows('area').find(a=>a['id']===p['area_id'])?.['nombre']??'Sin área';}
 plan(p:Row,date:string){return this.rows('teletrabajo_plan').find(r=>r['persona_id']===p['id']&&String(r['fecha']).slice(0,10)===date);}
 absent(p:Row,date:string){return this.rows('ausencia').some(a=>a['persona_id']===p['id']&&a['estado']==='aprobado'&&a['tipo']!=='salida_temprana'&&String(a['fecha_inicio']).slice(0,10)<=date&&String(a['fecha_fin']).slice(0,10)>=date);}
 mode(p:Row,date:string){return this.absent(p,date)?'ausente':this.plan(p,date)?.['modalidad']??'sin_plan';}
 label(p:Row,date:string){return ({ausente:'Ausente',oficina:'Oficina',teletrabajo:'Teletrabajo',sin_plan:'Sin plan'} as Record<string,string>)[this.mode(p,date)];}
 office(date:string){return this.people().filter(p=>this.plan(p,date)?.['modalidad']==='oficina').length;}
 unplanned(date:string){return this.people().filter(p=>this.mode(p,date)==='sin_plan').length;}
 remote(p:Row){return this.days().filter(d=>this.plan(p,d.key)?.['modalidad']==='teletrabajo').length;}
 parameter(key:string,fallback:number){return Number(this.rows('parametro_sistema').find(p=>p['clave']===key)?.['valor']??fallback);}
 extensionsFor(p:Row){return this.rows('teletrabajo_ampliacion').filter(r=>r['persona_id']===p['id']).sort((a,b)=>String(b['fecha_inicio']).localeCompare(String(a['fecha_inicio'])));}
 activeExtensions(p:Row){return this.extensionsFor(p).filter(a=>this.days().some((d,i)=>Number(a['dia_semana'])===i+1&&String(a['fecha_inicio']).slice(0,10)<=d.key&&String(a['fecha_fin']).slice(0,10)>=d.key));}
 quota(p:Row){return this.parameter(this.activeExtensions(p).length?'teletrabajo_dias_ampliado':'teletrabajo_dias_base',this.activeExtensions(p).length?3:2);}
 achievements(p:Row){const since=new Date(today()+'T12:00:00Z');since.setUTCMonth(since.getUTCMonth()-3);return this.rows('logro').filter(l=>l['persona_id']===p['id']&&String(l['fecha']).slice(0,10)>=dayKey(since)&&String(l['fecha']).slice(0,10)<=today()).sort((a,b)=>String(b['fecha']).localeCompare(String(a['fecha'])));}
 canEdit(p:Row){const a=this.api.actor();return !!a&&(a.role==='jefe_desarrollo'||a.id===p['id']||(a.role==='jefe_area'&&p['jefe_directo_id']===a.id));}
 canApprove(p:Row){const a=this.api.actor();return !!a&&a.role!=='miembro'&&a.id!==p['id']&&p['jefe_directo_id']===a.id;}
 async toggle(p:Row,date:string){if(this.saving()||!this.canEdit(p)||this.absent(p,date))return;const row=this.plan(p,date);const modalidad=row?.['modalidad']==='oficina'?'teletrabajo':'oficina';this.saving.set(p['id']+date);this.notice.set('');try{const result=await this.api.save('teletrabajo_plan',row?.['id']??null,{persona_id:p['id'],fecha:date,modalidad}) as Row;const updated:Row={...row,...result,persona_id:p['id'],fecha:date,modalidad};if(updated['id']!==undefined)this.records.update(r=>({...r,teletrabajo_plan:[...r['teletrabajo_plan'].filter(x=>x!==row),updated]}));else this.records.update(r=>({...r,teletrabajo_plan:r['teletrabajo_plan']}));if(updated['id']===undefined){const list=await this.data.list('teletrabajo_plan');this.records.update(r=>({...r,teletrabajo_plan:list}));}this.notice.set(`${personName(p)}: ${modalidad==='oficina'?'Oficina':'Teletrabajo'} · ${dateLabel(date)}.`);}catch(e){this.notice.set(e instanceof Error?e.message:'No se pudo guardar la modalidad.');}finally{this.saving.set('');}}
 open(p?:Row){this.selected.set(p?.['id']??this.candidates()[0]?.['id']??null);this.extensions?.nativeElement.showModal();}
 close(){this.extensions?.nativeElement.close();}
 approve(p:Row){this.close();this.action.emit({resource:'teletrabajo_ampliacion',defaults:{persona_id:p['id'],fecha_inicio:this.week(),fecha_fin:this.days()[4].key}});}
 linkedAchievements(extension:Row){return this.rows('teletrabajo_ampliacion_logro').filter(r=>r['ampliacion_id']===extension['id']).map(r=>this.rows('logro').find(l=>l['id']===r['logro_id'])).filter((r):r is Row=>!!r);}
 linked(extension:Row,logro:Row){return this.rows('teletrabajo_ampliacion_logro').some(l=>l['ampliacion_id']===extension['id']&&l['logro_id']===logro['id']);}
 link(extension:Row,logro:Row){this.close();this.action.emit({resource:'teletrabajo_ampliacion_logro',defaults:{ampliacion_id:extension['id'],logro_id:logro['id']}});}
}
