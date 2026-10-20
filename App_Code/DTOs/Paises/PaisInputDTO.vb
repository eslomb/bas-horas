Namespace DTOs.Paises
    ' En el alta se usa Codigo (clave de la tabla, nchar(4)); en la edición el código sale de la ruta y se ignora acá.
    Public Class PaisInputDTO
        Public Property Codigo As String
        Public Property Pais As String
        Public Property Preferido As Boolean?
    End Class
End Namespace
