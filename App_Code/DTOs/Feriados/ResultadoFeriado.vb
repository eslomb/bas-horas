Namespace DTOs.Feriados
    Public Enum ResultadoOperacionFeriado
        Ok
        NoEncontrado
        Duplicado
        Invalido
        Fallo
    End Enum

    Public Class ResultadoFeriado
        Public Property Estado As ResultadoOperacionFeriado
        Public Property Feriado As FeriadoDTO
        Public Property Mensaje As String
    End Class
End Namespace
