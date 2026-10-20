Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class ReporteGeneralRepository
        Implements IReporteGeneralRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function EjecutarReporteGeneral(desde As Date?, hasta As Date?, usuario As String, idProd As Integer?, idAct As Integer?, codPais As String) As List(Of spReporteGeneral_Result) Implements IReporteGeneralRepository.EjecutarReporteGeneral
            Return cntx.spReporteGeneral(desde, hasta, usuario, idProd, idAct, codPais).ToList()
        End Function
    End Class
End Namespace
