Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Centros
Imports Services.Interfaces

<JwtAuthorize>
Public Class CentrosController
    Inherits ControladorApiBase

    Private ReadOnly servicio As ICentroService

    Public Sub New(servicio As ICentroService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado.
    <HttpGet>
    <Route("api/centros")>
    Public Function Listar() As IHttpActionResult
        Return Ok(servicio.Listar())
    End Function

    <HttpGet>
    <Route("api/centros/{codigo}")>
    Public Function ObtenerPorCodigo(codigo As String) As IHttpActionResult
        Dim resultado = servicio.ObtenerPorCodigo(codigo)

        If resultado.Estado = ResultadoOperacionCentro.NoEncontrado Then Return NotFound()
        Return Ok(resultado.Centro)
    End Function

    ' Escritura: catálogo compartido por todos los usuarios, solo Administrador.
    <HttpPost>
    <Route("api/centros")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Crear(<FromBody> input As CentroInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del centro.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionCentro.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionCentro.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionCentro.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Centro)
        End Select
    End Function

    <HttpPut>
    <Route("api/centros/{codigo}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(codigo As String, <FromBody> input As CentroInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del centro.")

        Dim resultado = servicio.Actualizar(codigo, input)

        Select Case resultado.Estado
            Case ResultadoOperacionCentro.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionCentro.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionCentro.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Centro)
        End Select
    End Function

    <HttpDelete>
    <Route("api/centros/{codigo}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Eliminar(codigo As String) As IHttpActionResult
        Dim resultado = servicio.Eliminar(codigo)

        Select Case resultado.Estado
            Case ResultadoOperacionCentro.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionCentro.EnUso
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionCentro.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

End Class
