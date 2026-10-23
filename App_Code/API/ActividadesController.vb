Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Actividades
Imports Services.Interfaces

<JwtAuthorize>
Public Class ActividadesController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IActividadService

    Public Sub New(servicio As IActividadService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado (lo necesita el formulario de carga de horas).
    <HttpGet>
    <Route("api/actividades")>
    Public Function Listar(Optional soloHabilitadas As Boolean = True) As IHttpActionResult
        Return Ok(servicio.Listar(soloHabilitadas))
    End Function

    <HttpGet>
    <Route("api/actividades/{id:int}")>
    Public Function ObtenerPorId(id As Integer) As IHttpActionResult
        Dim resultado = servicio.ObtenerPorId(id)

        If resultado.Estado = ResultadoOperacionActividad.NoEncontrado Then Return NotFound()
        Return Ok(resultado.Actividad)
    End Function

    ' Escritura: catálogo compartido por todos los usuarios, solo Administrador.
    <HttpPost>
    <Route("api/actividades")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Crear(<FromBody> input As ActividadInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos de la actividad.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionActividad.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionActividad.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionActividad.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Actividad)
        End Select
    End Function

    <HttpPut>
    <Route("api/actividades/{id:int}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(id As Integer, <FromBody> input As ActividadInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos de la actividad.")

        Dim resultado = servicio.Actualizar(id, input)

        Select Case resultado.Estado
            Case ResultadoOperacionActividad.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionActividad.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionActividad.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionActividad.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Actividad)
        End Select
    End Function

    <HttpDelete>
    <Route("api/actividades/{id:int}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Eliminar(id As Integer) As IHttpActionResult
        Dim resultado = servicio.Eliminar(id)

        Select Case resultado.Estado
            Case ResultadoOperacionActividad.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionActividad.EnUso
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionActividad.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

End Class
