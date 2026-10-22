Namespace DTOs.Actividades
    Public Enum ResultadoOperacionActividad
        Ok
        NoEncontrado
        Duplicado
        EnUso
        Invalido
        Fallo
    End Enum

    Public Class ResultadoActividad
        Public Property Estado As ResultadoOperacionActividad
        Public Property Actividad As ActividadDTO
        Public Property Mensaje As String
    End Class
End Namespace
