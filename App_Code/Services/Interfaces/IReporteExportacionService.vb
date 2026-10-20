Imports DTOs.Reportes

Namespace Services.Interfaces
    Public Interface IReporteExportacionService
        ' Genera el .xlsx del reporte. Las columnas de remuneración/costo solo se incluyen si el usuario está habilitado.
        Function GenerarExcel(filtros As FiltroReporte, titulo As String, usuarioActual As String, esAdministrador As Boolean) As Byte()
    End Interface
End Namespace
