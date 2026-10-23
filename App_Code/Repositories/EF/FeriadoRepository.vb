Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class FeriadoRepository
        Implements IFeriadoRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of FERIADOS) Implements IFeriadoRepository.ObtenerTodos
            Return cntx.FERIADOS.OrderBy(Function(f) f.Feriado)
        End Function

        Public Function ObtenerPorFecha(fecha As Date) As FERIADOS Implements IFeriadoRepository.ObtenerPorFecha
            Return cntx.FERIADOS.FirstOrDefault(Function(f) f.Feriado = fecha)
        End Function

        Public Sub Agregar(feriado As FERIADOS) Implements IFeriadoRepository.Agregar
            cntx.FERIADOS.Add(feriado)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(feriado As FERIADOS) Implements IFeriadoRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(feriado As FERIADOS) Implements IFeriadoRepository.Eliminar
            cntx.FERIADOS.Remove(feriado)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
