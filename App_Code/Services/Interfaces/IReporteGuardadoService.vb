Imports DTOs.Reportes

Namespace Services.Interfaces
    ' CRUD de los reportes guardados (tabla REPORTES). Ejecutar/exportar un reporte: IReporteRegistrosService.
    Public Interface IReporteGuardadoService
        Function Listar(usuario As String) As List(Of ReporteGuardadoDTO)
        Function ObtenerPorId(id As Guid, usuario As String) As ResultadoReporte
        Function Crear(usuario As String, input As ReporteGuardadoInputDTO) As ResultadoReporte
        Function Actualizar(id As Guid, usuario As String, input As ReporteGuardadoInputDTO) As ResultadoReporte
        Function Eliminar(id As Guid, usuario As String) As ResultadoReporte
    End Interface
End Namespace
