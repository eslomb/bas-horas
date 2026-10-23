Namespace DTOs.Usuarios
    ' Edición completa de un usuario, incluyendo datos administrativos. Solo para uso por un Administrador.
    Public Class ActualizarUsuarioInputDTO
        Public Property Email As String
        Public Property Comentario As String
        Public Property Legajo As String
        Public Property Modalidad As Decimal
        Public Property PeriodoAbierto As Int16
        Public Property Centro As String
        Public Property CUIL As String
        Public Property Dependencia As String
        ' Uno de Aplicacion.Grupos (Basico, Avanzado, Administrador). El usuario queda solo en ese rol.
        Public Property Rol As String
        ' Se guarda en IsApproved de Membership. Nullable para que omitirlo sea un 400 y no deshabilite al usuario sin querer.
        Public Property Habilitado As Boolean?
        ' Opcional en la edición: si es Nothing o vacía no se toca la clave; si viene, se fija como nueva clave.
        Public Property Password As String
    End Class
End Namespace
