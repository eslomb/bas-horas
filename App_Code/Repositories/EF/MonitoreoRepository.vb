Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class MonitoreoRepository
        Implements IMonitoreoRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of MONITOREO) Implements IMonitoreoRepository.ObtenerTodos
            Return cntx.MONITOREO
        End Function

        ' Fecha es StoreGeneratedPattern=Computed: la pone el default de la base.
        Public Sub Agregar(visita As MONITOREO) Implements IMonitoreoRepository.Agregar
            cntx.MONITOREO.Add(visita)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
