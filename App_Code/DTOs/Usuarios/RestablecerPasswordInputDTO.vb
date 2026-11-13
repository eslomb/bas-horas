Namespace DTOs.Usuarios
    Public Class ValidarTokenInputDTO
        Public Property Token As String
    End Class

    Public Class RestablecerPasswordInputDTO
        Public Property Token As String
        Public Property Password As String
    End Class

    Public Enum ResultadoRestablecerPassword
        Ok
        TokenInvalido
        PasswordNoValida
        Fallo
    End Enum
End Namespace
