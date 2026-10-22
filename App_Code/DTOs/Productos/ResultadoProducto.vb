Namespace DTOs.Productos
    Public Enum ResultadoOperacionProducto
        Ok
        NoEncontrado
        EnUso
        Invalido
        Fallo
    End Enum

    Public Class ResultadoProducto
        Public Property Estado As ResultadoOperacionProducto
        Public Property Producto As ProductoDTO
        Public Property Mensaje As String
    End Class
End Namespace
