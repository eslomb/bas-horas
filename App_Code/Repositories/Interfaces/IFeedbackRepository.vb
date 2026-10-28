Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IFeedbackRepository
        Function ObtenerTodos() As IQueryable(Of FEEDBACK)
        Function ObtenerPorId(id As Integer) As FEEDBACK
        Sub Agregar(feedback As FEEDBACK)
        Sub Actualizar(feedback As FEEDBACK)
        Sub Eliminar(feedback As FEEDBACK)
    End Interface
End Namespace
