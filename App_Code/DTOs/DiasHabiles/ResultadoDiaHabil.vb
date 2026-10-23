Namespace DTOs.DiasHabiles
    Public Enum ResultadoOperacionDiaHabil
        Ok
        NoEncontrado
        Duplicado
        Invalido
        Fallo
    End Enum

    Public Class ResultadoDiaHabil
        Public Property Estado As ResultadoOperacionDiaHabil
        Public Property DiaHabil As DiaHabilDTO
        Public Property Mensaje As String
    End Class
End Namespace
