# REQUISITOS — TeamHub Dev

Aplicación web para que el Jefe de Desarrollo gestione su equipo: personas, proyectos, asignaciones, horas extra, permisos, teletrabajo, logros, objetivos y métricas.

> **Cómo usar este documento:** es la fuente de verdad de lo que la aplicación debe hacer. Las reglas de negocio llevan código (RN-xx) para referenciarlas en el código y en las pruebas. Lo marcado como **[POR CONFIRMAR]** es una suposición que debe validarse antes de programar.
>
> **Versión 8:** incorpora las respuestas del Jefe de Desarrollo a las preguntas abiertas de las versiones anteriores, los proveedores de las personas externas, las horas mensuales y presupuestadas por asignación, el Product Owner (PO) del proyecto y la facturación a externos (mes completo).

---

## 1. Objetivo

Dar al Jefe de Desarrollo una vista única y confiable para:

1. Saber quién está en qué proyecto, con qué dedicación y cuándo termina su asignación, para poder reasignar a tiempo.
2. Llevar un registro claro de horas extra, permisos, vacaciones, cumpleaños y teletrabajo, con evidencia y reglas aplicadas de forma consistente.
3. Registrar los logros y aportes extra de cada persona para reconocerlos (por ejemplo, ampliar teletrabajo).
4. Medir capacidad, velocidad y madurez del equipo con indicadores definidos (marco DORA).
5. Dar seguimiento a los objetivos individuales y a las 300 horas de evolutivos de cada desarrollador.

## 2. Alcance

**Incluye:** gestión de personas, cargos y áreas; conocimientos; proyectos y presupuesto; aplicaciones y trazabilidad de pases; asignaciones y capacidad; horas extra y recuperación; permisos, vacaciones y cumpleaños; teletrabajo; logros; objetivos; evolutivos; métricas; alertas y dashboards.

**No incluye (por ahora):** nómina, cálculo de pagos, reemplazo de Azure DevOps (solo se enlaza al tablero y, en una fase posterior, se consulta su API en modo lectura), control biométrico de asistencia.

## 3. Usuarios y roles

| Rol | Descripción | Permisos principales |
|---|---|---|
| Jefe de Desarrollo | Usuario principal | Acceso total: crea, edita, aprueba y consulta todo |
| Jefe de Modernización, de Mantenimiento y de Arquitectura | Usuarios de la aplicación | Consultan y **aprueban permisos, beneficios y horas extra** de los colaboradores que tienen asignados |
| Persona del equipo | Desarrollador, scrum master o arquitecto | Registrar sus horas extra, logros y evolutivos; ver sus datos, objetivos y calendario |

**RN-01.** Una persona solo ve sus propios datos personales (cédula, celular, contrato). El Jefe de Desarrollo ve los de todos. Los demás jefes ven los de sus colaboradores asignados.

**RN-31.** Un jefe solo puede aprobar permisos, beneficios (por ejemplo, la ampliación de teletrabajo) y horas extra de las personas que tiene asignadas como colaboradores. El Jefe de Desarrollo puede aprobar los de todos.

**Autenticación:** inicio de sesión con **Microsoft Entra ID** (cuentas institucionales). El rol de cada usuario se asigna dentro de la aplicación.

## 4. Estructura organizacional

Cada persona pertenece a un área y está bajo un cargo:

| Cargo | Equipo / Área |
|---|---|
| Jefe de Modernización | Equipo scrum |
| Jefe de Mantenimiento | Desarrollo evolutivo |
| Líder de Desarrollo | Equipo de desarrollo |
| Jefe de Arquitectura | Arquitectos |

Los cargos y áreas deben ser catálogos editables (no valores fijos en código).

## 5. Módulos y entidades

### 5.1 Equipo (personas)

**Campos de la persona:**
- Nombres, apellidos
- Cédula
- Correo institucional
- Celular
- Tipo: **interno** o **externo**
- Si es externo: **proveedor** (por ejemplo Netby o Adalove), **costo por hora**, fecha de ingreso, fecha de fin de contrato e historial de renovaciones (cada renovación con nueva fecha de fin, y puede cambiar de costo)
- Rol (desarrollador, scrum master, arquitecto, etc.)
- Área y cargo
- Fecha de cumpleaños
- Piso de oficina asignado (3 u 8)
- Jefe directo (el jefe al que está asignada como colaborador)
- Estado (activo / inactivo)

