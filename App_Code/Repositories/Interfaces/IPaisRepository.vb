Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IPaisRepository
        Function ObtenerTodos() As IQueryable(Of PAISES)
        Function ObtenerPreferidos() As IQueryable(Of PAISES)
        Function ObtenerPorCodigo(codigo As String) As PAISES
        Function TieneRegistros(codigo As String) As Boolean
        Sub Agregar(pais As PAISES)
        Sub Actualizar(pais As PAISES)
        Sub Eliminar(pais As PAISES)
    End Interface
End Namespace
