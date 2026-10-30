Namespace DTOs.Usuarios
    ' Periodo abierto de un usuario del equipo de quien edita (ver UsuarioService.ListarEquipo).
    Public Class PeriodoEquipoInputDTO
        Public Property UserName As String
        ' Nullable para que omitirlo sea un 400 y no lo deje en 0 sin querer.
        Public Property PeriodoAbierto As Int16?
    End Class
End Namespace
