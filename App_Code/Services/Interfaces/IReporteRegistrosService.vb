Imports DTOs.Reportes

Namespace Services.Interfaces
    Public Interface IReporteRegistrosService
        ' Ejecuta el reporte guardado y devuelve una página de sus registros (Registros), con los campos que corresponden a quien lo pide.
        Function ObtenerRegistros(id As Guid, usuario As String, pagina As Integer, tamanioPagina As Integer) As ResultadoReporte
        ' Todos los registros, sin paginar, como .xlsx (Archivo + NombreArchivo).
        Function Exportar(id As Guid, usuario As String) As ResultadoReporte
    End Interface
End Namespace
