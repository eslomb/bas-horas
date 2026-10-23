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

        Public Function ObtenerPorId(id As Integer) As PRODUCTOS Implements IProductoRepository.ObtenerPorId
            Return cntx.PRODUCTOS.FirstOrDefault(Function(p) p.IDPro = id)
        End Function

        Public Function TieneRegistros(id As Integer) As Boolean Implements IProductoRepository.TieneRegistros
            Return cntx.REGISTROS.Any(Function(r) r.idProducto = id)
        End Function

        ' PROYECTOS no está en el modelo EF pero tiene FK hacia PRODUCTOS, así que se consulta por SQL.
        Public Function TieneProyectos(id As Integer) As Boolean Implements IProductoRepository.TieneProyectos
            Dim cantidad = cntx.Database.SqlQuery(Of Integer)(
                "SELECT COUNT(*) FROM PROYECTOS WHERE ProductoBASHoras = @p0", id).First()
            Return cantidad > 0
        End Function

        Public Sub Agregar(producto As PRODUCTOS) Implements IProductoRepository.Agregar
            cntx.PRODUCTOS.Add(producto)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(producto As PRODUCTOS) Implements IProductoRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(producto As PRODUCTOS) Implements IProductoRepository.Eliminar
            cntx.PRODUCTOS.Remove(producto)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
