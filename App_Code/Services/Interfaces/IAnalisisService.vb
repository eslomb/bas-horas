Imports DTOs.Analisis

Namespace Services.Interfaces
    Public Interface IAnalisisService
        Function AnalizarActividades(concepto As String, desde As Date?, hasta As Date?) As ResultadoAnalisis
        Function ExportarActividades(concepto As String, desde As Date?, hasta As Date?, ids As String()) As ResultadoArchivoAnalisis
    End Interface
End Namespace
