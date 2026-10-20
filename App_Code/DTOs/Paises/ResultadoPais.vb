Namespace DTOs.Paises
    Public Enum ResultadoOperacionPais
        Ok
        NoEncontrado
        Duplicado
        EnUso
        EsPreferido
        Invalido
        Fallo
    End Enum

    Public Class ResultadoPais
        Public Property Estado As ResultadoOperacionPais
        Public Property Pais As PaisDTO
        Public Property Mensaje As String
    End Class
End Namespace
