Namespace Repositories.Interfaces
    Public Interface IReporteGeneralRepository
        Function EjecutarReporteGeneral(desde As Date?, hasta As Date?, usuario As String, idProd As Integer?, idAct As Integer?, codPais As String) As List(Of spReporteGeneral_Result)
    End Interface
End Namespace