**Reglas:**
- **RN-02.** La cédula y el correo institucional son únicos.
- **RN-03.** Los externos deben tener proveedor, costo por hora, fecha de ingreso y fecha de fin de contrato. Los internos no.
- **RN-04.** Al renovar un contrato externo se conserva el historial; la fecha de fin vigente es la de la última renovación.
- **RN-05.** Alerta cuando faltan 30, 15 y 7 días para el fin de contrato de un externo, indicando su proveedor.
- **RN-34.** Los proveedores son un catálogo editable (nombre, estado activo). Cada contrato de un externo pertenece a un proveedor y tiene su propio **costo por hora**, por lo que una persona puede cambiar de proveedor o de costo al renovar sin perder el historial. Como el costo está en el contrato, puede ser distinto por proveedor, por persona o por rol.
- **RN-35.** El costo y la facturación de las personas externas son información confidencial: solo los ve el Jefe de Desarrollo. **[POR CONFIRMAR]**
- **RN-36.** Se puede filtrar y consultar al equipo externo por proveedor (personas activas, contratos por vencer y costo por proveedor).

### 5.2 Conocimientos y habilidades

- Cada persona tiene **especializaciones en aplicaciones internas** (catálogo de aplicaciones).
- Cada persona tiene **conocimientos en tecnologías y herramientas**, clasificados por categoría (por ejemplo backend, frontend, bases de datos, cloud, DevOps, testing, herramientas) y con nivel de dominio (básico, intermedio, avanzado). Categorías y niveles son catálogos editables.
- **Dashboard de conocimientos:** matriz persona × tecnología, cobertura por tecnología, mapa de aplicaciones con sus especialistas.
- **RN-06.** Señalar como riesgo toda tecnología o aplicación con un solo experto.

### 5.3 Proyectos

**Campos del proyecto:**
- Nombre
- Link al tablero de Azure DevOps (donde viven las actividades del proyecto)
- Fecha de inicio y fecha de fin
- Presupuesto: horas de desarrollador, horas de scrum, **costo total** y **ROM** (costo máximo)
- Estado: por iniciar, en curso, finalizado.
- Marca de **atrasado**: se activa y se desactiva manualmente, con fecha y motivo.
- **Product Owner (PO):** contacto a quien se envían las facturas del equipo externo. Datos: nombres, apellidos, correo, celular, cargo y área. Los PO son un catálogo reutilizable (un PO puede serlo de varios proyectos). Por ahora es un dato de contacto, no un usuario de la aplicación.
- Equipo asignado
- Aplicaciones que se modifican

**Equipo del proyecto (asignaciones):**
- Scrum Master: dedicación 25%, 50% o 100%
- Desarrollador: dedicación 50% o 100%
- Arquitecto: dedicación de 25% hasta 100%
- Cada asignación tiene fecha de inicio y fin propias.
- Cada asignación tiene **horas mensuales**: un mes tiene 160 horas laborables, así que un desarrollador al 100% tiene 160 horas al mes y un scrum al 50% tiene 80. Se calculan a partir de la dedicación (base de 160 horas, configurable) y se pueden ajustar. Se registran para internos y externos, pero solo se pagan a los externos.
- Cada asignación puede tener **horas presupuestadas**: el total de horas de esa persona en el proyecto. Ejemplo: 3 desarrolladores de 350 horas cada uno y un scrum al 50%, que equivale a la mitad de horas (175).
- Una asignación puede registrarse como **reemplazo** de otra del mismo proyecto y rol (por ejemplo, un interno que pasa a otro proyecto y es reemplazado por un externo).

