import { Component, HostListener, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { A11yModule } from '@angular/cdk/a11y';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TeamHubApi, Resource, Field, Row, Option } from './api';
import { modules, Module, Tab } from './modules';
import { PeopleScreenComponent } from './screens/people-screen.component';
import { SkillsScreenComponent } from './screens/skills-screen.component';
import { CapacityScreenComponent } from './screens/capacity-screen.component';
import { ProjectsComponent } from './projects.component';
import { ScreenAction, ScreenNavigation } from './screens/screen-support';
import { OvertimeScreenComponent } from './screens/overtime-screen.component';
import { LeaveScreenComponent } from './screens/leave-screen.component';
import { RemoteScreenComponent } from './screens/remote-screen.component';

@Component({selector:'app-teamhub',standalone:true,imports:[CommonModule,FormsModule,A11yModule,RouterLink,PeopleScreenComponent,SkillsScreenComponent,CapacityScreenComponent,OvertimeScreenComponent,LeaveScreenComponent,RemoteScreenComponent,ProjectsComponent],templateUrl:'./teamhub.component.html'})
export class TeamHubComponent implements OnInit {
 readonly screenRevision=signal(0);readonly screenSelectedId=signal<string|null>(null);
 readonly customScreenIds:string[]=['equipo','conocimientos','asignaciones','horas-extra','permisos','teletrabajo'];
 isCustomScreen(){return this.customScreenIds.includes(this.active().id);}
 readonly api=inject(TeamHubApi);private router=inject(Router);private route=inject(ActivatedRoute);
 readonly modules=modules;readonly groups=['General','Personas y proyectos','Tiempo','Desempeño','Administración'];
 readonly active=signal<Module>(modules[0]);readonly tab=signal<Tab|null>(null);
 readonly metadata=signal<Resource[]>([]);readonly rows=signal<Row[]>([]);readonly dashboard=signal<any>(null);
 readonly loading=signal(true);readonly busy=signal(false);readonly error=signal('');readonly notice=signal('');
 readonly modal=signal<'edit'|'detail'|null>(null);readonly selected=signal<Row|null>(null);
 readonly collapsed=signal(false);readonly mobileOpen=signal(false);readonly notifications=signal(false);
 readonly resource=computed(()=>this.metadata().find(r=>r.name===this.tab()?.resource));
 readonly columns=computed(()=>{
  const r=this.resource();
  if(r?.name==='proyecto')return['nombre','fecha_inicio','fecha_fin','estado','atrasado','costo_total','rom'];
  if(r?.name==='asignacion')return['proyecto_id','persona_id','rol_proyecto','porcentaje','horas_mensuales','fecha_inicio','fecha_fin'];
  if(r) return r.fields.filter(f=>!['id','creado_en','subido_en','fecha_resolucion','entra_object_id','rfc_ambiente','registrado_por','aprobado_por','datos'].includes(f.name)).slice(0,7).map(f=>f.name);
  return Object.keys(this.rows()[0]??{}).filter(k=>!k.endsWith('_id')).slice(0,8);
 });
 directories:Record<string,Option[]>={};form:Row={};editId:string|number|null=null;search='';year=new Date().getFullYear();page=1;total=0;private loadVersion=0;private focusBack?:HTMLElement;private toastTimer?:ReturnType<typeof setTimeout>;
 contractForm:Row={};readonly personaContracts=signal<Row[]>([]);
 readonly months=['ene','feb','mar','abr','may','jun','jul','ago','sep','oct','nov','dic'];
 async ngOnInit() {
  this.route.queryParamMap.subscribe(p=>{this.screenSelectedId.set(p.get('id'));const key=p.get('tab');const t=this.active().tabs.find(t=>t.resource===key||t.report===key);if(t&&t!==this.tab()){this.tab.set(t);if(this.api.actor())void this.load();}});
  this.route.paramMap.subscribe(p=>{this.active.set(modules.find(m=>m.id===p.get('module'))??modules[0]);this.tab.set(this.active().tabs.find(t=>t.resource===this.route.snapshot.queryParamMap.get('tab')||t.report===this.route.snapshot.queryParamMap.get('tab'))??this.active().tabs[0]??null);this.page=1;this.search='';if(this.api.actor()) void this.load();});
  try {await this.api.init();if(this.api.actor()) await this.initialize();} catch(e){this.fail(e);}finally{this.loading.set(false);}
 }
 async initialize(){this.metadata.set(await this.api.get<Resource[]>('/metadata'));await this.load();}
 async login(local=false){this.busy.set(true);this.error.set('');try{await this.api.login(local);await this.initialize();}catch(e){this.fail(e);}finally{this.busy.set(false);}}
 async logout(){try{await this.api.logout();this.rows.set([]);this.dashboard.set(null);}catch(e){this.fail(e);}}
 visibleModules(group:string){return modules.filter(m=>m.group===group && (group!=='Administración'||this.api.actor()?.role==='jefe_desarrollo'));}
 navigate(m:Module){this.mobileOpen.set(false);this.notifications.set(false);void this.router.navigate(['/',m.id]);}
 navigateScreen(destination:ScreenNavigation){this.mobileOpen.set(false);this.notifications.set(false);void this.router.navigate(['/',destination.module],{queryParams:{id:destination.id??null,tab:destination.tab??null}});}
 async openScreenRecord(action:ScreenAction){
  const resource=this.metadata().find(r=>r.name===action.resource);if(!resource){this.fail(new Error('No se pudo cargar este formulario.'));return;}
  try{await this.loadDirectories(resource);this.tab.set(this.active().tabs.find(t=>t.resource===action.resource)??{resource:action.resource,label:resource.label});this.open(action.row??null,action.detail??false);Object.assign(this.form,action.defaults??{});}
  catch(e){this.fail(e);}
 }
 async selectTab(t:Tab){this.tab.set(t);this.page=1;this.search='';await this.router.navigate([],{relativeTo:this.route,queryParams:{tab:t.resource??t.report},queryParamsHandling:'merge'});await this.load();}
 async load(){
  const version=++this.loadVersion;this.loading.set(true);this.error.set('');this.rows.set([]);
  try {
   if(this.isCustomScreen()){this.screenRevision.update(v=>v+1);return;}
   if(this.active().id==='dashboard'){const d=await this.api.get<any>('/dashboard');if(version===this.loadVersion)this.dashboard.set(d);}
   else if(this.tab()?.resource){
    const r=this.resource();if(!r)return;
    const [result]=await Promise.all([this.api.get<any>(`/resources/${r.name}?page=${this.page}&search=${encodeURIComponent(this.search)}`),this.loadDirectories(r)]);
    if(r.name==='proyecto'){
     for(let page=2;result.items.length<result.total;page++){
      const next=await this.api.get<any>(`/resources/proyecto?page=${page}`);
      if(!next.items.length)break;
      result.items.push(...next.items);
     }
    }
    if(version===this.loadVersion){const scoped=this.active().id==='proyectos'&&r.name==='proyecto_aplicacion'&&this.screenSelectedId()?result.items.filter((row:Row)=>String(row['proyecto_id'])===this.screenSelectedId()):result.items;this.rows.set(scoped);this.total=result.total;}
   } else if(this.tab()?.report){
    const result=await this.api.get<Row[]>(`/reports/${this.tab()!.report}?year=${this.year}`);
    if(version===this.loadVersion){this.rows.set(result);this.total=result.length;}
   }
  }catch(e){if(version===this.loadVersion)this.fail(e);}finally{if(version===this.loadVersion)this.loading.set(false);}
 }
 async loadDirectories(r:Resource){await Promise.all([...new Set(r.fields.map(f=>f.reference).filter(Boolean))].map(async ref=>{this.directories[ref!]=await this.api.get<Option[]>('/directory/'+ref);}));}
 canCreate(){const r=this.resource();return r && !r.readonly && r.name!=='evidencia_hora_extra' && (!r.managerOnly||this.api.actor()?.role!=='miembro') && (!['persona','parametro_sistema','config_oficina','cargo','area','rol','categoria_tecnologia','nivel_dominio','tecnologia','aplicacion','indicador_metrica','proveedor','product_owner','contrato_externo'].includes(r.name)||this.api.actor()?.role==='jefe_desarrollo');}
 canEdit(row:Row){const r=this.resource();return this.canCreate() && r && !['pase','rfc','recuperacion_hora','contrato_externo','teletrabajo_ampliacion','teletrabajo_ampliacion_logro'].includes(r.name) && (!['hora_extra','ausencia'].includes(r.name)||row['estado']==='pendiente');}
 canDelete(){return ['objetivo','logro','persona_tecnologia','persona_aplicacion','teletrabajo_plan'].includes(this.resource()?.name??'');}
 today(){return this.dashboard()?.today??new Intl.DateTimeFormat('en-CA',{timeZone:'America/Guayaquil',year:'numeric',month:'2-digit',day:'2-digit'}).format(new Date());}
 open(row:Row|null=null,detail=false){
  this.focusBack=document.activeElement as HTMLElement;this.error.set('');this.selected.set(row);this.editId=row?.[this.resource()!.key]??null;
  this.form={};
  for(const f of this.resource()!.fields.filter(f=>!f.readonly)) {
   // Postgres serializa "date" como "2026-09-03T00:00:00"; <input type="date"> solo acepta "2026-09-03".
   if(row)this.form[f.name]=f.type==='date' && typeof row[f.name]==='string' ? row[f.name].slice(0,10) : row[f.name];
   else if(f.type==='boolean')this.form[f.name]=false;
   else if(f.name==='persona_id')this.form[f.name]=this.api.actor()!.id;
   else if(f.name==='anio')this.form[f.name]=this.year;
   else if(f.name==='fecha'||f.name==='fecha_inicio'||f.name==='fecha_inicio_trabajo')this.form[f.name]=this.today();
   else if(f.name==='mes')this.form[f.name]=this.today().slice(0,7)+'-01';
   else if(f.options.length)this.form[f.name]=f.options[0];
   else if(f.type==='numeric')this.form[f.name]=0;
   else this.form[f.name]='';
  }
  if(this.active().id==='proyectos' && this.screenSelectedId() && !row && this.resource()?.fields.some(f=>f.name==='proyecto_id'))this.form['proyecto_id']=Number(this.screenSelectedId());
  if(this.resource()?.name==='persona' && !row){this.form['rol_app']='miembro';this.form['estado']='activo';}
  if(this.resource()?.name==='proyecto' && !row)this.form['estado']='por_iniciar';
  if(this.resource()?.name==='persona' && !detail){
   this.contractForm={proveedor_id:null,costo_hora:0,fecha_inicio:this.today(),fecha_fin:'',observacion:''};
   this.personaContracts.set([]);
   void this.loadPersonaContracts(row);
  }
  this.modal.set(detail?'detail':'edit');
 }
 private async loadPersonaContracts(row:Row|null){
  if(!this.directories['proveedor'])this.directories['proveedor']=await this.api.get<Option[]>('/directory/proveedor');
  if(row && row['tipo']==='externo'){
   const result=await this.api.get<any>('/resources/contrato_externo?pageSize=100');
   this.personaContracts.set((result.items as Row[]).filter(c=>String(c['persona_id'])===String(row['id'])));
  }
 }
 isExterno(){return this.resource()?.name==='persona' && this.form['tipo']==='externo';}
 vigenteContrato():Row|null{const rows=this.personaContracts();return rows.length?rows.reduce((a,b)=>Date.parse(b['fecha_fin'])>Date.parse(a['fecha_fin'])?b:a):null;}
 proveedorLabel(id:unknown){return this.directories['proveedor']?.find(o=>String(o.id)===String(id))?.label??'—';}
 private validateContractForm(){
  if(this.contractForm['proveedor_id']==null)throw new Error('Selecciona el proveedor del contrato.');
  if(this.contractForm['costo_hora']===null||this.contractForm['costo_hora']===undefined||this.contractForm['costo_hora']==='')throw new Error('Indica el costo por hora del contrato.');
  if(!this.contractForm['fecha_inicio'])throw new Error('Indica la fecha de inicio del contrato.');
  if(!this.contractForm['fecha_fin'])throw new Error('Indica la fecha de fin del contrato.');
  if(this.contractForm['fecha_fin']<this.contractForm['fecha_inicio'])throw new Error('La fecha de fin del contrato debe ser posterior a la de inicio.');
 }
 private async saveContrato(personaId:number,esRenovacion:boolean){
  await this.api.save('contrato_externo',null,{
   persona_id:personaId,
   proveedor_id:Number(this.contractForm['proveedor_id']),
   costo_hora:Number(this.contractForm['costo_hora']),
   fecha_inicio:this.contractForm['fecha_inicio'],
   fecha_fin:this.contractForm['fecha_fin'],
   es_renovacion:esRenovacion,
   observacion:this.contractForm['observacion']||null
  });
 }
 async saveRenewal(){
  if(!this.selected())return;
  this.busy.set(true);this.error.set('');
  try{
   this.validateContractForm();
   await this.saveContrato(Number(this.selected()!['id']),this.personaContracts().length>0);
   this.toast('Contrato registrado');
   await this.loadPersonaContracts(this.selected());this.screenRevision.update(v=>v+1);
   this.contractForm={proveedor_id:null,costo_hora:0,fecha_inicio:this.today(),fecha_fin:'',observacion:''};
  }catch(e){this.fail(e);}finally{this.busy.set(false);}
 }
 close(){if(this.busy())return;this.modal.set(null);this.error.set('');queueMicrotask(()=>this.focusBack?.focus());}
 @HostListener('document:keydown.escape')escape(){this.close();this.mobileOpen.set(false);this.notifications.set(false);}
 fields(){return this.resource()!.fields.filter(f=>!f.readonly && !(this.editId!==null && f.name===this.resource()!.key));}
 visibleField(f:Field){
  if(['pase_corregido_id','motivo_correccion'].includes(f.name))return this.form['tipo']==='correccion';
  if(['atrasado_desde','atrasado_motivo'].includes(f.name))return this.form['atrasado'];
  if(this.resource()?.name==='ausencia' && ['rfc_id','hora_salida'].includes(f.name))return this.form['tipo']==='salida_temprana';
  return true;
 }
 isRequired(f:Field){return f.required||(['atrasado_desde','atrasado_motivo'].includes(f.name)&&this.form['atrasado'])||(['pase_corregido_id','motivo_correccion'].includes(f.name)&&this.form['tipo']==='correccion');}
 multiline(f:Field){return ['descripcion','motivo','atrasado_motivo','motivo_correccion','observacion','observaciones','detalle','alcance','impacto','por_que_se_mide','objetivo'].includes(f.name);}
 inputType(f:Field){return f.type==='date'?'date':f.type==='time'?'time':['integer','smallint','bigint','numeric'].includes(f.type)?'number':f.name.includes('correo')?'email':f.name.includes('url')?'url':'text';}
 async save(){
  const r=this.resource()!;this.busy.set(true);this.error.set('');
  try{
   const payload:Row={};for(const f of this.fields()){
    let value=this.form[f.name];if(!this.visibleField(f))value=null;
    if(value===''||value===undefined)value=null;
    if(this.isRequired(f)&&value===null&&this.visibleField(f))throw new Error(`Completa ${f.label.toLowerCase()}.`);
    if(value!==null && ['integer','smallint','bigint','numeric'].includes(f.type))value=Number(value);
    if(value!==null||this.editId!==null)payload[f.name]=value;
   }
   const creandoExterno=r.name==='persona' && this.editId===null && payload['tipo']==='externo';
   if(creandoExterno)this.validateContractForm();
   const result=await this.api.save(r.name,this.editId,payload) as Row;
   if(creandoExterno)await this.saveContrato(Number(result['id']),false);
   this.modal.set(null);this.toast('Cambios guardados');await this.load();
  }catch(e){this.fail(e);}finally{this.busy.set(false);}
 }
 async resolve(row:Row,approve:boolean){
  this.busy.set(true);this.error.set('');try{await this.api.request(`/api/v2/resolve/${this.resource()!.name}/${row['id']}`,'POST',{approve});this.toast(approve?'Solicitud aprobada':'Solicitud rechazada');this.modal.set(null);await this.load();}catch(e){this.fail(e);}finally{this.busy.set(false);}
 }
 async remove(){if(!this.selected() || !confirm('¿Eliminar este registro? Esta acción no se puede deshacer.'))return;this.busy.set(true);try{await this.api.request(`/api/v2/resources/${this.resource()!.name}/${this.editId}`,'DELETE');this.modal.set(null);this.toast('Registro eliminado');await this.load();}catch(e){this.fail(e);}finally{this.busy.set(false);}}
 async upload(event:Event,row:Row){const input=event.target as HTMLInputElement;const file=input.files?.[0];if(!file)return;this.busy.set(true);try{const data=new FormData();data.append('file',file);await this.api.request(`/api/v2/evidence/${row['id']}`,'POST',data);this.toast('Evidencia adjuntada. La solicitud ya puede revisarse.');}catch(e){this.fail(e);}finally{this.busy.set(false);input.value='';}}
 async download(row:Row){try{await this.api.download(row['id']);}catch(e){this.fail(e);}}
 async changePage(delta:number){this.page+=delta;await this.load();}
 label(key:string){return this.resource()?.fields.find(f=>f.name===key)?.label??({'persona_id':'Persona','proyecto_id':'Proyecto','anio':'Año','porcentaje_total':'Dedicación (%)','dias_para_fin':'Días restantes','dias_restauracion':'Días de restauración','pases_prod':'Pases a producción','porcentaje_fallos':'Fallos (%)','dias_inicio_a_prod':'Inicio → PROD (días)','dias_test_a_prod':'TEST → PROD (días)','__version':'Versión'} as Row)[key]??key.replaceAll('_',' ').replace(/^./,c=>c.toUpperCase());}
 option(value:string){return ({interno:'Interno',externo:'Externo',miembro:'Miembro',jefe_area:'Jefe de área',jefe_desarrollo:'Jefe de desarrollo',por_iniciar:'Por iniciar',en_curso:'En curso',finalizado:'Finalizado',activo:'Activo',inactivo:'Inactivo',pendiente:'Pendiente',aprobada:'Aprobada',aprobado:'Aprobado',rechazada:'Rechazada',rechazado:'Rechazado',cancelado:'Cancelado',no_recuperable:'No recuperable',recuperable:'Recuperable',recuperado:'Recuperado',salida_temprana:'Salida temprana',cumpleanos:'Cumpleaños',scrum_master:'Scrum Master'} as Row)[value]??value.replaceAll('_',' ').replace(/^./,c=>c.toUpperCase());}
 display(row:Row,key:string):string{
  const val=row[key];if(val===null||val===undefined)return '—';if(typeof val==='boolean')return val?'Sí':'No';if(typeof val==='object')return JSON.stringify(val,null,2);
  const f=this.resource()?.fields.find(f=>f.name===key);if(f?.reference)return this.directories[f.reference]?.find(o=>String(o.id)===String(val))?.label??String(val);
  if(f?.options.length||['estado','tipo','estado_recuperacion'].includes(key))return this.option(String(val));
  if(/^\d{4}-\d{2}-\d{2}/.test(String(val)))return new Intl.DateTimeFormat('es-EC',{day:'2-digit',month:'short',year:'numeric',timeZone:'UTC'}).format(new Date(String(val).length===10?val+'T12:00:00Z':val));
  return typeof val==='number'?new Intl.NumberFormat('es-EC',{maximumFractionDigits:2}).format(val):String(val);
 }
 tone(value:any){return ['atrasado','rechazado','rechazada','no_recuperable'].includes(value)?'danger':['pendiente','por_iniciar'].includes(value)?'warning':['aprobado','aprobada','activo','finalizado','recuperable','recuperado'].includes(value)?'success':'info';}
 // RN-27: la marca de atrasado es manual; esto solo señala que la fecha de fin ya pasó sin cerrar el proyecto.
 proyectoVencido(row:Row){return row['fecha_fin']<this.today() && row['estado']!=='finalizado' && !row['atrasado'];}
 estadoAtraso(row:Row):string{return row['atrasado']?'Atrasado':this.proyectoVencido(row)?'Vencido':'A tiempo';}
 formatDate(value:string){return this.display({fecha:value},'fecha');}
 progress(p:Row){const start=Date.parse(p['fecha_inicio']),end=Date.parse(p['fecha_fin']);return Math.max(0,Math.min(100,100*(Date.parse(this.today())-start)/Math.max(1,end-start)));}
 timelineLeft(p:Row){const start=Date.parse(`${this.year}-01-01`),end=Date.parse(`${this.year+1}-01-01`);return Math.max(0,Math.min(100,100*(Date.parse(p['fecha_inicio'])-start)/(end-start)));}
 timelineWidth(p:Row){const start=Date.parse(`${this.year}-01-01`),end=Date.parse(`${this.year+1}-01-01`);return Math.max(1,Math.min(100,100*(Date.parse(p['fecha_fin'])-start)/(end-start))-this.timelineLeft(p));}
 initials(){return this.api.actor()?.name.split(' ').slice(0,2).map(n=>n[0]).join('')??'TH';}
 toast(message:string){this.notice.set(message);clearTimeout(this.toastTimer);this.toastTimer=setTimeout(()=>this.notice.set(''),5000);}
 fail(e:unknown){this.error.set(e instanceof Error?e.message:'Ocurrió un error inesperado.');}
 exportCsv(){const columns=this.columns();const escape=(v:string)=>'"'+(/^[=+@-]/.test(v)?"'":'')+v.replaceAll('"','""')+'"';const content='\uFEFF'+[columns.map(k=>escape(this.label(k))).join(','),...this.rows().map(row=>columns.map(k=>escape(this.display(row,k))).join(','))].join('\r\n');const url=URL.createObjectURL(new Blob([content],{type:'text/csv;charset=utf-8'}));const a=document.createElement('a');a.href=url;a.download=this.active().id+'.csv';a.click();URL.revokeObjectURL(url);}
}
