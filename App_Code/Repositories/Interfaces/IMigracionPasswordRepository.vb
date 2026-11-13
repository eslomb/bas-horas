Namespace Repositories.Interfaces
    Public Interface IMigracionPasswordRepository
        Function ObtenerPendientes() As List(Of PasswordPendienteMigracion)
        Function Migrar(pendiente As PasswordPendienteMigracion) As Boolean
    End Interface

    ' Fila de aspnet_Membership con la clave todavía en texto plano (PasswordFormat = 0).
    Public Class PasswordPendienteMigracion
        Public Property UserName As String
        Public Property Password As String
        Public Property PasswordSalt As String
    End Class
End Namespace
