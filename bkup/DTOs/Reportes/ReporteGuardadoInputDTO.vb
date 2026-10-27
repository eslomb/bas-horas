Namespace DTOs.Reportes
    ' No incluye IDRep (lo genera el servidor) ni Usuario (sale del token JWT).
    Public Class ReporteGuardadoInputDTO
        Public Property Titulo As String
        Public Property Desde As Date?
        Public Property Hasta As Date?
        Public Property ActividadId As Integer?
        Public Property ProductoId As Integer?
        Public Property CodPais As String
        Public Property DeUsuario As String
        Public Property DeCentro As String
    End Class
End Namespace
