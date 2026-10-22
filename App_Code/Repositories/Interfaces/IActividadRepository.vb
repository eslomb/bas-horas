Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IActividadRepository
        Function ObtenerTodas() As IQueryable(Of ACTIVIDADES)
        Function ObtenerHabilitadas() As IQueryable(Of ACTIVIDADES)
        Function ObtenerPorId(id As Integer) As ACTIVIDADES
        Function ExisteCodigo(codigo As String, excluirId As Integer) As Boolean
        Function TieneRegistros(id As Integer) As Boolean
        Function TieneTareas(id As Integer) As Boolean
        Sub Agregar(actividad As ACTIVIDADES)
        Sub Actualizar(actividad As ACTIVIDADES)
        Sub Eliminar(actividad As ACTIVIDADES)
    End Interface
End Namespace
