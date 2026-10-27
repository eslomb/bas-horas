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

    ' PAGINADO DESACTIVADO
    'Private Const TamanioPaginaMaximo As Integer = 200
    'Private Const MensajePaginadoInvalido As String = "pagina debe ser >= 1 y tamanio debe estar entre 1 y 200."

    Private ReadOnly servicio As IReporteGeneralService
    Private ReadOnly servicioGuardados As IReporteGuardadoService

    Public Sub New(servicio As IReporteGeneralService, servicioGuardados As IReporteGuardadoService)
        Me.servicio = servicio
        Me.servicioGuardados = servicioGuardados
    End Sub

    <HttpPost>
    <Route("api/reportes/general")>
    Public Function GenerarReporteGeneral(<FromBody> filtros As FiltroReporte) As IHttpActionResult
        If filtros Is Nothing Then Return BadRequest("Debe enviar los filtros del reporte.")

        Return Ok(servicio.GenerarReporteGeneral(filtros))
    End Function

    ' PAGINADO DESACTIVADO: POST api/reportes/general/paginado
    '<HttpPost>
    '<Route("api/reportes/general/paginado")>
    'Public Function GenerarReporteGeneralPaginado(<FromBody> filtros As FiltroReporte, Optional pagina As Integer = 1, Optional tamanio As Integer = 50) As IHttpActionResult
    '    If filtros Is Nothing Then Return BadRequest("Debe enviar los filtros del reporte.")
    '    If Not ParametrosPaginadoValidos(pagina, tamanio) Then Return BadRequest(MensajePaginadoInvalido)
    '
    '    Return Ok(servicio.GenerarReporteGeneralPaginado(filtros, pagina, tamanio))
    'End Function

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

    <HttpGet>
    <Route("api/reportes/{id:guid}/resultado")>
    Public Function EjecutarPorId(id As Guid) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicioGuardados.EjecutarPorId(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case Else
                Return Ok(resultado.Filas)
        End Select
    End Function

    ' PAGINADO DESACTIVADO: GET api/reportes/{id:guid}/resultado/paginado
    '<HttpGet>
    '<Route("api/reportes/{id:guid}/resultado/paginado")>
    'Public Function EjecutarPorIdPaginado(id As Guid, Optional pagina As Integer = 1, Optional tamanio As Integer = 50) As IHttpActionResult
    '    Dim usuario = ObtenerUsuarioActual()
    '    If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
    '    If Not ParametrosPaginadoValidos(pagina, tamanio) Then Return BadRequest(MensajePaginadoInvalido)
    '
    '    Dim resultado = servicioGuardados.EjecutarPorIdPaginado(id, usuario, pagina, tamanio)
    '
    '    Select Case resultado.Estado
    '        Case ResultadoOperacionReporte.NoEncontrado
    '            Return NotFound()
    '        Case ResultadoOperacionReporte.Prohibido
    '            Return StatusCode(HttpStatusCode.Forbidden)
    '        Case Else
    '            Return Ok(resultado.FilasPaginadas)
    '    End Select
    'End Function

    <HttpGet>
    <Route("api/reportes/{id:guid}/exportar")>
    Public Function Exportar(id As Guid) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicioGuardados.ExportarPorId(id, usuario, User.IsInRole(Aplicacion.Grupos.Administrador))

        Select Case resultado.Estado
            Case ResultadoOperacionReporte.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionReporte.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacionReporte.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Dim respuesta As New HttpResponseMessage(HttpStatusCode.OK) With {
                    .Content = New ByteArrayContent(resultado.Archivo)
                }
                respuesta.Content.Headers.ContentType = New MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                respuesta.Content.Headers.ContentDisposition = New ContentDispositionHeaderValue("attachment") With {
                    .FileName = NombreArchivo(resultado.Reporte.Titulo)
                }
                Return ResponseMessage(respuesta)
        End Select
    End Function

    Private Shared Function NombreArchivo(titulo As String) As String
        Dim nombre = If(String.IsNullOrWhiteSpace(titulo), "Reporte", titulo.Trim())
        For Each c In System.IO.Path.GetInvalidFileNameChars()
            nombre = nombre.Replace(c, "_"c)
        Next
        Return nombre & ".xlsx"
    End Function

    ' PAGINADO DESACTIVADO
    'Private Shared Function ParametrosPaginadoValidos(pagina As Integer, tamanio As Integer) As Boolean
    '    Return pagina >= 1 AndAlso tamanio >= 1 AndAlso tamanio <= TamanioPaginaMaximo
    'End Function

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
