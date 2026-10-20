Imports DTOs.Reportes

Namespace Services.Interfaces
    Public Interface IReporteGuardadoService
        Function Listar(usuario As String) As List(Of ReporteGuardadoDTO)
        Function ObtenerPorId(id As Guid, usuario As String) As ResultadoReporte
        Function EjecutarPorId(id As Guid, usuario As String) As ResultadoReporte
        Function EjecutarPorIdPaginado(id As Guid, usuario As String, pagina As Integer, tamanioPagina As Integer) As ResultadoReporte
        Function ExportarPorId(id As Guid, usuario As String, esAdministrador As Boolean) As ResultadoReporte
        Function Crear(usuario As String, input As ReporteGuardadoInputDTO) As ResultadoReporte
        Function Actualizar(id As Guid, usuario As String, input As ReporteGuardadoInputDTO) As ResultadoReporte
        Function Eliminar(id As Guid, usuario As String) As ResultadoReporte
    End Interface
End Namespace
