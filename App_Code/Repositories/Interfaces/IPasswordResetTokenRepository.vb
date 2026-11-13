Namespace Repositories.Interfaces
    Public Interface IPasswordResetTokenRepository
        Sub Crear(token As PASSWORD_RESET_TOKENS)
        Function ObtenerPorHash(tokenHash As String) As PASSWORD_RESET_TOKENS
        Function MarcarUsado(idToken As Integer) As Boolean
        Sub InvalidarDelUsuario(userName As String)
        Function ContarRecientes(userName As String, origen As String, desde As Date) As Integer
    End Interface
End Namespace
