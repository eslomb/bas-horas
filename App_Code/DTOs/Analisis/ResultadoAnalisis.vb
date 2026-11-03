Namespace DTOs.Analisis
    Public Enum ResultadoOperacionAnalisis
        Ok
        Invalido
    End Enum

    Public Class ResultadoAnalisis
        Public Property Estado As ResultadoOperacionAnalisis
        Public Property Items As List(Of AnalisisActividadDTO)
        Public Property Mensaje As String
    End Class

    Public Class ResultadoArchivoAnalisis
        Public Property Estado As ResultadoOperacionAnalisis
        Public Property Contenido As Byte()
        Public Property NombreArchivo As String
        Public Property Mensaje As String
    End Class
End Namespace
