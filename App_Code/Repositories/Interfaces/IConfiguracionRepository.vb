Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IConfiguracionRepository
        Function ObtenerTodas() As IQueryable(Of CONFIGURACIONES)
        Function ObtenerPorCodigo(codigo As String) As CONFIGURACIONES
        Sub Actualizar(configuracion As CONFIGURACIONES)
    End Interface
End Namespace
