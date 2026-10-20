Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class CentroRepository
        Implements ICentroRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of CENTROS) Implements ICentroRepository.ObtenerTodos
            Return cntx.CENTROS.OrderBy(Function(c) c.Nombre)
        End Function
    End Class
End Namespace
