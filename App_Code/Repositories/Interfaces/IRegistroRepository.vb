Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IRegistroRepository
        Function ObtenerPorUsuarioYRango(usuario As String, desde As Date, hasta As Date) As IQueryable(Of vRegistros)
        ' Registros con duracion de todos los usuarios en el rango (inclusive), solo lectura. Lo usan los costos.
        Function ObtenerTodosEnRango(desde As Date, hasta As Date) As IQueryable(Of REGISTROS)
        Function ObtenerVistaPorId(idRegistro As Integer) As vRegistros
        Function ObtenerEntidadPorId(idRegistro As Integer) As REGISTROS
        Function Existe(registro As REGISTROS) As Boolean
        Function MinutosDelDia(usuario As String, fecha As Date, excluirIdReg As Integer) As Integer
        Sub Agregar(registro As REGISTROS)
        Sub Actualizar(registro As REGISTROS)
        Sub Eliminar(idRegistro As Integer)
    End Interface
End Namespace
