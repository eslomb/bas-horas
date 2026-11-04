Namespace DTOs.Monitoreo
    ' Usuario sale del token y Fecha la pone la base (default GETDATE()): no se reciben acá.
    ' Hoy solo se registra la URL; si se suman datos a futuro, agregarlos acá y en la tabla.
    Public Class MonitoreoInputDTO
        Public Property Url As String
    End Class
End Namespace
