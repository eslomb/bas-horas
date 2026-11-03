Namespace Repositories.Interfaces
    Public Interface IAnalisisRepository
        Function EjecutarAnalisisActividades(concepto As String, desde As Date?, hasta As Date?) As List(Of spAnalisisActividades_Result)
    End Interface
End Namespace
