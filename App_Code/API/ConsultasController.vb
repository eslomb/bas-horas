Imports System.Web.Http
Imports Services.Interfaces

Public Class ConsultasController
    Inherits ApiController

    Private ReadOnly servicio As IConsultasService

    Public Sub New(servicio As IConsultasService)
        Me.servicio = servicio
    End Sub

    <HttpGet>
    <Route("api/consultas/productos")>
    Public Function ListarProductos(Optional soloActivos As Boolean = True) As IHttpActionResult
        Return Ok(servicio.ListarProductos(soloActivos))
    End Function

    <HttpGet>
    <Route("api/consultas/actividades")>
    Public Function ListarActividades(Optional soloHabilitadas As Boolean = True) As IHttpActionResult
        Return Ok(servicio.ListarActividades(soloHabilitadas))
    End Function

    <HttpGet>
    <Route("api/consultas/centros")>
    Public Function ListarCentros() As IHttpActionResult
        Return Ok(servicio.ListarCentros())
    End Function

End Class