**Reglas:**
- **RN-07.** La dedicación total de una persona en un mismo período no debe superar el 100%. Si se supera, mostrar alerta de sobreasignación.
- **RN-08.** El costo total no debe superar el ROM. Alertar cuando el costo total alcance el **80% del ROM** y de nuevo si lo supera.
- **RN-09.** Mostrar el consumo de horas por rol: reales vs presupuestadas, y planificadas (según las horas mensuales de las asignaciones) vs presupuestadas.
- **RN-10.** Alerta cuando una asignación o el proyecto está por terminar (30/15/7 días).
- **RN-27.** Un proyecto está **atrasado** solo cuando el Jefe de Desarrollo lo marca manualmente (con fecha y motivo). Esta marca bloquea la recuperación de horas (RN-16). El sistema únicamente alerta cuando la fecha de fin ya pasó y el proyecto no está finalizado ni marcado como atrasado, para que se revise.
- **RN-37.** Las horas mensuales de cada asignación son las que se pagan a los externos: pago mensual = horas mensuales × costo por hora del contrato vigente. A los proveedores siempre se les paga el **mes completo**: se factura cada mes en que la asignación tiene al menos un día, sin prorrateo por días. Los internos no se facturan porque ya tienen salario.
- **RN-38.** Si a mitad de proyecto un interno pasa a otro proyecto y entra un externo, se cierra la asignación del interno (fecha de fin) y se crea la del externo como reemplazo, con el mismo proyecto y rol. La facturación del externo empieza en su fecha de inicio, y ese mes se paga completo (RN-37).
- **RN-39.** No se puede asignar a un externo a un proyecto sin PO, ni quitar el PO a un proyecto que tiene externos asignados, porque a él se envían las facturas.
- **RN-40.** Facturación mensual por proyecto: para cada mes se lista a los externos con sus horas, costo por hora y monto estimado, agrupado por proveedor y PO. Sirve de base para enviar las facturas. El sistema no emite facturas.
- **RN-41.** Alerta cuando la asignación de un externo termina después del fin de su contrato vigente, o cuando no tiene contrato.
- **RN-42.** Alerta cuando las horas planificadas de una asignación (meses × horas mensuales) superan sus horas presupuestadas. Ejemplo: 3 meses × 160 = 480 horas frente a 350 presupuestadas.

### 5.4 Aplicaciones, RFC y trazabilidad

**Catálogo de aplicaciones:** cada aplicación (API) se elige de un catálogo seleccionable y se puede agregar una nueva si no existe. Cada aplicación guarda además el **proyecto y repositorio de Azure DevOps** donde está su código (será la clave para integrar métricas, ver 5.12).

**Trazabilidad dentro de cada proyecto**, por aplicación:
1. **Fecha de inicio de trabajo** en esa aplicación.
2. **Pase a TEST:** requiere un **RFC de TEST** (formato `RFC-xxxxx`) y fecha.
3. **Pase a PROD:** requiere un **RFC de PROD** (otro RFC, formato `RFC-xxxxx`) y fecha. Lo esperado es un solo pase a PROD por aplicación, pero si algo sale mal en producción se registran **pases adicionales de corrección en posproducción**.

**Modelo del RFC:** un mismo RFC puede mover **varias aplicaciones** a TEST o a PROD. Por eso el RFC es una entidad propia (número, ambiente TEST o PROD, fecha) relacionada con una o más aplicaciones del proyecto.

**Reglas:**
- **RN-11.** El orden es obligatorio: no se puede registrar pase a TEST sin fecha de inicio, ni pase a PROD sin un pase a TEST previo de esa aplicación. Esto incluye las correcciones en posproducción: cada corrección debe pasar primero por TEST con su propio RFC de TEST.
- **RN-12.** Validar el formato del RFC y que el número sea único por ambiente.
- **RN-13.** Una aplicación puede tener varios pases a PROD dentro del mismo proyecto. Cada pase tiene su RFC, su fecha y un tipo: **normal** o **corrección en posproducción**. Un pase de corrección se vincula al pase que corrige.
- **RN-28.** Un pase a PROD de tipo **corrección** debe indicar el pase que corrige y el motivo. Un pase que tiene al menos una corrección cuenta como **fallido** para las métricas (5.12), y el tiempo hasta el pase de corrección es su tiempo de restauración.
- **RN-29.** Cuando un pase a PROD es nocturno, el sistema sugiere registrar la salida temprana de las personas involucradas (ver 5.7). La sugerencia no se aplica sola.

