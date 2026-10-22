Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface ICentroRepository
        Function ObtenerTodos() As IQueryable(Of CENTROS)
        Function ObtenerPorCodigo(codigo As String) As CENTROS
        Sub Agregar(centro As CENTROS)
        Sub Actualizar(centro As CENTROS)
        Sub Eliminar(centro As CENTROS)
    End Interface
End Namespace
