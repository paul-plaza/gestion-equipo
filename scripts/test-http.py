"""HTTP contract and authorization checks. Called with a disposable database."""
import os,json,subprocess,socket,time,urllib.request,urllib.error,http.cookiejar
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
root=Path(__file__).resolve().parent.parent
env=os.environ.copy();env['ConnectionStrings__gestionequipodb']=env['TEAMHUB_TEST_CONNECTION'];env['ASPNETCORE_ENVIRONMENT']='Development';env['DOTNET_ENVIRONMENT']='Development'
with socket.socket() as s:s.bind(('127.0.0.1',0));port=s.getsockname()[1]
base=f'http://127.0.0.1:{port}'
log=open('/tmp/teamhub-http-test.log','w')
process=subprocess.Popen(['dotnet',str(root/'src/GestionEquipo.Api/bin/Debug/net10.0/GestionEquipo.Api.dll'),'--urls',base],cwd=root/'src/GestionEquipo.Api',env=env,stdout=log,stderr=log)
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
checks=0
def request(path,method='GET',data=None,status=200,raw=None,content=None):
 global checks
 body=raw if raw is not None else json.dumps(data).encode() if data is not None else None
 req=urllib.request.Request(base+path,data=body,method=method,headers={'X-TeamHub-Request':'1','Content-Type':content or 'application/json'})
 try:r=client.open(req);code=r.status;body=r.read()
 except urllib.error.HTTPError as e:code=e.code;body=e.read()
 assert code==status,f'{method} {path}: expected {status}, got {code}: {body[:500]}'
 checks+=1
 value=json.loads(body) if body else None
 return value.get('data',value) if isinstance(value,dict) else value
try:
 for _ in range(80):
  try:urllib.request.urlopen(base+'/api/auth/config',timeout=.5);break
  except Exception:time.sleep(.2)
 else:raise RuntimeError('API did not start; see /tmp/teamhub-http-test.log')
 request('/api/v2/me',status=401)
 request('/api/auth/local','POST',{})
 actor=request('/api/v2/me');assert actor['role']=='jefe_desarrollo'
 metadata=request('/api/v2/metadata')
 for r in metadata:
  request('/api/v2/resources/'+r['name'])
 for name in ['contratos','proyectos','carga','sobreasignacion','disponibles','disponibles-pronto','saldos','horas','vacaciones','oficina','objetivos','evolutivos','tecnologias','aplicaciones','dora-frecuencia','dora-entrega','dora-fallos','dora-restauracion','consumo','solapes','salidas','ampliaciones','calendario','cumpleanos']:
  request('/api/v2/reports/'+name)
 request('/api/v2/dashboard')
 people=request('/api/v2/resources/persona')['items'];member=next(p for p in people if p['cedula']=='DEV-ANA')
 row=request('/api/v2/resources/hora_extra')['items'][0];overtime=row['id'];assert row['can_approve']
 request('/api/v2/resolve/hora_extra/'+str(overtime),'POST',{'approve':True},400)
 request('/api/v2/resources/hora_extra/'+str(overtime),'PUT',{'estado':'aprobada'},400)
 boundary='TeamHubTestBoundary'
 raw=(f'--{boundary}\r\nContent-Disposition: form-data; name="file"; filename="evidencia.txt"\r\nContent-Type: text/plain\r\n\r\nEvidencia de prueba\r\n--{boundary}--\r\n').encode()
 request('/api/v2/evidence/'+str(overtime),'POST',raw=raw,content='multipart/form-data; boundary='+boundary)
 request('/api/v2/resolve/hora_extra/'+str(overtime),'POST',{'approve':True})
 request('/api/v2/resolve/hora_extra/'+str(overtime),'POST',{'approve':True},409)
 today=request('/api/v2/dashboard')['today']
 request('/api/v2/resources/recuperacion_hora','POST',{'hora_extra_id':overtime,'fecha':today,'horas':1})
 request('/api/v2/resources/recuperacion_hora','POST',{'hora_extra_id':overtime,'fecha':today,'horas':10},400)
 request('/api/v2/resources/objetivo','POST',{'persona_id':member['id'],'anio':2027,'numero':1,'alcance':'Contrato anual'})
 request('/api/v2/resources/objetivo','POST',{'persona_id':member['id'],'anio':2028,'numero':1,'alcance':'Contrato anual'})
 request('/api/v2/resources/objetivo','POST',{'persona_id':member['id'],'anio':2027,'numero':8,'alcance':'Inválido'},400)
 request('/api/v2/resources/auditoria','POST',{'tabla':'fake'},403)
 request('/api/v2/resources/not_a_table',status=404)
 # Competing recoveries must not spend the same remaining balance twice.
 def recover():
  req=urllib.request.Request(base+'/api/v2/resources/recuperacion_hora',data=json.dumps({'hora_extra_id':overtime,'fecha':today,'horas':2}).encode(),method='POST',headers={'X-TeamHub-Request':'1','Content-Type':'application/json'})
  try:return client.open(req).status
  except urllib.error.HTTPError as e:return e.code
 with ThreadPoolExecutor(max_workers=2) as pool:statuses=list(pool.map(lambda _:recover(),range(2)))
 assert statuses.count(200)==1 and all(s in (200,400,409) for s in statuses),statuses
 checks+=1
 saldo=next(x for x in request('/api/v2/reports/horas') if x['id']==overtime)
 assert saldo['saldo']==0 and saldo['recuperadas']==3
 # Change only the disposable account's DB role to test scope and access control.
 sql=f"update teamhub.persona set rol_app='miembro' where id={actor['id']}"
 subprocess.run(['podman','exec',os.environ['TEAMHUB_TEST_CONTAINER'],'sh','-c','PGPASSWORD="$POSTGRES_PASSWORD" psql -h 127.0.0.1 -U "$POSTGRES_USER" -d "$1" -v ON_ERROR_STOP=1 -c "$2"','sh',os.environ['TEAMHUB_TEST_DATABASE'],sql],check=True,capture_output=True)
 assert request('/api/v2/me')['role']=='miembro'
 assert all(p['id']==actor['id'] for p in request('/api/v2/resources/persona')['items'])
 request('/api/v2/resources/proyecto','POST',{'nombre':'Unauthorized'},403)
 request('/api/v2/resources/objetivo','POST',{'persona_id':member['id'],'anio':2029,'numero':1,'alcance':'Forged owner'},403)
 request('/api/v2/resources/persona/'+str(actor['id']),'PUT',{'rol_app':'jefe_desarrollo'},403)
 request('/api/v2/resources/auditoria',status=403)
 request('/api/v2/resolve/hora_extra/'+str(overtime),'POST',{'approve':True},403)
 assert request('/api/v2/resources/evidencia_hora_extra')['items']==[]
 assert request('/api/v2/reports/horas')==[]
 print(f'{checks} HTTP checks passed, including every module/report and approval with an uploaded file.')
finally:
 process.terminate()
 try:process.wait(timeout=8)
 except subprocess.TimeoutExpired:process.kill();process.wait()
 log.close()
