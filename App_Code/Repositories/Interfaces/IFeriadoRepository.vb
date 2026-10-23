Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IFeriadoRepository
        Function ObtenerTodos() As IQueryable(Of FERIADOS)
        Function ObtenerPorFecha(fecha As Date) As FERIADOS
        Sub Agregar(feriado As FERIADOS)
        Sub Actualizar(feriado As FERIADOS)
        Sub Eliminar(feriado As FERIADOS)
    End Interface
End Namespace
