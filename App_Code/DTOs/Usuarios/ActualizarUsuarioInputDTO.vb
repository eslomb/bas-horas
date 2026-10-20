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
    End Class
End Namespace
