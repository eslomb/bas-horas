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
        Public Property Filas As List(Of ReporteFilaDTO)
        ' PAGINADO DESACTIVADO
        'Public Property FilasPaginadas As ResultadoPaginadoDTO(Of ReporteFilaDTO)
        Public Property Archivo As Byte()
        Public Property Mensaje As String
    End Class
End Namespace
