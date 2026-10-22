Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class CentroRepository
        Implements ICentroRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of CENTROS) Implements ICentroRepository.ObtenerTodos
            Return cntx.CENTROS.OrderBy(Function(c) c.Nombre)
        End Function

        Public Function ObtenerPorCodigo(codigo As String) As CENTROS Implements ICentroRepository.ObtenerPorCodigo
            Return cntx.CENTROS.FirstOrDefault(Function(c) c.CodCen = codigo)
        End Function

        Public Sub Agregar(centro As CENTROS) Implements ICentroRepository.Agregar
            cntx.CENTROS.Add(centro)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(centro As CENTROS) Implements ICentroRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(centro As CENTROS) Implements ICentroRepository.Eliminar
            cntx.CENTROS.Remove(centro)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
