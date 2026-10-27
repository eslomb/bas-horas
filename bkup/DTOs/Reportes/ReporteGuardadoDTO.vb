Namespace DTOs.Reportes
    Public Class ReporteGuardadoDTO
        Public Property IDRep As Guid
        Public Property Titulo As String
        Public Property Usuario As String
        Public Property Desde As Date?
        Public Property Hasta As Date?
        Public Property ActividadId As Integer?
        Public Property ProductoId As Integer?
        Public Property CodPais As String
        Public Property DeUsuario As String
        Public Property DeCentro As String
    End Class
End Namespace
