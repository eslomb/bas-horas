Namespace DTOs.Remuneraciones
    ' Alta. Nullable para que un campo omitido sea un 400 y no un 0 silencioso.
    Public Class RemuneracionInputDTO
        Public Property Usuario As String
        Public Property Anio As Integer?
        Public Property Mes As Integer?
        Public Property Remuneracion As Decimal?
    End Class

    ' Edición: solo el monto. Usuario, año y mes son la identidad de la fila y no se modifican.
    Public Class ActualizarRemuneracionInputDTO
        Public Property Remuneracion As Decimal?
    End Class
End Namespace