### 5.5 Asignaciones y capacidad

- Vista por persona: en qué proyecto(s) está, con qué porcentaje y hasta cuándo.
- Panel **"Disponibles pronto":** personas cuya asignación termina en los próximos **30 días**, junto a los proyectos por iniciar donde podrían asignarse.
- Dashboard de proyectos sobre una **línea de tiempo anual** (inicio y fin respecto al año en curso), con marca del día actual.
- Dashboard para ver quién está por terminar su proyecto.

### 5.6 Horas extra y recuperación

**Registro de horas extra** (cuando alguien se queda fuera de horario; en la práctica, por pases a producción):
- **La persona registra**; aprueba el jefe al que está asignada la persona o el Jefe de Desarrollo (RN-31).
- Persona, fecha
- Actividad realizada (obligatoria)
- Tiempo (obligatorio)
- Evidencias adjuntas (obligatorias)
- Tipo: **pase a producción** (obligatorio vincular el RFC de PROD). Es el único tipo de horas extra.
- Proyecto relacionado
- Estado: pendiente, aprobado, rechazado, recuperado, no recuperable

**Reglas:**
- **RN-14.** No se puede aprobar un registro sin actividad, tiempo y al menos una evidencia.
- **RN-15.** Durante el desarrollo y las pruebas **no se registran horas extra**: esas actividades deben cumplirse dentro de las 8 horas de trabajo. Las horas extra solo aplican a **pases a producción**. El apoyo extra que no sea un pase se reconoce únicamente como logro (ver 5.9).
- **RN-16.** **No se puede recuperar tiempo si el proyecto está atrasado** (RN-27), porque cumplir el proyecto es prioridad y las 8 horas no siempre son efectivas. La acción de recuperar se deshabilita y se muestra el motivo.
- **RN-18.** Cada persona tiene un saldo de horas extra y de horas recuperadas.
- **RN-19.** El registro de horas extra aprobadas también cuenta como aporte para sugerir la ampliación de teletrabajo (ver 5.8).

### 5.7 Permisos, vacaciones y cumpleaños

- **Permisos** solicitados: persona, fecha(s), motivo, estado. Los aprueba el jefe al que está asignada la persona o el Jefe de Desarrollo (RN-31).
- **Salida temprana** cuando hay pase a producción en la noche: persona, fecha, motivo (pase a producción), relacionado con el RFC de PROD.
- **Vacaciones:**
  - Cada persona tiene **2 semanas de vacaciones al año**, que equivalen a 11 días laborables (15 días corridos contando los 4 días de fin de semana, que no se descuentan).
  - Las dos semanas se toman **por separado**: no se pueden tomar juntas.
  - Cada solicitud cubre **una semana completa** (de lunes a viernes).
  - Las vacaciones **no se acumulan** al año siguiente: la semana que no se tome en el año se pierde. **[POR CONFIRMAR si el año es el calendario]**
- **Cumpleaños:**
  - Cada persona tiene un **día libre** por su cumpleaños.
  - Se puede tomar el **mismo día**, o moverlo al **lunes** o al **viernes de la misma semana** del cumpleaños.
- **Calendario mensual del equipo** con eventos de colores: cumpleaños, vacaciones, permisos, salidas tempranas.
- **RN-20.** Cada año se genera automáticamente el día de cumpleaños a partir de la fecha de nacimiento. La persona o el jefe elige entre las tres opciones de fecha.
- **RN-21.** Alertar cuando se solapan ausencias de muchas personas del mismo proyecto.
- **RN-30.** No se puede solicitar más de 2 semanas de vacaciones al año sin aprobación explícita del jefe.
- **RN-33.** Cada solicitud de vacaciones cubre exactamente una semana completa (lunes a viernes) y las dos semanas del año no pueden solicitarse juntas.

### 5.8 Teletrabajo

