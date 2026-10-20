Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class PaisRepository
        Implements IPaisRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of PAISES) Implements IPaisRepository.ObtenerTodos
            Return cntx.PAISES.OrderBy(Function(p) p.Pais)
        End Function
    End Class
End Namespace
