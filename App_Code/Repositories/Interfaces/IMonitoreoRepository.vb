Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IMonitoreoRepository
        Function ObtenerTodos() As IQueryable(Of MONITOREO)
        Sub Agregar(visita As MONITOREO)
    End Interface
End Namespace
