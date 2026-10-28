Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class FeedbackRepository
        Implements IFeedbackRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of FEEDBACK) Implements IFeedbackRepository.ObtenerTodos
            Return cntx.FEEDBACK
        End Function

        Public Function ObtenerPorId(id As Integer) As FEEDBACK Implements IFeedbackRepository.ObtenerPorId
            Return cntx.FEEDBACK.FirstOrDefault(Function(f) f.IDFeedback = id)
        End Function

        ' Fecha es StoreGeneratedPattern=Computed: la pone el default de la base y EF la relee tras el INSERT.
        Public Sub Agregar(feedback As FEEDBACK) Implements IFeedbackRepository.Agregar
            cntx.FEEDBACK.Add(feedback)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(feedback As FEEDBACK) Implements IFeedbackRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(feedback As FEEDBACK) Implements IFeedbackRepository.Eliminar
            cntx.FEEDBACK.Remove(feedback)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
