Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Web.Http
Imports Auth
Imports DTOs.Analisis
Imports Services.Interfaces

' Análisis de horas por actividad (spAnalisisActividades). Solo Administrador. Errores como { "mensaje": "..." }.
<JwtAuthorize(Roles:="Administrador")>
Public Class AnalisisController
    Inherits ControladorApiBase

    Private Const TipoXlsx As String = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"

    Private ReadOnly servicio As IAnalisisService

    Public Sub New(servicio As IAnalisisService)
        Me.servicio = servicio
    End Sub

    ' concepto: producto, pais o usuario. Fechas yyyy-MM-dd, ambas opcionales e inclusive.
    <HttpGet>
    <Route("api/analisis/actividades")>
    Public Function Actividades(concepto As String, Optional desde As Date? = Nothing, Optional hasta As Date? = Nothing) As IHttpActionResult
        Dim resultado = servicio.AnalizarActividades(concepto, desde, hasta)

        If resultado.Estado = ResultadoOperacionAnalisis.Invalido Then
            Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
        End If
        Return Ok(resultado.Items)
    End Function

    ' Los conceptos elegidos (ids repetido: ?ids=1&ids=2) como tabla cruzada .xlsx: concepto x actividad, con totales.
    <HttpGet>
    <Route("api/analisis/actividades/exportar")>
    Public Function Exportar(concepto As String, <FromUri> ids As String(), Optional desde As Date? = Nothing, Optional hasta As Date? = Nothing) As IHttpActionResult
        Dim resultado = servicio.ExportarActividades(concepto, desde, hasta, ids)

        If resultado.Estado = ResultadoOperacionAnalisis.Invalido Then
            Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
        End If

        Dim respuesta As New HttpResponseMessage(HttpStatusCode.OK) With {
            .Content = New ByteArrayContent(resultado.Contenido)
        }
        respuesta.Content.Headers.ContentType = New MediaTypeHeaderValue(TipoXlsx)
        respuesta.Content.Headers.ContentDisposition = New ContentDispositionHeaderValue("attachment") With {
            .FileName = resultado.NombreArchivo
        }
        Return ResponseMessage(respuesta)
    End Function

End Class
