Namespace DTOs.Usuarios
    Public Class UsuarioDTO
        Public Property UserName As String
        Public Property Email As String
        Public Property IsApproved As Boolean
        Public Property Comentario As String
        Public Property Legajo As String
        Public Property Modalidad As Decimal
        Public Property PeriodoAbierto As Int16
        Public Property Centro As String
        Public Property CUIL As String
        Public Property Dependencia As String
        Public Property Roles As List(Of String)
    End Class
End Namespace
