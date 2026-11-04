Namespace DTOs.Monitoreo
    ' Cantidad de visitas de un usuario a una URL en un día (Fecha sin hora).
    Public Class VisitaAgrupadaDTO
        Public Property Usuario As String
        Public Property Url As String
        Public Property Fecha As Date
        Public Property Cantidad As Integer
    End Class
End Namespace
