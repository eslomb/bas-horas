Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface ICentroRepository
        Function ObtenerTodos() As IQueryable(Of CENTROS)
    End Interface
End Namespace
