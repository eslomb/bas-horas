Namespace DTOs.Usuarios
    Public Class ResultadoMigracionPasswordsDTO
        Public Property Migrados As Integer
        Public Property Fallidos As New List(Of FalloMigracionPasswordDTO)
    End Class

    Public Class FalloMigracionPasswordDTO
        Public Property UserName As String
        Public Property Motivo As String
    End Class
End Namespace
