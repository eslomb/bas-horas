Namespace DTOs.Configuraciones
    Public Enum ResultadoOperacionConfiguracion
        Ok
        NoEncontrado
        Invalido
        Fallo
    End Enum

    Public Class ResultadoConfiguracion
        Public Property Estado As ResultadoOperacionConfiguracion
        Public Property Configuracion As ConfiguracionDTO
        Public Property Mensaje As String
    End Class
End Namespace
