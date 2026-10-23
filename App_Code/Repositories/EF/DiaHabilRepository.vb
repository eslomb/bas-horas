Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class DiaHabilRepository
        Implements IDiaHabilRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of DIAS_HABILES) Implements IDiaHabilRepository.ObtenerTodos
            Return cntx.DIAS_HABILES.OrderBy(Function(d) d.Anio).ThenBy(Function(d) d.Mes)
        End Function

        Public Function ObtenerPorId(id As Integer) As DIAS_HABILES Implements IDiaHabilRepository.ObtenerPorId
            Return cntx.DIAS_HABILES.FirstOrDefault(Function(d) d.Id = id)
        End Function

        Public Function ExisteAnioMes(anio As Integer, mes As Integer, excluirId As Integer) As Boolean Implements IDiaHabilRepository.ExisteAnioMes
            Return cntx.DIAS_HABILES.Any(Function(d) d.Anio = anio AndAlso d.Mes = mes AndAlso d.Id <> excluirId)
        End Function

        Public Sub Agregar(diaHabil As DIAS_HABILES) Implements IDiaHabilRepository.Agregar
            cntx.DIAS_HABILES.Add(diaHabil)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(diaHabil As DIAS_HABILES) Implements IDiaHabilRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(diaHabil As DIAS_HABILES) Implements IDiaHabilRepository.Eliminar
            cntx.DIAS_HABILES.Remove(diaHabil)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
