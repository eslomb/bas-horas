' Arma RecuperacionPasswordService para las páginas WebForms (login.aspx, recuperaClave.aspx,
' restablecerClave.aspx), que no pasan por el contenedor de Autofac de la API. Mismas piezas que
' registra DependencyConfig. El contexto lo crea y libera la página.
Public Module RecuperacionPasswordPaginas
    Public Function CrearServicio(contexto As BASHorasEntities) As Services.RecuperacionPasswordService
        Return New Services.RecuperacionPasswordService(
            New Repositories.EF.PasswordResetTokenRepository(contexto),
            New Repositories.Membership.UsuarioRepository(),
            New Repositories.Email.NotificadorUsuarios())
    End Function
End Module
