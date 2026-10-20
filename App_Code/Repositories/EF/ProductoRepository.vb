Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class ProductoRepository
        Implements IProductoRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of PRODUCTOS) Implements IProductoRepository.ObtenerTodos
            Return cntx.PRODUCTOS.OrderBy(Function(p) p.Producto)
        End Function

        Public Function ObtenerActivos() As IQueryable(Of PRODUCTOS) Implements IProductoRepository.ObtenerActivos
            Return ObtenerTodos().Where(Function(p) p.Activo)
        End Function
    End Class
End Namespace
