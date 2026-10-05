import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Row } from '../api';
import { ScreenAction, ScreenData, ScreenNavigation, initials, personName } from './screen-support';
@Component({selector:'app-skills-screen',standalone:true,imports:[FormsModule],templateUrl:'./skills-screen.component.html',styleUrl:'./skills-screen.component.scss'})
export class SkillsScreenComponent {
 readonly revision=input(0); readonly action=output<ScreenAction>(); readonly navigate=output<ScreenNavigation>();
 readonly data=inject(ScreenData); readonly api=this.data.api; readonly loading=signal(true); readonly error=signal(''); readonly records=signal<Record<string,Row[]>>({});
 readonly category=signal(''); readonly query=signal(''); readonly selected=signal<number|null>(null); readonly initials=initials; readonly personName=personName;
 readonly resources=['tecnologia','categoria_tecnologia','nivel_dominio','persona','area','persona_tecnologia','aplicacion','persona_aplicacion'];
 rows(key:string){return this.records()[key]??[];}
 readonly people=computed(()=>this.rows('persona').filter(p=>p['estado']!=='inactivo').sort((a,b)=>String(a['area_id']).localeCompare(String(b['area_id']))||personName(a).localeCompare(personName(b))));
 readonly categories=computed(()=>this.rows('categoria_tecnologia'));
 readonly technologies=computed(()=>this.rows('tecnologia').filter(t=>t['activo']!==false&&(!this.category()||String(t['categoria_id'])===this.category())&&String(t['nombre']).toLocaleLowerCase().includes(this.query().toLocaleLowerCase())));
 readonly levels=computed(()=>[...this.rows('nivel_dominio')].sort((a,b)=>a['orden']-b['orden']));
 readonly groups=computed(()=>this.categories().map(c=>({name:c['nombre'],rows:this.technologies().filter(t=>t['categoria_id']===c['id'])})).filter(c=>c.rows.length));
 readonly areas=computed(()=>{const result:{name:string;span:number}[]=[];for(const p of this.people()){const name=this.rows('area').find(a=>a['id']===p['area_id'])?.['nombre']??'Sin área';if(result.at(-1)?.name===name)result[result.length-1].span++;else result.push({name,span:1});}return result;});
 readonly risks=computed(()=>this.technologies().filter(t=>this.count(t['id'],this.levels().at(-1)?.['id'])<=1));
 constructor(){effect(()=>{this.revision();void this.load();});}
 async load(){this.loading.set(true);this.error.set('');try{const values=await Promise.all(this.resources.map(r=>this.data.list(r)));this.records.set(Object.fromEntries(this.resources.map((r,i)=>[r,values[i]])));}catch(e){this.error.set(e instanceof Error?e.message:'No se pudieron cargar los conocimientos.');}finally{this.loading.set(false);}}
 count(id:number,level?:number){return this.rows('persona_tecnologia').filter(r=>r['tecnologia_id']===id&&this.people().some(p=>p['id']===r['persona_id'])&&(level===undefined||r['nivel_id']===level)).length;}
 cell(tech:number,person:number){return this.rows('persona_tecnologia').find(r=>r['tecnologia_id']===tech&&r['persona_id']===person);}
 level(row?:Row){return this.levels().find(l=>l['id']===row?.['nivel_id']);}
 tone(row?:Row){const index=this.levels().findIndex(l=>l['id']===row?.['nivel_id']);return index<0?'none':index===this.levels().length-1?'advanced':index===0?'basic':'intermediate';}
 experts(t:Row){return this.rows('persona_tecnologia').filter(r=>r['tecnologia_id']===t['id']&&r['nivel_id']===this.levels().at(-1)?.['id']).map(r=>this.people().find(p=>p['id']===r['persona_id'])).filter((p):p is Row=>!!p);}
 backups(t:Row){return this.rows('persona_tecnologia').filter(r=>r['tecnologia_id']===t['id']&&r['nivel_id']!==this.levels().at(-1)?.['id']).map(r=>this.people().find(p=>p['id']===r['persona_id'])).filter((p):p is Row=>!!p);}
 canEdit(person:number){const actor=this.api.actor();const p=this.people().find(p=>p['id']===person);return !!actor&&(actor.role==='jefe_desarrollo'||actor.id===person||(actor.role==='jefe_area'&&p?.['jefe_directo_id']===actor.id));}
 editCell(t:Row,p:Row){if(!this.canEdit(p['id']))return;const row=this.cell(t['id'],p['id']);this.action.emit({resource:'persona_tecnologia',row,defaults:{persona_id:p['id'],tecnologia_id:t['id']}});}
 appPeople(app:Row){return this.rows('persona_aplicacion').filter(r=>r['aplicacion_id']===app['id']).map(r=>({row:r,person:this.people().find(p=>p['id']===r['persona_id'])})).filter(p=>!!p.person);}
 appCount(n:number){return this.rows('aplicacion').filter(a=>n===2?this.appPeople(a).length>=2:this.appPeople(a).length===n).length;}
 goPerson(p:Row){this.navigate.emit({module:'equipo',id:p['id']});}
 exportMatrix(){const csv=[['Tecnología',...this.people().map(p=>personName(p))],...this.technologies().map(t=>[t['nombre'],...this.people().map(p=>this.level(this.cell(t['id'],p['id']))?.['nombre']??'Sin conocimiento')])].map(row=>row.map(v=>'"'+String(v).replaceAll('"','""')+'"').join(';')).join('\r\n');const url=URL.createObjectURL(new Blob(['\ufeff'+csv],{type:'text/csv;charset=utf-8;'}));const a=document.createElement('a');a.href=url;a.download='matriz-conocimientos.csv';a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);}
}
