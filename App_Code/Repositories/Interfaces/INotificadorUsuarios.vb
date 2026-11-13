Namespace Repositories.Interfaces
    Public Interface INotificadorUsuarios
        Sub EnviarLinkRecuperacion(email As String, link As String, minutosValidez As Integer)
    End Interface
End Namespace
