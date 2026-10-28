Namespace DTOs.Feedback
    Public Enum ResultadoOperacionFeedback
        Ok
        NoEncontrado
        Prohibido
        Invalido
        Fallo
    End Enum

    Public Class ResultadoFeedback
        Public Property Estado As ResultadoOperacionFeedback
        Public Property Feedback As FeedbackDTO
        Public Property Mensaje As String
    End Class
End Namespace
