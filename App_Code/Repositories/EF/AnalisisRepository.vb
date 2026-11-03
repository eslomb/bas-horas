Imports System.Globalization
Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class AnalisisRepository
        Implements IAnalisisRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        ' El SP recibe las fechas como texto dd/mm/yyyy (convert estilo 103).
        Public Function EjecutarAnalisisActividades(concepto As String, desde As Date?, hasta As Date?) As List(Of spAnalisisActividades_Result) Implements IAnalisisRepository.EjecutarAnalisisActividades
            Return cntx.spAnalisisActividades(concepto, FechaSP(desde), FechaSP(hasta)).ToList()
        End Function

        Private Shared Function FechaSP(fecha As Date?) As String
            Return If(fecha.HasValue, fecha.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), Nothing)
        End Function
    End Class
End Namespace
