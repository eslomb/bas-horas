Imports DTOs.Reportes

Namespace Services.Interfaces
    Public Interface IReporteRegistrosService
        ' Ejecuta el reporte guardado y devuelve sus registros con los campos que corresponden a quien lo pide.
        Function ObtenerRegistros(id As Guid, usuario As String) As ResultadoReporte
        ' Lo mismo, como .xlsx (Archivo + NombreArchivo).
        Function Exportar(id As Guid, usuario As String) As ResultadoReporte
    End Interface
End Namespace
