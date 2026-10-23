Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.DiasHabiles
Imports Services.Interfaces

<JwtAuthorize>
Public Class DiasHabilesController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IDiaHabilService

    Public Sub New(servicio As IDiaHabilService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado. Filtro opcional por año.
    <HttpGet>
    <Route("api/dias-habiles")>
    Public Function Listar(Optional anio As Integer? = Nothing) As IHttpActionResult
        Return Ok(servicio.Listar(anio))
    End Function

    <HttpGet>
    <Route("api/dias-habiles/{id:int}")>
    Public Function ObtenerPorId(id As Integer) As IHttpActionResult
        Dim resultado = servicio.ObtenerPorId(id)

        If resultado.Estado = ResultadoOperacionDiaHabil.NoEncontrado Then Return NotFound()
        Return Ok(resultado.DiaHabil)
    End Function

    ' Escritura: dato compartido por todos los usuarios, solo Administrador.
    <HttpPost>
    <Route("api/dias-habiles")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Crear(<FromBody> input As DiaHabilInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos de los días hábiles.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionDiaHabil.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionDiaHabil.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionDiaHabil.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.DiaHabil)
        End Select
    End Function

    <HttpPut>
    <Route("api/dias-habiles/{id:int}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(id As Integer, <FromBody> input As DiaHabilInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos de los días hábiles.")

        Dim resultado = servicio.Actualizar(id, input)

        Select Case resultado.Estado
            Case ResultadoOperacionDiaHabil.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionDiaHabil.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionDiaHabil.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionDiaHabil.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.DiaHabil)
        End Select
    End Function

    <HttpDelete>
    <Route("api/dias-habiles/{id:int}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Eliminar(id As Integer) As IHttpActionResult
        Dim resultado = servicio.Eliminar(id)

        Select Case resultado.Estado
            Case ResultadoOperacionDiaHabil.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionDiaHabil.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

End Class
