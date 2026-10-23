Namespace DTOs.Actividades
    ' El IDAct es identity: en el alta lo genera la base y en la edición sale de la ruta.
    Public Class ActividadInputDTO
        Public Property Codigo As String
        Public Property Actividad As String
        Public Property Habilitada As Boolean?
    End Class
End Namespace
