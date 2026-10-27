Namespace DTOs.Registros
    ' No incluye Usuario ni IDReg: el Usuario se toma del token JWT y el IDReg lo genera la base.
    Public Class RegistroInputDTO
        Public Property IdProducto As Integer
        Public Property IdActividad As Integer
        Public Property Fecha As Date
        Public Property Horas As Integer
        Public Property Minutos As Integer
        Public Property CodPais As String
        Public Property Observaciones As String
    End Class
End Namespace
