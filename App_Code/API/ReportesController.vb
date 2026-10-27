Imports System.Web.Http
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports Auth
Imports DTOs.Reportes
Imports Services.Interfaces

<JwtAuthorize>
Public Class ReportesController
    Inherits ControladorApiBase

    Private ReadOnly servicioGuardados As IReporteGuardadoService
    Private ReadOnly servicioRegistros As IReporteRegistrosService

    Public Sub New(servicioGuardados As IReporteGuardadoService, servicioRegistros As IReporteRegistrosService)
        Me.servicioGuardados = servicioGuardados
        Me.servicioRegistros = servicioRegistros
    End Sub

    <HttpGet>
    <Route("api/reportes")>
    Public Function Listar() As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Return Ok(servicioGuardados.Listar(usuario))
    End Function

    <HttpGet>
    <Route("api/reportes/{id:guid}")>
    Public Function ObtenerPorId(id As Guid) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicioGuardados.ObtenerPorId(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case Else
                Return Ok(resultado.Reporte)
        End Select
    End Function

    ' Registros del reporte. Qué registros y qué campos se devuelven depende del rol/usuario (ver ReporteRegistrosService).
    <HttpGet>
    <Route("api/reportes/{id:guid}/registros")>
    Public Function ObtenerRegistros(id As Guid) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicioRegistros.ObtenerRegistros(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return Prohibido(resultado.Mensaje)
            Case Else
                Return Ok(resultado.Registros)
        End Select
    End Function

    ' Los mismos registros (y columnas) que /registros, como .xlsx.
    <HttpGet>
    <Route("api/reportes/{id:guid}/exportar")>
    Public Function Exportar(id As Guid) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicioRegistros.Exportar(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return Prohibido(resultado.Mensaje)
            Case ResultadoOperacionReporte.Fallo
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = resultado.Mensaje})
            Case Else
                Dim respuesta As New HttpResponseMessage(HttpStatusCode.OK) With {
                    .Content = New ByteArrayContent(resultado.Archivo)
                }
                respuesta.Content.Headers.ContentType = New MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                respuesta.Content.Headers.ContentDisposition = New ContentDispositionHeaderValue("attachment") With {
                    .FileName = resultado.NombreArchivo
                }
                Return ResponseMessage(respuesta)
        End Select
    End Function

    Private Function Prohibido(mensaje As String) As IHttpActionResult
        If String.IsNullOrEmpty(mensaje) Then Return StatusCode(HttpStatusCode.Forbidden)
        Return Content(HttpStatusCode.Forbidden, New With {.mensaje = mensaje})
    End Function

    <HttpPost>
    <Route("api/reportes")>
    Public Function Crear(<FromBody> input As ReporteGuardadoInputDTO) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del reporte.")

        Dim resultado = servicioGuardados.Crear(usuario, input)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Reporte)
        End Select
    End Function

    <HttpPut>
    <Route("api/reportes/{id:guid}")>
    Public Function Actualizar(id As Guid, <FromBody> input As ReporteGuardadoInputDTO) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del reporte.")

        Dim resultado = servicioGuardados.Actualizar(id, usuario, input)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacionReporte.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Reporte)
        End Select
    End Function

    <HttpDelete>
    <Route("api/reportes/{id:guid}")>
    Public Function Eliminar(id As Guid) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicioGuardados.Eliminar(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacionReporte.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

End Class
