# AGENTS.md — BASHoras

Guía para agentes de IA que trabajen en este repo. Proyecto ASP.NET para gestión de horas de trabajo (carga de registros, reportes, usuarios), con Membership de ASP.NET para autenticación/roles y Entity Framework 6 sobre SQL Server como ORM.

## 1. Tipo de proyecto — importante antes de tocar nada

Esto **no** es un proyecto moderno con `.csproj`/`.vbproj`. Es un **ASP.NET Website Project** clásico:

- No hay archivo de proyecto (`.vbproj`/`.csproj`). `BASHoras.sln` y `vwd.webinfo` son metadatos de Visual Studio, no un project file de MSBuild.
- `App_Code/` se compila automáticamente en runtime/build por el motor de ASP.NET (cualquier `.vb` ahí dentro entra en un único ensamblado dinámico, salvo que se declare `<codeSubDirectories>` en `Web.config` para mezclar lenguajes — no está configurado, así que **todo `App_Code` debe ser VB.NET**).
- Los paquetes NuGet se gestionan vía `packages.config` + copia de DLLs a `Bin/` (cada una con su `.dll.refresh`). **No hay forma de instalar paquetes por CLI** (`dotnet add package` / `nuget install` no aplican a este modelo) — siempre hay que pedirle al usuario que instale desde el NuGet Package Manager de Visual Studio y reportar qué se agregó.
- Target framework: **.NET Framework 4.7.2**. Cualquier paquete nuevo hay que verificar en nuget.org que tenga binarios compatibles con `net45`-`net472` antes de instalar como "latest" (ver gotchas más abajo).
- `app/` es la interfaz WebForms existente (ASPX + code-behind). **Nunca modificarla.** Es la UI en producción; el objetivo de este trabajo es una API nueva que corre en paralelo, sin tocar esa parte.

## 2. Dónde trabajar

