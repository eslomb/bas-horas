Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IActividadRepository
        Function ObtenerTodas() As IQueryable(Of ACTIVIDADES)
        Function ObtenerHabilitadas() As IQueryable(Of ACTIVIDADES)
    End Interface
End Namespace