- Beneficio base: **2 días de teletrabajo por semana** por persona.
- **Ampliación temporal a 3 días:** el jefe (el Jefe de Desarrollo o el jefe al que está asignada la persona) la crea indicando **fecha de inicio, fecha de fin y el día de la semana** adicional. Debe poder vincularla a los logros o aportes extra que la justifican.
- **RN-22.** El sistema **sugiere** la ampliación según los logros y aportes registrados, pero la decisión y las fechas las pone el jefe. Al llegar la fecha de fin, la persona vuelve automáticamente a 2 días.
- **RN-23.** Cupo de oficina: **15 puestos en el piso 8 y 15 puestos en el piso 3**. Cada persona tiene un piso asignado y el cupo de cada piso se cuenta con las personas de ese piso que van a la oficina ese día. No permitir (o alertar) cuando una planificación supera el cupo de un piso ese día.
- Planificador semanal: grilla persona × día (lunes a viernes), con contador de puestos ocupados por piso.

### 5.9 Logros y aportes extra

- Registro individual de logros: persona, título, descripción, impacto, evidencia o link, fecha.
- Ejemplos: crear un skill de revisión de código de base de datos basado en los estándares de arquitectura; empaquetar un NuGet y dejarlo disponible en DevOps.
- **RN-24.** Un logro debe tener descripción y evidencia o link.
- El apoyo extra que no sea un pase a producción se registra **solo como logro**, no como horas extra.
- Línea de tiempo de logros por persona, usada como respaldo para ampliar teletrabajo y para evaluaciones.

### 5.10 Objetivos

- Cada desarrollador tiene **7 objetivos** por área.
- Por objetivo: alcance, detalle, % de avance, fecha de seguimiento y observaciones.
- **RN-25.** Cada persona debe tener exactamente 7 objetivos (alertar si faltan o sobran).
- Vista de seguimiento por persona y vista consolidada del equipo.

### 5.11 Evolutivos (300 horas)

- Cada desarrollador debe cumplir un **mínimo de 300 horas de mantenimientos evolutivos por año**, que puede realizar en sus horas libres del día.
- Se registran de forma similar a un proyecto, pero de tipo **evolutivo**: persona, fecha, aplicación, actividad, horas.
- **RN-26.** Mostrar progreso hacia 300 horas por persona en el año calendario y ranking del equipo. Se puede **sobrepasar** el mínimo; el progreso puede mostrarse por encima del 100%.
- El contador se reinicia cada año.

### 5.12 Métricas del equipo

Qué se mide, con qué indicadores y con qué frecuencia:

| Dimensión | Indicadores | Frecuencia |
|---|---|---|
| Entrega (DORA) | Frecuencia de despliegue, tiempo de entrega de cambios, tasa de fallos de cambio, tiempo de restauración | Mensual |
| Velocidad | Velocidad por sprint (puntos o ítems completados) | Por sprint |
| Capacidad | Horas disponibles vs asignadas, % de ocupación por persona y proyecto | Semanal |
| Madurez | Cobertura de conocimientos, cumplimiento de objetivos, cumplimiento de evolutivos | Trimestral |

- Cada indicador tiene: objetivo, frecuencia, tendencia y una explicación breve de **por qué se mide**.
- Filtros por proyecto, equipo y período.

**Cómo se obtienen las métricas DORA (por fases):**

*Fase A: con los datos que ya se registran (RFC y trazabilidad), sin integración.*

| Métrica | Cálculo |
|---|---|
| Frecuencia de despliegue | Número de pases a PROD (aplicación + RFC de PROD) por período, por aplicación, proyecto o equipo |
| Tiempo de entrega de cambios | Días entre la fecha de inicio de trabajo en la aplicación y el pase a PROD (también inicio→TEST y TEST→PROD por separado) |
| Tasa de fallos de cambio | % de pases a PROD originales que tuvieron al menos un pase de corrección en posproducción (RN-28) |
| Tiempo de restauración | Tiempo entre el pase fallido y su pase de corrección en posproducción |

