"""Run DB checks against a disposable database on the current Aspire PostgreSQL.
Connection credentials stay in memory and are never printed.
"""
import json,os,subprocess,uuid
from pathlib import Path
root=Path(__file__).resolve().parent.parent
raw=subprocess.check_output(['aspire','describe','postgres','--format','Json'],text=True)
info=json.loads(raw[raw.index('{'):]);resource=next(r for r in info['resources'] if r['displayName']=='postgres')
cid=resource['properties']['container.id']
container=json.loads(subprocess.check_output(['podman','inspect',cid],text=True))[0]
env=dict(x.split('=',1) for x in container['Config']['Env'] if '=' in x)
port=container['NetworkSettings']['Ports']['5432/tcp'][0]['HostPort']
name='teamhub_test_'+uuid.uuid4().hex[:12]
def sql(statement):
 return subprocess.run(['podman','exec',cid,'sh','-c','PGPASSWORD="$POSTGRES_PASSWORD" psql -h 127.0.0.1 -U "$POSTGRES_USER" -d postgres -v ON_ERROR_STOP=1 -c "$1"','sh',statement],capture_output=True,text=True,check=True)
sql('CREATE DATABASE '+name)
try:
 testenv=os.environ.copy();testenv['TEAMHUB_TEST_CONTAINER']=cid;testenv['TEAMHUB_TEST_DATABASE']=name;testenv['TEAMHUB_TEST_CONNECTION']=f"Host=127.0.0.1;Port={port};Username={env.get('POSTGRES_USER','postgres')};Password={env['POSTGRES_PASSWORD']};Database={name}"
 result=subprocess.run(['dotnet','run','--project','tests/TeamHub.Integration','--no-build'],cwd=root,env=testenv)
 if result.returncode==0:
  result=subprocess.run(['python3','scripts/test-http.py'],cwd=root,env=testenv)
 raise SystemExit(result.returncode)
finally:sql('DROP DATABASE '+name+' WITH (FORCE)')
