Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class RemuneracionRepository
        Implements IRemuneracionRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ListarRemuneraciones() As IQueryable(Of REMUNERACIONES) Implements IRemuneracionRepository.ListarRemuneraciones
            Return cntx.REMUNERACIONES.AsNoTracking()
        End Function
    End Class
End Namespace
