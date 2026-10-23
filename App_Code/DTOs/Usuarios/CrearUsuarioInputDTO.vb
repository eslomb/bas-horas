Namespace DTOs.Usuarios
    ' Alta de un usuario. Hereda los campos de la edición (incluida Password, que acá es obligatoria)
    ' y suma el nombre de usuario, que es la clave y no se puede cambiar después.
    Public Class CrearUsuarioInputDTO
        Inherits ActualizarUsuarioInputDTO

        Public Property UserName As String
    End Class
End Namespace
