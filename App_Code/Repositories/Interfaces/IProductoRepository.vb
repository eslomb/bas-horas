Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IProductoRepository
        Function ObtenerTodos() As IQueryable(Of PRODUCTOS)
        Function ObtenerActivos() As IQueryable(Of PRODUCTOS)
        Function ObtenerPorId(id As Integer) As PRODUCTOS
        Function TieneRegistros(id As Integer) As Boolean
        Function TieneProyectos(id As Integer) As Boolean
        Sub Agregar(producto As PRODUCTOS)
        Sub Actualizar(producto As PRODUCTOS)
        Sub Eliminar(producto As PRODUCTOS)
    End Interface
End Namespace
