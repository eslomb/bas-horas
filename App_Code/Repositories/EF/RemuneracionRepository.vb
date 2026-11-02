Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class RemuneracionRepository
        Implements IRemuneracionRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ListarRemuneraciones() As IQueryable(Of REMUNERACIONES) Implements IRemuneracionRepository.ListarRemuneraciones
            Return cntx.REMUNERACIONES.AsNoTracking()
        End Function

        Public Function ObtenerPorId(id As Integer) As REMUNERACIONES Implements IRemuneracionRepository.ObtenerPorId
            Return cntx.REMUNERACIONES.FirstOrDefault(Function(r) r.IDRem = id)
        End Function

        ' Anio es smallint en la tabla: se convierte antes para que EF no compare contra int.
        Public Function ObtenerPorAnios(anios As IEnumerable(Of Integer)) As IQueryable(Of REMUNERACIONES) Implements IRemuneracionRepository.ObtenerPorAnios
            Dim aniosShort = anios.Select(Function(a) CShort(a)).ToList()
            Return cntx.REMUNERACIONES.Where(Function(r) aniosShort.Contains(r.Anio))
        End Function

        Public Function Existe(usuario As String, anio As Integer, mes As Integer, excluirId As Integer) As Boolean Implements IRemuneracionRepository.Existe
            Dim anioShort = CShort(anio)
            Dim mesShort = CShort(mes)
            Return cntx.REMUNERACIONES.Any(Function(r) r.Usuario = usuario AndAlso r.Anio = anioShort AndAlso r.Mes = mesShort AndAlso r.IDRem <> excluirId)
        End Function

        Public Sub Agregar(remuneracion As REMUNERACIONES) Implements IRemuneracionRepository.Agregar
            cntx.REMUNERACIONES.Add(remuneracion)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(remuneracion As REMUNERACIONES) Implements IRemuneracionRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(remuneracion As REMUNERACIONES) Implements IRemuneracionRepository.Eliminar
            cntx.REMUNERACIONES.Remove(remuneracion)
            cntx.SaveChanges()
        End Sub

        Public Sub AgregarSinGuardar(remuneracion As REMUNERACIONES) Implements IRemuneracionRepository.AgregarSinGuardar
            cntx.REMUNERACIONES.Add(remuneracion)
        End Sub

        Public Sub Guardar() Implements IRemuneracionRepository.Guardar
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
