Imports DTOs.Monitoreo

Namespace Services.Interfaces
    Public Interface IMonitoreoService
        Function Registrar(usuario As String, input As MonitoreoInputDTO) As ResultadoMonitoreo
        Function ObtenerVisitas(desde As Date?, hasta As Date?) As ResultadoMonitoreo
    End Interface
End Namespace