*Fase B: integración de solo lectura con la API de Azure DevOps.*
- Los repositorios y pull requests están en proyectos de DevOps distintos de los tableros. El **puente es el catálogo de aplicaciones** (cada aplicación guarda su proyecto y repositorio) y el **link al tablero de cada proyecto**. Así se consulta cada fuente por separado y se unen en TeamHub por aplicación y por proyecto.
- Se usa para afinar el tiempo de entrega (primer commit, merge del pull request) y para la velocidad por sprint desde el tablero.
- Requiere un token de acceso de solo lectura con permisos sobre los proyectos de repositorios y de tableros.

## 6. Alertas

| Alerta | Condición |
|---|---|
| Contrato por vencer | 30/15/7 días para el fin de contrato (externos), con su proveedor |
| Asignación por terminar | 30/15/7 días para el fin de la asignación |
| Proyecto por terminar o vencido sin cerrar | Fecha actual cerca o posterior a la fecha de fin sin estar finalizado |
| Sobreasignación | Dedicación mayor al 100% |
| Presupuesto | Costo total al 80% del ROM o por encima |
| Horas planificadas | Las horas mensuales de las asignaciones superan las horas presupuestadas (de la asignación o del proyecto) |
| Contrato de externo | Asignación que termina después del contrato vigente, o sin contrato |
| Horas extra | Registros pendientes de aprobar o de recuperar |
| Teletrabajo | Cupo de un piso superado; ampliación por vencer |
| Objetivos | Persona sin los 7 objetivos |
| Evolutivos | Persona con avance bajo respecto al mínimo anual |

## 7. Dashboards

1. **General:** KPIs (personas activas, internos vs externos y externos por proveedor, proyectos activos, disponibles pronto, horas extra pendientes, facturación mensual estimada a externos), asignaciones y contratos por terminar, línea de tiempo anual de proyectos, cumpleaños y vacaciones del mes, alertas.
2. **Proyectos:** línea de tiempo anual (inicio-fin), consumo de presupuesto, facturación mensual a externos por PO.
3. **Capacidad:** persona × tiempo con % de dedicación.
4. **Conocimientos:** matriz y riesgos.
5. **Métricas:** DORA, velocidad, capacidad, madurez.

## 8. Requisitos no funcionales

- **Seguridad:** autenticación con Microsoft Entra ID; control de acceso por rol (RN-01).
- **Datos personales:** cédula y celular son datos personales; cifrado en tránsito y en reposo, y registro de quién consulta o modifica (auditoría). Revisar la normativa de protección de datos personales aplicable.
- **Auditoría:** toda aprobación, cambio de estado y modificación de contrato queda registrada con usuario y fecha.
- **Evidencias:** almacenamiento de archivos adjuntos con límite de tamaño y tipos permitidos.
- **Idioma:** interfaz en español.
- **Dispositivos:** uso principal en escritorio, con diseño adaptable a móvil.
- **Exportación:** posibilidad de exportar listados y reportes a Excel o PDF.
- **Integración (fase posterior):** API de Azure DevOps en solo lectura, con credenciales guardadas de forma segura y nunca en el código.

## 9. Fases sugeridas de construcción

1. **Base:** modelo de datos, autenticación con Entra ID y roles, catálogos (cargos, áreas, aplicaciones, tecnologías).
2. **Equipo:** personas, contratos y renovaciones, conocimientos.
3. **Proyectos:** proyectos, PO, presupuesto, asignaciones con horas mensuales y reemplazos, facturación mensual a externos, aplicaciones, RFC y trazabilidad de pases.
4. **Día a día:** horas extra y recuperación, permisos, vacaciones, cumpleaños, teletrabajo.
5. **Desarrollo del equipo:** logros, objetivos, evolutivos.
6. **Dashboards, alertas y métricas (fase A).**
7. **Integración con Azure DevOps (métricas fase B).**

## 10. Preguntas abiertas

1. **Vacaciones:** ¿el año en que se pierden las semanas no tomadas es el año calendario (enero a diciembre)?
2. **Visibilidad del costo y la facturación:** ¿solo los ve el Jefe de Desarrollo, o también los otros jefes de sus colaboradores externos?
