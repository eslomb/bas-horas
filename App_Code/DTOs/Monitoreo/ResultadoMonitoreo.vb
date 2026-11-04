Namespace DTOs.Monitoreo
    Public Enum ResultadoOperacionMonitoreo
        Ok
        Invalido
        Fallo
    End Enum

    Public Class ResultadoMonitoreo
        Public Property Estado As ResultadoOperacionMonitoreo
        Public Property Mensaje As String
        Public Property Items As List(Of VisitaAgrupadaDTO)
    End Class
End Namespace
