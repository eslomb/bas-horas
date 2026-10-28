Namespace DTOs.Feedback
    ' Usuario sale del token y Fecha la pone la base (default GETDATE()): no se reciben acá.
    ' Email es opcional: si viene vacío se usa el email del usuario en Membership.
    Public Class FeedbackInputDTO
        Public Property Email As String
        Public Property Comentario As String
        Public Property Tipo As String
    End Class
End Namespace
