Namespace DTOs.Reportes
    Public Enum ResultadoOperacionReporte
        Ok
        NoEncontrado
        Prohibido
        Fallo
    End Enum

    Public Class ResultadoReporte
        Public Property Estado As ResultadoOperacionReporte
        Public Property Reporte As ReporteGuardadoDTO
        ' Registros del reporte: cada elemento es Basico/Avanzado/Full según quién lo pide.
        Public Property Registros As List(Of RegistroReporteBasicoDTO)
        Public Property Archivo As Byte()
        Public Property NombreArchivo As String
        Public Property Mensaje As String
    End Class
End Namespace
