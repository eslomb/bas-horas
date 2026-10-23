Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IDiaHabilRepository
        Function ObtenerTodos() As IQueryable(Of DIAS_HABILES)
        Function ObtenerPorId(id As Integer) As DIAS_HABILES
        Function ExisteAnioMes(anio As Integer, mes As Integer, excluirId As Integer) As Boolean
        Sub Agregar(diaHabil As DIAS_HABILES)
        Sub Actualizar(diaHabil As DIAS_HABILES)
        Sub Eliminar(diaHabil As DIAS_HABILES)
    End Interface
End Namespace
