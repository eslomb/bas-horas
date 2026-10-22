Namespace DTOs.Centros
    Public Enum ResultadoOperacionCentro
        Ok
        NoEncontrado
        Duplicado
        EnUso
        Invalido
        Fallo
    End Enum

    Public Class ResultadoCentro
        Public Property Estado As ResultadoOperacionCentro
        Public Property Centro As CentroDTO
        Public Property Mensaje As String
    End Class
End Namespace
