Namespace DTOs.Analisis
    ' Horas cargadas de una actividad para un concepto (producto, país o usuario) en el período.
    Public Class AnalisisActividadDTO
        ' IDPro, código de país o nombre de usuario, según el concepto.
        Public Property Id As String
        Public Property Concepto As String
        Public Property CodActividad As String
        Public Property Horas As Integer
        Public Property Minutos As Integer
    End Class
End Namespace
