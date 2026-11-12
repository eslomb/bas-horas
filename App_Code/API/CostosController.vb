Imports System.Net
Imports System.Web.Http
Imports Auth
Imports DTOs.Costos
Imports Services.Interfaces

' Costos de las horas cargadas (mismo cálculo que "Costo Tarea" de los reportes). Mismo acceso que remuneraciones:
' Administrador cuyo usuario figura en Utils.AccesoRemuneraciones; el resto recibe 403. Errores como { "mensaje": "..." }.
<RemuneracionesAuthorize>
Public Class CostosController
    Inherits ControladorApiBase

    Private ReadOnly servicio As ICostoService

    Public Sub New(servicio As ICostoService)
        Me.servicio = servicio
    End Sub

    ' Costo de todos los usuarios agrupado por actividad (código de tarea, por defecto) o por centro, de mayor a menor.
    ' Fechas yyyy-MM-dd, obligatorias e inclusive.
    <HttpGet>
    <Route("api/costos")>
    Public Function Listar(Optional desde As Date? = Nothing, Optional hasta As Date? = Nothing, Optional agrupacion As String = Nothing) As IHttpActionResult
        Dim resultado = servicio.Costos(desde, hasta, agrupacion)

        If resultado.Estado = ResultadoOperacionCostos.Invalido Then
            Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
        End If
        Return Ok(resultado.Costos)
    End Function

End Class
