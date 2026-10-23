Namespace DTOs.Usuarios
    Public Enum ResultadoOperacionUsuario
        Ok
        NoEncontrado
        Invalido
        Conflicto
        Fallo
    End Enum

    Public Class ResultadoUsuario
        Public Property Estado As ResultadoOperacionUsuario
        Public Property Usuario As UsuarioDTO
        Public Property Mensaje As String
    End Class
End Namespace
