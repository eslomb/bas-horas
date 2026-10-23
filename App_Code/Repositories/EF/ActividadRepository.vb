Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class ActividadRepository
        Implements IActividadRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodas() As IQueryable(Of ACTIVIDADES) Implements IActividadRepository.ObtenerTodas
            Return cntx.ACTIVIDADES.OrderBy(Function(a) a.Actividad)
        End Function

        Public Function ObtenerHabilitadas() As IQueryable(Of ACTIVIDADES) Implements IActividadRepository.ObtenerHabilitadas
            Return ObtenerTodas().Where(Function(a) a.Habilitada = True)
        End Function

        Public Function ObtenerPorId(id As Integer) As ACTIVIDADES Implements IActividadRepository.ObtenerPorId
            Return cntx.ACTIVIDADES.FirstOrDefault(Function(a) a.IDAct = id)
        End Function

        Public Function ExisteCodigo(codigo As String, excluirId As Integer) As Boolean Implements IActividadRepository.ExisteCodigo
            Return cntx.ACTIVIDADES.Any(Function(a) a.Codigo = codigo AndAlso a.IDAct <> excluirId)
        End Function

        Public Function TieneRegistros(id As Integer) As Boolean Implements IActividadRepository.TieneRegistros
            Return cntx.REGISTROS.Any(Function(r) r.idActividad = id)
        End Function

        ' TAREASDEV no está en el modelo EF pero tiene FK hacia ACTIVIDADES, así que se consulta por SQL.
        Public Function TieneTareas(id As Integer) As Boolean Implements IActividadRepository.TieneTareas
            Dim cantidad = cntx.Database.SqlQuery(Of Integer)(
                "SELECT COUNT(*) FROM TAREASDEV WHERE Actividad_id = @p0", id).First()
            Return cantidad > 0
        End Function

        Public Sub Agregar(actividad As ACTIVIDADES) Implements IActividadRepository.Agregar
            cntx.ACTIVIDADES.Add(actividad)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(actividad As ACTIVIDADES) Implements IActividadRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(actividad As ACTIVIDADES) Implements IActividadRepository.Eliminar
            cntx.ACTIVIDADES.Remove(actividad)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
