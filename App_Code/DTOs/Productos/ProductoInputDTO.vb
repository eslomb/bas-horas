Namespace DTOs.Productos
    ' El IDPro es identity: en el alta lo genera la base y en la edición sale de la ruta.
    Public Class ProductoInputDTO
        Public Property Codigo As String
        Public Property Producto As String
        Public Property Activo As Boolean?
        Public Property FProyIni As Date?
        Public Property FProyFin As Date?
    End Class
End Namespace
