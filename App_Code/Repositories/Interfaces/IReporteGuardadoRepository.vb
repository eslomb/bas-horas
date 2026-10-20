Namespace Repositories.Interfaces
    Public Interface IReporteGuardadoRepository
        Function Listar(usuario As String) As List(Of REPORTES)
        Function ObtenerPorId(id As Guid) As REPORTES
        Sub Agregar(reporte As REPORTES)
        Sub Actualizar(reporte As REPORTES)
        Sub Eliminar(id As Guid)
    End Interface
End Namespace
