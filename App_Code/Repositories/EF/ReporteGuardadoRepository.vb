Imports System.Linq
Imports System.Data.Entity
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class ReporteGuardadoRepository
        Implements IReporteGuardadoRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function Listar(usuario As String) As List(Of REPORTES) Implements IReporteGuardadoRepository.Listar
            Return cntx.REPORTES.
                Where(Function(r) r.Usuario = usuario).
                OrderBy(Function(r) r.Titulo).
                ToList()
        End Function

        Public Function ObtenerPorId(id As Guid) As REPORTES Implements IReporteGuardadoRepository.ObtenerPorId
            Return cntx.REPORTES.FirstOrDefault(Function(r) r.IDRep = id)
        End Function

        Public Sub Agregar(reporte As REPORTES) Implements IReporteGuardadoRepository.Agregar
            cntx.REPORTES.Add(reporte)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(reporte As REPORTES) Implements IReporteGuardadoRepository.Actualizar
            cntx.REPORTES.Attach(reporte)
            cntx.Entry(reporte).State = EntityState.Modified
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(id As Guid) Implements IReporteGuardadoRepository.Eliminar
            Dim reporte = ObtenerPorId(id)
            If reporte IsNot Nothing Then
                cntx.REPORTES.Remove(reporte)
                cntx.SaveChanges()
            End If
        End Sub
    End Class
End Namespace
