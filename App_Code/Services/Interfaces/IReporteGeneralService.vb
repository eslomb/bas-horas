Imports DTOs.Reportes

Namespace Services.Interfaces
    Public Interface IReporteGeneralService
        Function GenerarReporteGeneral(filtros As FiltroReporte) As List(Of ReporteFilaDTO)
        Function GenerarReporteGeneralPaginado(filtros As FiltroReporte, pagina As Integer, tamanioPagina As Integer) As ResultadoPaginadoDTO(Of ReporteFilaDTO)
    End Interface
End Namespace
