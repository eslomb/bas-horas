Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Paises
Imports Services.Interfaces

<JwtAuthorize>
Public Class PaisesController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IPaisService

    Public Sub New(servicio As IPaisService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado (lo necesita el formulario de carga de horas).
    <HttpGet>
    <Route("api/paises")>
    Public Function Listar() As IHttpActionResult
        Return Ok(servicio.Listar())
    End Function

    <HttpGet>
    <Route("api/paises/{codigo}")>
    Public Function ObtenerPorCodigo(codigo As String) As IHttpActionResult
        Dim resultado = servicio.ObtenerPorCodigo(codigo)

        If resultado.Estado = ResultadoOperacionPais.NoEncontrado Then Return NotFound()
        Return Ok(resultado.Pais)
    End Function

    ' Escritura: catálogo compartido por todos los usuarios, solo Administrador.
    <HttpPost>
    <Route("api/paises")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Crear(<FromBody> input As PaisInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del país.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionPais.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionPais.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionPais.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Pais)
        End Select
    End Function

    <HttpPut>
    <Route("api/paises/{codigo}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(codigo As String, <FromBody> input As PaisInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del país.")

        Dim resultado = servicio.Actualizar(codigo, input)

        Select Case resultado.Estado
            Case ResultadoOperacionPais.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionPais.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionPais.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Pais)
        End Select
    End Function

    <HttpDelete>
    <Route("api/paises/{codigo}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Eliminar(codigo As String) As IHttpActionResult
        Dim resultado = servicio.Eliminar(codigo)

        Select Case resultado.Estado
            Case ResultadoOperacionPais.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionPais.EnUso, ResultadoOperacionPais.EsPreferido
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionPais.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

End Class
