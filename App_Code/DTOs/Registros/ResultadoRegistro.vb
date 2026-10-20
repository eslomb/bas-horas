Namespace DTOs.Registros
    Public Enum ResultadoOperacion
        Ok
        NoEncontrado
        Prohibido
        Duplicado
        Fallo
    End Enum

    Public Class ResultadoRegistro
        Public Property Estado As ResultadoOperacion
        Public Property Registro As RegistroDTO
        Public Property Mensaje As String
    End Class
End Namespace
