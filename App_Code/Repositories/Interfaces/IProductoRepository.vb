Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IProductoRepository
        Function ObtenerTodos() As IQueryable(Of PRODUCTOS)
        Function ObtenerActivos() As IQueryable(Of PRODUCTOS)
    End Interface
End Namespace
