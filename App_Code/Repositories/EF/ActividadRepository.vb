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
    End Class
End Namespace
