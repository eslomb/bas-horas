Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IPaisRepository
        Function ObtenerTodos() As IQueryable(Of PAISES)
    End Interface
End Namespace
