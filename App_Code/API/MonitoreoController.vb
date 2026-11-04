Imports System.Net
Imports System.Web.Http
Imports Auth
Imports DTOs.Monitoreo
Imports Services.Interfaces

' Monitoreo de uso del frontend. Errores como { "mensaje": "..." }.
<JwtAuthorize>
Public Class MonitoreoController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IMonitoreoService

    Public Sub New(servicio As IMonitoreoService)
        Me.servicio = servicio
    End Sub

    ' Cualquier usuario logueado. Usuario y fecha salen del token y del servidor, no del body.
    <HttpPost>
    <Route("api/monitoreo")>
    Public Function Registrar(<FromBody> input As MonitoreoInputDTO) As IHttpActionResult
        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()
        If input Is Nothing Then Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "Debe enviar la URL visitada."})

        Dim resultado = servicio.Registrar(actual, input)

        Select Case resultado.Estado
            Case ResultadoOperacionMonitoreo.Invalido
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacionMonitoreo.Fallo
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = resultado.Mensaje})
            Case Else
                Return Ok()
        End Select
    End Function

    ' Cantidad de visitas por usuario, URL y día. Fechas yyyy-MM-dd, ambas opcionales e inclusive.
    <HttpGet>
    <JwtAuthorize(Roles:="Administrador")>
    <Route("api/monitoreo")>
    Public Function Listar(Optional desde As Date? = Nothing, Optional hasta As Date? = Nothing) As IHttpActionResult
        Dim resultado = servicio.ObtenerVisitas(desde, hasta)

        If resultado.Estado = ResultadoOperacionMonitoreo.Invalido Then
            Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
        End If
        Return Ok(resultado.Items)
    End Function

End Class
