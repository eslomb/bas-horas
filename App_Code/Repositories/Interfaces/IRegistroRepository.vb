Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IRegistroRepository
        Function ObtenerPorUsuarioYRango(usuario As String, desde As Date, hasta As Date) As IQueryable(Of vRegistros)
        Function ObtenerVistaPorId(idRegistro As Integer) As vRegistros
        Function ObtenerEntidadPorId(idRegistro As Integer) As REGISTROS
        Function Existe(registro As REGISTROS) As Boolean
        Sub Agregar(registro As REGISTROS)
        Sub Actualizar(registro As REGISTROS)
        Sub Eliminar(idRegistro As Integer)
    End Interface
End Namespace
