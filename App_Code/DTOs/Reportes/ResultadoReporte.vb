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
        ' Página de registros del reporte: cada elemento de Items es Basico/Avanzado/Full según quién lo pide.
        Public Property Registros As DTOs.ResultadoPaginado(Of RegistroReporteBasicoDTO)
        Public Property Archivo As Byte()
        Public Property NombreArchivo As String
        Public Property Mensaje As String
    End Class
End Namespace
