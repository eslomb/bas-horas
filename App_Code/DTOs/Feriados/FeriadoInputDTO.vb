Namespace DTOs.Feriados
    ' En el alta se usa Feriado (clave de la tabla); en la edición la fecha sale de la ruta y se ignora acá.
    Public Class FeriadoInputDTO
        Public Property Feriado As Date?
        Public Property Motivo As String
    End Class
End Namespace
