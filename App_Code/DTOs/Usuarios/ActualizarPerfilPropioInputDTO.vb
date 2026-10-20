Namespace DTOs.Usuarios
    ' Campos que un usuario puede editar de su propia cuenta (sin ser Admin).
    ' Legajo/Centro/CUIL/Dependencia/Modalidad son datos administrativos: solo editables por un Admin (ver ActualizarUsuarioInputDTO).
    Public Class ActualizarPerfilPropioInputDTO
        Public Property Email As String
        Public Property Comentario As String
    End Class
End Namespace
