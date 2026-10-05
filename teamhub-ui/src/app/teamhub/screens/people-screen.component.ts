import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeamHubApi, Row, Option } from '../api';
import { ScreenAction, ScreenNavigation, ScreenData, dateLabel, personName, initials, today } from './screen-support';
@Component({selector:'app-people-screen',standalone:true,imports:[CommonModule,FormsModule],templateUrl:'./people-screen.component.html',styleUrl:'./people-screen.component.scss'})
export class PeopleScreenComponent {
 readonly api=inject(TeamHubApi); readonly data=inject(ScreenData);
 readonly revision=input(0); readonly selectedId=input<number|string|null>(null);
 readonly action=output<ScreenAction>(); readonly navigate=output<ScreenNavigation>();
 readonly records=signal<Record<string,Row[]>>({}); readonly loading=signal(false); readonly errors=signal<Record<string,string>>({}); readonly personId=signal<number|string|null>(null); readonly tab=signal('perfil');
 readonly person=computed(()=>this.rows('persona').find(p=>String(p['id'])===String(this.personId())));
 directories:Record<string,Option[]>={}; q=''; cargo=''; area=''; project=''; tipo='';
 readonly tabs=[['perfil','Perfil y contrato'],['conoc','Conocimientos'],['trabajo','Proyectos y evolutivos'],['tiempo','Horas extra y permisos'],['desemp','Logros y objetivos']];
 readonly date=dateLabel; readonly name=personName; readonly initials=initials; readonly year=Number(today().slice(0,4));
 private version=0;
 constructor(){effect(()=>{this.revision();void this.load();});effect(()=>{this.personId.set(this.selectedId());this.tab.set('perfil');});}
 async load(){const version=++this.version;this.loading.set(true);const records:Record<string,Row[]>={},errors:Record<string,string>={};await Promise.allSettled(['persona','contrato_externo','asignacion','proyecto','persona_aplicacion','persona_tecnologia','hora_extra','recuperacion_hora','ausencia','logro','objetivo','evolutivo_hora'].map(async r=>{try{records[r]=await this.data.list(r);}catch(e){errors[r]=e instanceof Error?e.message:'No se pudo cargar.';}}));await Promise.allSettled(['area','cargo','rol','persona','proveedor','aplicacion','tecnologia','nivel_dominio'].map(async r=>{try{this.directories[r]=await this.api.get<Option[]>('/directory/'+r);}catch(e){errors[r]=e instanceof Error?e.message:'No se pudo cargar el catálogo.';}}));if(version!==this.version)return;this.records.set(records);this.errors.set(errors);this.loading.set(false);}
 rows(resource:string){return this.records()[resource]??[];}
 mine(resource:string){return this.rows(resource).filter(r=>String(r['persona_id'])===String(this.personId()));}
 label(resource:string,id:unknown){return this.directories[resource]?.find(o=>String(o.id)===String(id))?.label??'—';}
 text(value:unknown){return value?String(value).replaceAll('_',' ').replace(/^./,c=>c.toUpperCase()):'—';}
 assignments(id:unknown,active=false){return this.rows('asignacion').filter(a=>String(a['persona_id'])===String(id)&&(!active||(String(a['fecha_inicio']).slice(0,10)<=today()&&String(a['fecha_fin']).slice(0,10)>=today())));}
 projectName(id:unknown){return this.rows('proyecto').find(p=>String(p['id'])===String(id))?.['nombre']??'Proyecto';}
 filtered(){return this.rows('persona').filter(p=>(!this.q||[this.name(p),p['cedula'],p['correo_institucional']].join(' ').toLocaleLowerCase().includes(this.q.toLocaleLowerCase()))&&(!this.cargo||String(p['cargo_id'])===this.cargo)&&(!this.area||String(p['area_id'])===this.area)&&(!this.tipo||p['tipo']===this.tipo)&&(!this.project||this.assignments(p['id'],true).some(a=>String(a['proyecto_id'])===this.project)));}
 count(type:string){return this.rows('persona').filter(p=>p['tipo']===type).length;}
 areaCount(){return new Set(this.rows('persona').map(p=>p['area_id'])).size;}
 clear(){this.q=this.cargo=this.area=this.project=this.tipo='';}
 open(p:Row){this.personId.set(p['id']);this.tab.set('perfil');this.navigate.emit({module:'equipo',id:p['id']});}
 back(){this.personId.set(null);this.navigate.emit({module:'equipo'});}
 edit(resource:string,row?:Row){this.action.emit({resource,row,defaults:{persona_id:this.personId()}});}
 canEditPerson(){return this.api.actor()?.role==='jefe_desarrollo';}
 canManage(){return !!this.api.actor()&&this.api.actor()?.role!=='miembro';}
 latestContract(id:unknown){return this.rows('contrato_externo').filter(c=>String(c['persona_id'])===String(id)).sort((a,b)=>String(b['fecha_fin']).localeCompare(String(a['fecha_fin'])))[0];}
 total(resource:string,key:string){return this.mine(resource).reduce((sum,r)=>sum+Number(r[key]??0),0);}
 recovered(){const ids=this.mine('hora_extra').map(h=>h['id']);return this.rows('recuperacion_hora').filter(r=>ids.includes(r['hora_extra_id'])).reduce((sum,r)=>sum+Number(r['horas']),0);}
 goals(){return this.mine('objetivo').filter(g=>Number(g['anio'])===this.year).sort((a,b)=>a['numero']-b['numero']);}
 average(){const goals=this.goals();return goals.length?Math.round(goals.reduce((s,g)=>s+Number(g['avance']),0)/goals.length):0;}
 evo(){return this.mine('evolutivo_hora').filter(e=>String(e['fecha']).startsWith(String(this.year)));}
 evoHours(){return this.evo().reduce((sum,e)=>sum+Number(e['horas']),0);}
 safeUrl(value:unknown){try{const url=new URL(String(value));return ['https:','http:'].includes(url.protocol)?url.href:null;}catch{return null;}}
 tabResource(){return ({perfil:'persona',conoc:'persona_tecnologia',trabajo:'asignacion',tiempo:'hora_extra',desemp:'logro',datos:'persona',contrato:'contrato_externo',apps:'persona_aplicacion',tech:'persona_tecnologia',proyectos:'asignacion',horas:'hora_extra',permisos:'ausencia',logros:'logro',objetivos:'objetivo',evolutivos:'evolutivo_hora'} as Record<string,string>)[this.tab()];}
 export(){const cell=(v:unknown)=>'"'+String(v??'').replace(/^[=+@-]/,"'").replaceAll('"','""')+'"';const rows=[['Nombre','Correo','Cargo','Área','Tipo','Estado'],...this.filtered().map(p=>[this.name(p),p['correo_institucional'],this.label('cargo',p['cargo_id']),this.label('area',p['area_id']),p['tipo'],p['estado']])];const url=URL.createObjectURL(new Blob(['\ufeff'+rows.map(r=>r.map(cell).join(';')).join('\r\n')],{type:'text/csv;charset=utf-8'}));const a=document.createElement('a');a.href=url;a.download='equipo.csv';a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);}
}
