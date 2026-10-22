Imports DTOs.Productos

Namespace Services.Interfaces
    Public Interface IProductoService
        Function Listar(soloActivos As Boolean) As List(Of ProductoDTO)
        Function ObtenerPorId(id As Integer) As ResultadoProducto
        Function Crear(input As ProductoInputDTO) As ResultadoProducto
        Function Actualizar(id As Integer, input As ProductoInputDTO) As ResultadoProducto
        Function Eliminar(id As Integer) As ResultadoProducto
    End Interface
End Namespace
