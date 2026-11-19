Namespace Repositories.Interfaces
    Public Interface INotificadorFeedback
        ' Lanza InvalidOperationException si el servidor de correo rechaza el envío.
        Sub EnviarRespuesta(email As String, respuesta As String, comentarioOriginal As String)
    End Interface
End Namespace