- **Todo el código nuevo de la API va en `App_Code/API/`, `App_Code/Repositories/`, `App_Code/Services/`, `App_Code/DTOs/`.**
- `App_Code/Aplicacion.vb` (clases `Logica`, `Pagina*`, `Utilidades`) y `App_Code/Usuarios.vb` (clase `Aplicacion.Usuarios`) son la capa de negocio **preexistente, usada únicamente por `app/`**. La API **no depende de nada de `Aplicacion.vb`/`Usuarios.vb`** (ni `Logica`, ni `Usuarios`, ni `Utilidades`) — cada dominio de la API reimplementa lo que necesita en su propio Repository/Service/Utils. Esto mantiene `app/` y la API totalmente desacopladas: un cambio en una no puede romper la otra. (Hubo una excepción puntual reusando `Aplicacion.Utilidades.GenerarPassword` para generar passwords temporales, pero se reemplazó por `Utils.GeneradorPassword` — ver más abajo — así que la regla ya no tiene excepciones.)
- `App_Code/Model.*` (`Model.edmx`, `Model.vb`, `Model.Context.vb`, `Model.Designer.vb`) es el modelo EF6 generado, compartido por `app/` y la API. Solo tocar `Model.edmx` para corregir mapeos EF genuinamente rotos (como se hizo una vez, ver gotcha #5) — nunca para lógica nueva específica de la API.
- Base de datos: SQL Server en `LUCI\SQLEXPRESS`, base `BASHoras`, connection strings en `Web.config` (`LocalSqlServer` para Membership, `BASHorasEntities` para EF). Usuario `sa` / contraseña `sa`. Hay `sqlcmd` disponible en la máquina de desarrollo (`C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE` o `180`), útil para inspeccionar SPs reales cuando EF tira errores de mapeo:
  ```
  sqlcmd -S "LUCI\SQLEXPRESS" -U sa -P sa -d BASHoras -Q "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.<NombreSP>'))"
  ```
- `Web.config` → `appSettings` → `EnviarMailsHabilitado` (default `true`): en `false`, `NotificadorUsuarios.EnviarNuevaPassword` no manda el mail — en cambio, deja constancia (fecha, email, password en texto plano) en `<temp del sistema>\BASHoras\recuperacion-password-dev.log` (`Path.GetTempPath()`, fuera de la carpeta del sitio a propósito, para que un deploy/backup de la carpeta no se lleve passwords de paso). El valor por defecto commiteado es `true` (seguro para producción) — cada quien lo pone en `false` en su `Web.config` local si lo necesita. No hay ningún framework de logging centralizado en este proyecto (ni log4net/NLog/Event Log) — esto es solo una conveniencia puntual de desarrollo, no un patrón a replicar para logging general sin discutirlo antes.

## 3. Arquitectura de la API

**Capas: Controller → Service → Repository → EF6.** Elegido así (en vez de Controller → Repository directo) pensando en una futura migración a ASP.NET Core + C#: los Services/Repositories se portan casi 1:1 (mismos nombres y firmas, solo traducción VB→C#, y se cambia la implementación EF6→EF Core), mientras los Controllers quedan tan delgados que reescribirlos en Core es trivial.

- **Repositories** devuelven `IQueryable(Of T)` — nunca el `DbQuery(Of T)` específico de EF6 — porque `IQueryable` existe igual en EF Core (portabilidad futura).
- **DI**: Autofac + Autofac.WebApi2, registrado en `App_Code/API/DependencyConfig.vb` (`InstancePerRequest` para `BASHorasEntities`, repos y services), invocado desde `WebApiConfig.Register`. `RegisterApiControllers` descubre los controllers por reflexión sobre el ensamblado de `App_Code` en runtime.
- **CORS**: habilitado globalmente en `WebApiConfig.vb` vía `Microsoft.AspNet.WebApi.Cors` (paquete `Microsoft.AspNet.WebApi.Cors` + `Microsoft.AspNet.Cors`, ambos 5.3.0, mismo target net45 que el resto del stack WebApi). El origen permitido hoy es `http://localhost:3000` (frontend en desarrollo) — si se agrega un dominio de stage/prod, sumarlo a la lista de orígenes del `EnableCorsAttribute` (acepta varios separados por coma). `config.EnableCors(...)` se llama antes de `MapHttpAttributeRoutes` a propósito (orden requerido por el paquete). El preflight `OPTIONS` lo intercepta el message handler del paquete de CORS antes de llegar a los filtros de autorización, así que `<JwtAuthorize>` no bloquea el preflight.
- **Convención de carpeta = namespace:**

  | Carpeta | Namespace |
  |---|---|
  | `App_Code/DTOs/<Dominio>/` | `DTOs.<Dominio>` |
  | `App_Code/Repositories/Interfaces/` | `Repositories.Interfaces` |
  | `App_Code/Repositories/EF/` | `Repositories.EF` |
  | `App_Code/Repositories/Membership/` | `Repositories.Membership` (repos que envuelven `System.Web.Security.Membership`/`Roles`/`Profile` en vez de EF — ej. Usuarios) |
  | `App_Code/Repositories/Email/` | `Repositories.Email` (envuelve el envío de mails sobre la clase `Correo` ya existente — ej. `NotificadorUsuarios`) |
  | `App_Code/Services/Interfaces/` | `Services.Interfaces` |
  | `App_Code/Services/` | `Services` |
  | `App_Code/API/` | (sin namespace, global) |
  | `App_Code/API/auth/` | `Auth` |
  | `App_Code/Utils/` | `Utils` — helpers puros reusables entre capas (ej. `GeneradorPassword`), sin acceso a datos ni dependencias de `Aplicacion.vb` |

- **Patrón por dominio** (replicar para dominios nuevos, ej. Usuarios): `XxxDTO` (lectura), `XxxInputDTO` (alta/edición, sin campos controlados por el servidor como IDs o el usuario dueño), `ResultadoXxx` + enum `ResultadoOperacionXxx` (`Ok`/`NoEncontrado`/`Prohibido`/`Duplicado`/`Fallo` según aplique — ojo, **`Error` es palabra reservada en VB**, no usarla como nombre de miembro de enum), `IXxxRepository`/`XxxRepository`, `IXxxService`/`XxxService`, `XxxController` heredando de `ControladorApiBase`. Registrar las nuevas piezas en `DependencyConfig.vb`.
- **`ControladorApiBase`** (`App_Code/API/ControladorApiBase.vb`, `MustInherit`, hereda `ApiController`) centraliza `ObtenerUsuarioActual()` — lee el username del `ClaimsPrincipal` del JWT. Todo controller que necesite saber "quién está logueado" hereda de acá en vez de `ApiController` directo.

### Seguridad / ownership

- Cualquier campo "dueño" de un registro (ej. `Usuario`) **siempre sale del claim del JWT** vía `ObtenerUsuarioActual()`, nunca del body que manda el cliente — evita que alguien cargue datos a nombre de otro usuario.
- Mutaciones (`PUT`/`DELETE`) y lecturas por ID (`GET /{id}`) verifican ownership por igual y devuelven `403 Forbidden` si el que llama no es el dueño (antes las lecturas por ID no lo chequeaban — era un IDOR real, cualquier usuario autenticado podía leer registros/reportes de otro cambiando el id; corregido en `RegistroService.ObtenerPorId` y `ReporteGuardadoService.ObtenerPorId`). Si se agrega un nuevo endpoint `GET /{id}` sobre datos de un usuario, aplicar el mismo chequeo por default, no asumir que un simple "no encontrado" alcanza.
- `<JwtAuthorize>` (atributo propio en `App_Code/API/auth/JwtAuthorizeAttribute.vb`, namespace `Auth`) se aplica a nivel de clase en los controllers con datos sensibles (Registros, Reportes, Usuarios). Los catálogos (Países, Productos, Actividades, Centros) también exigen JWT: antes vivían en un `ConsultasController` público, que se eliminó al migrar cada catálogo a su propio controller con CRUD de Administrador. Si se agrega un endpoint con datos sensibles, proteger por default y preguntar si hay dudas.
- **Restricción por rol**: `JwtAuthorizeAttribute` hereda de `AuthorizeAttribute` y llama a `MyBase.OnAuthorization` después de fijar el `ClaimsPrincipal`, así que su propiedad `Roles` (heredada) funciona tal cual — alcanza con `<JwtAuthorize(Roles:="Administrador")>` a nivel de acción para restringir un endpoint puntual a un rol (ver `UsuariosController.Listar`/`ObtenerPorUserName`/`ActualizarUsuario`). No hace falta chequeo manual de `User.IsInRole(...)` en el controller. Los nombres de rol ("Administrador", "Avanzado", "Basico", extensible a futuro) están centralizados en `Aplicacion.Grupos` (`Aplicacion.vb`) — son solo constantes de string sin lógica ni acceso a datos, así que referenciarlas desde la API no rompe el desacople con `app/`; reusarlas en vez de hardcodear los strings de nuevo.
- Los matices más finos de permisos por rol que tiene `app/` (ej. qué campos puede setear cada rol al cargar un reporte) no se replicaron — si se pide algo así, es una decisión de diseño a confirmar con el usuario, no asumir.

### Dominio Reportes — decisiones de diseño

- **Dos servicios**: `ReporteGuardadoService` es CRUD puro sobre `REPORTES` (con ownership). `ReporteRegistrosService` ejecuta un reporte guardado (`spReporteGeneral` vía `IReporteGeneralRepository`) y devuelve/exporta sus registros. No hay endpoint de filtros libres: el viejo `POST /api/reportes/general` se eliminó porque dejaba ver registros de cualquier usuario sin control de rol.
- **Qué registros ve cada uno** (`ReporteRegistrosService.Preparar`, siempre sobre un reporte propio, 403 si no):
  - `Basico` (o sin rol): solo los propios; `REPORTES.DeUsuario` se ignora.
  - `Avanzado`: los propios y los de todos los usuarios `Basico` (sin restricción por Centro). Si `DeUsuario` apunta a otro usuario → 403 `{mensaje}`.
  - `Administrador`: todos, o solo `DeUsuario` si el reporte lo define.
  - `DeCentro` del reporte filtra además por el Centro del Profile. Producto/actividad nulos se mandan como `0` (el SP lo interpreta como "todos").
- **Qué campos ve cada uno** (`NivelDetalleReporte`, DTOs en `DTOs/Reportes/RegistroReporteDTO.vb`, cada nivel hereda del anterior):
  - `RegistroReporteBasicoDTO`: IDReg (para editar/borrar vía `/api/registros/{id}`), Fecha, Centro, Cod/Actividad, Pais, IdProducto/Producto, Horas, Minutos, Observaciones.
  - `RegistroReporteAvanzadoDTO` (Avanzado y Administrador): + Usuario, Legajo, CUIL, Jornada (= `Profile.Modalidad`).
  - `RegistroReporteFullDTO` (solo usuarios cuyo nombre empieza con un prefijo de `UsuariosNivelFull`: ROCCHI, CARRACEDO, mismo criterio que `habilitarRemuneraciones` de `app/`; independiente del rol): + Dias.habil (`DIAS_HABILES`), Durac.tarea, Remuneracion (`REMUNERACIONES` por usuario/mes/año de la tarea), Costo Tarea (= Remuneracion / DiasHabiles / Jornada × duración; vacío si falta algún dato). Un Administrador que no está en esa lista **no** ve costos.
  - `Items` es una `List(Of RegistroReporteBasicoDTO)` con instancias del nivel que corresponda; Json.NET serializa el tipo real de cada elemento, así que el JSON trae los campos del nivel (Swagger solo documenta los del Basico).
- **Profile**: Legajo/Centro/CUIL/Jornada salen del Profile vía `IUsuarioRepository.ObtenerPerfil`, una vez por usuario único (no por fila). La SP los deja en NULL.
- **Excel**: `Utils.ExportadorExcel.Generar(items, tipo, hoja)` arma el `.xlsx` (ClosedXML) por reflexión sobre el DTO: una columna por propiedad, las de la clase base primero, encabezado = `<DisplayName>`; las propiedades con `<Browsable(False)>` (ej. `IDReg`) van en el JSON pero no en el Excel. El orden de columnas sigue la jerarquía del DTO, salvo las que lista `ReporteRegistrosService.ColumnasExcelPrimero` (Fecha, Usuario, Legajo, CUIL, Jornada), que van primero; cambiar ahí el orden del Excel sin tocar el JSON. Para sumar una columna, agregar la propiedad al DTO del nivel correspondiente. `/exportar` devuelve exactamente los mismos registros y campos que `/registros`.
- **Paginado** (`ReporteRegistrosService.ObtenerRegistros`): el SP no pagina, así que se ejecuta completo, se filtra por rol y centro, se ordena (FechaActividad desc, IDReg desc — orden determinista, el del dashboard) y recién ahí `Skip/Take`. `TotalRegistros` es el conteo antes de paginar. Perfiles, remuneraciones/días hábiles y mapeo a DTO se hacen solo para la página, salvo que el reporte tenga `DeCentro` (ese filtro necesita el Profile de todos los usuarios). `/exportar` **no** pagina: todas las filas, en el orden del SP. La validación/visibilidad común a ambos está en `Preparar`.
- **CORS y descargas**: `WebApiConfig` expone `Content-Disposition` (`corsPolicy.ExposedHeaders.Add(...)`; la propiedad es de solo lectura, no se puede asignar) para que el frontend lea el nombre del `.xlsx`.
- `DTOs.Reportes.FiltroReporte` ya no lo usa la API, pero **no borrarlo**: lo usan `Aplicacion.vb` y `app/reporte.aspx.vb`.

## 4. Superficie actual de la API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | — | Login contra Membership, devuelve JWT |
| POST | `/api/auth/refresh` | JWT | Renueva el token |
| GET | `/api/test` | — | Endpoint de ejemplo/smoke test |
| GET | `/api/centros` | JWT | Lista centros por nombre. Reemplaza al viejo `/api/consultas/centros`, que era público y se eliminó |
| GET | `/api/centros/{codigo}` | JWT | Detalle de un centro (404 si no existe) |
| POST | `/api/centros` | JWT + rol `Administrador` | Alta (`CodCen` ≤ 50 chars, `Nombre` ≤ 50; 400 si inválido, 409 si el código ya existe) |
| PUT | `/api/centros/{codigo}` | JWT + rol `Administrador` | Edita `Nombre` (el código es la clave y no se modifica) |
| DELETE | `/api/centros/{codigo}` | JWT + rol `Administrador` | Baja (409 si algún usuario tiene ese centro en su Profile) |
| GET | `/api/configuraciones` | JWT | Lista las filas de `CONFIGURACIONES` (`CodConfig`, `Valor`, `Descripcion`); cualquier usuario autenticado, el frontend lee los valores (ej. `PERMITIR_CARGAS`) |
| GET | `/api/configuraciones/{codigo}` | JWT | Detalle de una configuración (404 si no existe) |
| PUT | `/api/configuraciones/{codigo}` | JWT + rol `Administrador` | Edita **solo** `Valor` (≤ 50 chars, no nulo; 400 si inválido). `CodConfig` y `Descripcion` no se modifican. No hay alta ni baja |
| GET | `/api/feriados?anio=` | JWT | Lista feriados por fecha; `anio` opcional filtra por año |
| GET | `/api/feriados/{fecha}` | JWT | Detalle de un feriado; la fecha va como `yyyy-MM-dd` (400 si el formato es inválido, 404 si no existe) |
| POST | `/api/feriados` | JWT + rol `Administrador` | Alta (`Feriado` fecha obligatoria, `Motivo` opcional ≤ 50; 400 si inválido, 409 si ya hay un feriado ese día) |
| PUT | `/api/feriados/{fecha}` | JWT + rol `Administrador` | Edita `Motivo` (la fecha es la clave y no se modifica) |
| DELETE | `/api/feriados/{fecha}` | JWT + rol `Administrador` | Baja (sin dependencias: la tabla no tiene FKs) |
| GET | `/api/dias-habiles?anio=` | JWT | Lista cantidad de días hábiles por año/mes (`DIAS_HABILES`); `anio` opcional filtra |
| GET | `/api/dias-habiles/{id:int}` | JWT | Detalle (404 si no existe) |
| POST | `/api/dias-habiles` | JWT + rol `Administrador` | Alta (`Anio` 2000-2100, `Mes` 1-12, `CantDias` entre 0 y los días del mes; 400 si inválido, 409 si ya existe ese año/mes — hay UNIQUE en la base) |
| PUT | `/api/dias-habiles/{id:int}` | JWT + rol `Administrador` | Edición completa (409 si año/mes choca con otro registro) |
| DELETE | `/api/dias-habiles/{id:int}` | JWT + rol `Administrador` | Baja (sin FKs). Ojo: el export de reportes usa esta tabla para Dias.habil/Costo |
| GET | `/api/paises` | JWT | Lista países (preferidos primero, luego por nombre). Reemplaza al viejo `/api/consultas/paises`, que era público y se eliminó |
| GET | `/api/paises/{codigo}` | JWT | Detalle de un país (404 si no existe) |
| POST | `/api/paises` | JWT + rol `Administrador` | Alta (`Codigo` ≤ 4 chars, `Pais` ≤ 100; 400 si inválido, 409 si el código ya existe) |
| PUT | `/api/paises/{codigo}` | JWT + rol `Administrador` | Edita `Pais`/`Preferido` (el código es la clave y no se modifica). Hay un único país preferido: marcar uno (en alta o edición) desmarca el anterior en la misma transacción |
| DELETE | `/api/paises/{codigo}` | JWT + rol `Administrador` | Baja (409 si es el país preferido o si hay registros de horas con ese país) |
| GET | `/api/productos?soloActivos=` | JWT | Lista productos (default `soloActivos=true`). Reemplaza al viejo `/api/consultas/productos`, que era público y se eliminó |
| GET | `/api/productos/{id:int}` | JWT | Detalle de un producto (404 si no existe) |
| POST | `/api/productos` | JWT + rol `Administrador` | Alta (`Codigo` ≤ 10 chars, `Producto` ≤ 400, `Activo` default true; 400 si inválido o `FProyIni` > `FProyFin`). No hay 409 por código duplicado: la base ya tiene códigos repetidos |
| PUT | `/api/productos/{id:int}` | JWT + rol `Administrador` | Edición completa (Codigo, Producto, Activo, FProyIni, FProyFin) |
| DELETE | `/api/productos/{id:int}` | JWT + rol `Administrador` | Baja (409 si hay registros de horas o filas de `PROYECTOS` que lo referencian; en ese caso usar `Activo=false`) |
| GET | `/api/actividades?soloHabilitadas=` | JWT | Lista actividades (default `soloHabilitadas=true`). Reemplaza al viejo `/api/consultas/actividades`, que era público y se eliminó |
| GET | `/api/actividades/{id:int}` | JWT | Detalle de una actividad (404 si no existe) |
| POST | `/api/actividades` | JWT + rol `Administrador` | Alta (`Codigo` ≤ 10 chars, `Actividad` ≤ 400, `Habilitada` default true; 400 si inválido, 409 si el código ya existe) |
| PUT | `/api/actividades/{id:int}` | JWT + rol `Administrador` | Edición completa (409 si el código choca con otra actividad) |
| DELETE | `/api/actividades/{id:int}` | JWT + rol `Administrador` | Baja (409 si hay registros de horas o filas de `TAREASDEV` que la referencian; en ese caso usar `Habilitada=false`) |
| GET | `/api/registros?desde=&hasta=` | JWT | Lista registros de horas del usuario logueado en el rango (default: hoy-hoy si no se manda ninguno; 400 si desde > hasta) |
| GET | `/api/registros/{id}` | JWT | Detalle de un registro (403 si no es el dueño) |
| POST | `/api/registros` | JWT | Alta (Usuario = token). Valida en `RegistroService.Validar` (orden del legacy): carga habilitada (`PERMITIR_CARGAS`), duración/producto/actividad, duplicado (409), rango de fechas y día laborable (estos dos no aplican a `Administrador`), proyecto en curso/cerrado (`FProyIni`/`FProyFin`), observaciones obligatorias (`ACTIV_CON_OBSERV`). Errores de regla: 400 con `{ "mensaje": "..." }` (también 409/500 devuelven `{mensaje}`) |
| PUT | `/api/registros/{id}` | JWT | Edición (403 si no es el dueño); aplica las mismas reglas que el alta sobre la fecha nueva. Además, salvo `Administrador`, la fecha **original** del registro tiene que estar dentro del período abierto del usuario (`CalcularFechaDesde`..hoy); si no, 400 `{mensaje}` |
| DELETE | `/api/registros/{id}` | JWT | Baja (403 si no es el dueño). 400 `{mensaje}` si `PERMITIR_CARGAS` = False o, salvo `Administrador`, si el registro está fuera del período abierto del usuario |
| GET | `/api/reportes` | JWT | Lista reportes guardados del usuario |
| GET | `/api/reportes/{id:guid}` | JWT | Detalle de un reporte guardado (403 si no es el dueño) |
| GET | `/api/reportes/{id:guid}/registros?pagina=&tamanioPagina=` | JWT | Ejecuta el reporte guardado y devuelve una página de sus registros en `ResultadoPaginado` (`Items`, `TotalRegistros`, `Pagina`, `TamanioPagina`, `TotalPaginas`). `pagina` default 1, `tamanioPagina` default 50, máx 1000 (fuera de rango se ajusta, como `/api/registros`). Qué registros (por rol) y qué campos (Basico/Avanzado/Full) dependen de quién llama — ver "Dominio Reportes". 403 si no es el dueño o si un Avanzado pide un `DeUsuario` no permitido |
| GET | `/api/reportes/{id:guid}/exportar` | JWT | Los mismos registros y columnas que `/registros`, como `.xlsx` (nombre = título del reporte) |
| POST | `/api/reportes` | JWT | Crea un reporte guardado (IDRep = Guid nuevo) |
| PUT | `/api/reportes/{id:guid}` | JWT | Edita (403 si no es el dueño) |
| DELETE | `/api/reportes/{id:guid}` | JWT | Borra (403 si no es el dueño) |
| GET | `/api/usuarios/me` | JWT | Datos propios (Membership + Profile + roles) |
| PUT | `/api/usuarios/me` | JWT | Edita datos propios — **solo** Email y Comentario |
| POST | `/api/usuarios/me/password` | JWT | Cambia la propia contraseña (requiere la actual) |
| GET | `/api/usuarios` | JWT + rol `Administrador` | Lista todos los usuarios |
| GET | `/api/usuarios/{userName}` | JWT + rol `Administrador` | Datos de cualquier usuario |
| POST | `/api/usuarios` | JWT + rol `Administrador` | Alta de usuario. Body `CrearUsuarioInputDTO` (hereda los campos del PUT) + `UserName` (obligatorio, sin espacios al inicio/final) y `Password` (obligatoria). `Rol` y `Habilitado` obligatorios, mismas validaciones que el PUT. 201 con el `UsuarioDTO`; 400 si hay datos mal formados o `MembershipCreateStatus` = `InvalidPassword`/`InvalidUserName`/`InvalidEmail`; 409 si `DuplicateUserName`/`DuplicateEmail` (o si el usuario ya existe); 500 con mensaje si falla un paso. Orden: validar sin escribir → `Membership.CreateUser` → `UpdateUser` (comentario + `IsApproved`) → Profile → `AsignarRolUnico`. Si falla Update/Profile/rol se intenta borrar el usuario recién creado (`DeleteUser` con datos relacionados); si el borrado también falla, 500 avisando que quedó creado a medias |
| PUT | `/api/usuarios/{userName}` | JWT + rol `Administrador` | Edita cualquier usuario, incluye campos administrativos (Legajo/Centro/CUIL/Dependencia/Modalidad/PeriodoAbierto) y además `Password` (opcional: vacía/`null` no la toca; si viene se fija con `ForzarNuevaPassword`, tras `UpdateUser` y antes de Profile/rol; si la cuenta está bloqueada se la **desbloquea** primero porque `ResetPassword` falla en cuentas bloqueadas; la política de contraseña se valida antes de escribir nada y devuelve 400), `Rol` (obligatorio: `Basico`/`Avanzado`/`Administrador`, el usuario queda **solo** en ese rol) y `Habilitado` (obligatorio, se guarda en `IsApproved`). 400 si el rol es inválido o falta `Habilitado`; 409 si un administrador intenta quitarse su propio rol/deshabilitarse, o si el cambio dejaría al sistema sin ningún administrador habilitado (aprobado y no bloqueado); 500 con mensaje si falla un paso intermedio. Orden de escrituras (Membership/Profile/Roles no comparten transacción): `UpdateUser` (puede fallar por email duplicado) → Profile → rol (se agrega el nuevo antes de sacar los viejos) |
| POST | `/api/auth/recuperar-password` | — | Genera una password nueva (`Utils.GeneradorPassword`) y la manda por mail al usuario encontrado por email |

**Pendiente / fuera de alcance por ahora (no asumir que existe):** baja (borrado) de usuarios, y un endpoint dedicado de reseteo de contraseña ajena (hoy un Admin puede fijarla con el campo `Password` de `PUT /api/usuarios/{userName}`, además del self-service "olvidé mi contraseña" de `/api/auth/recuperar-password`). El alta de usuarios (`POST /api/usuarios`) y el cambio de rol (campo `Rol` del PUT) ya existen.

## 5. Documentación / Swagger

Swashbuckle clásico (paquetes `Swashbuckle` + `Swashbuckle.Core`, **no** `Swashbuckle.AspNetCore`) — es el correcto para este stack (`System.Web.Http` sobre .NET Framework). La deprecación de Swagger en .NET Core (reemplazado por `Microsoft.AspNetCore.OpenApi` + Scalar) es una historia aparte que no aplica acá.

- UI en `/swagger`, doc JSON en `/swagger/docs/v1`.
- Configurado en `App_Code/API/SwaggerConfig.vb`, invocado explícitamente desde `WebApiConfig.Register` (no depende de `WebActivatorEx`/atributos de ensamblado).
- Es la UI vieja de Swagger (2.x). El campo de autenticación arriba a la derecha tiene el placeholder fijo `api_key` (cosmético, no configurable en esa versión) — ahí se pega `Bearer <token>` obtenido de `POST /api/auth/login`.

## 6. Gotchas ya pisados (para no repetirlos)

1. **Paquetes netstandard-only sobre net472**: `Autofac.WebApi2` en su versión "latest" (7.0.0) requiere `net481` y no instala contra este proyecto (net472) — hay que fijar una versión compatible explícita (`Install-Package Autofac.WebApi2 -Version 6.2.0`). Verificar siempre el target framework real en nuget.org antes de instalar sin versión.
2. **Falta el facade `netstandard.dll`**: paquetes que solo traen binarios `netstandard2.0` (como `Autofac` 9.x) tiran `BC30007: Es necesaria una referencia al ensamblado 'netstandard...'` porque este Website Project no tiene MSBuild para auto-agregar el facade. Solución aplicada: copiar manualmente `netstandard.dll` desde `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Facades\netstandard.dll` a `Bin/`. **Este archivo no lo gestiona NuGet** — si se limpia `Bin/` hay que volver a copiarlo.
3. **LINQ to Entities no traduce funciones locales**: no se puede hacer `Select MapearDTO(r)` sobre un `IQueryable` de EF6 sin materializar antes — siempre `.ToList()` primero y recién ahí mapear en memoria con una función VB. (Bug real, pisado en `RegistroService.Listar`.)
4. **Mapeo EDMX vs. alias de `spReporteGeneral`**: el `ColumnName` del `<ScalarProperty>` de `Model.edmx` debe coincidir exactamente con el alias de la columna que devuelve la SP (`[FECHA_ACTIVIDAD]`, con guion bajo; es lo que tiene producción). La base local tenía una versión de la SP desviada, con `[FECHA ACTIVIDAD]` (espacio), y daba "The data reader is incompatible with the specified 'BASHorasModel.spReporteGeneral_Result'... 'FECHA_ACTIVIDAD' does not have a corresponding column". Se resolvió corrigiendo la SP **local** para alinearla con producción y dejando el EDMX en `ColumnName="FECHA_ACTIVIDAD"`. Si reaparece el error, comparar la SP de la base contra producción antes de tocar el EDMX; y tras un "Update Model from Database" revisar que el mapeo siga igual. Nota aparte: la SP deja `LEGAJO`, `JORNADA`, `CENTRO`, `REMUNERACION`, `CUIL` siempre en `NULL` (comentario en la SP: "valores a averiguar por Membership") — está incompleta, no es un bug de la API.
5. **`Error` es palabra reservada en VB.NET** — no usarla como nombre de miembro de enum (se detectó al nombrar un estado de resultado; se renombró a `Fallo`).
6. **El instalador de Swashbuckle asume Web Application Project**: deja un `SwaggerConfig.cs` bajo `App_Start/`, carpeta que este Website Project no compila automáticamente. Se reescribió a mano en VB.NET en `App_Code/API/SwaggerConfig.vb`.
7. **Membership no tiene un "set password" directo** sin conocer la actual (`ChangePassword` la pide, `ResetPassword()` genera una propia que no podés elegir). Para forzar una password nuestra (ej. recuperación por mail) el truco es encadenar `Dim temporal = usuario.ResetPassword() : usuario.ChangePassword(temporal, nuevaPassword)` — ver `UsuarioRepository.ForzarNuevaPassword`.
8. **`Aplicacion.Utilidades.GenerarPassword` usa `System.Random`**, no es criptográficamente seguro (y `New Random()` creado en threads distintos casi al mismo tiempo puede repetir semilla). Para generar passwords desde la API se usa `Utils.GeneradorPassword.Generar(longitud)` (`App_Code/Utils/GeneradorPassword.vb`), que usa `RandomNumberGenerator` y garantiza al menos una minúscula/mayúscula/dígito.
9. **IDOR en `GET /{id}`**: al construir Registros y Reportes, las lecturas por ID no chequeaban ownership (solo las mutaciones) — cualquier usuario autenticado podía leer el registro/reporte de otro con solo cambiar el id. Corregido en `RegistroService.ObtenerPorId` / `ReporteGuardadoService.ObtenerPorId` (ahora devuelven `ResultadoRegistro`/`ResultadoReporte` con `Prohibido` igual que las mutaciones). Al agregar un `GET /{id}` nuevo sobre datos de un usuario, no repetir el error: aplicar el chequeo de ownership desde el principio.
10. **Encoding de los `.vb`**: guardar los archivos nuevos o editados por agentes en **UTF-8 sin BOM** (convención del proyecto, para que los mensajes con acentos/ñ que devuelve la API salgan bien). Muchos archivos existentes generados por Visual Studio tienen BOM; no convertirlos en masa sin pedirlo.

## 7. Qué información darme si algo de esto cambió

Si el usuario dice que ya no es cierto algo de acá (por ejemplo, si el proyecto migra a Web Application Project con `.vbproj`, si cambia el motor de DI, o si se migran paquetes), verificar contra el estado real del repo antes de asumir que esta guía sigue vigente — está escrita a partir del estado al 2026-10-01.
