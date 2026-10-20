
Partial Class app_password
    Inherits PaginaApp


    Private usr As MembershipUser

    Protected Sub btnAceptar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAceptar.Click
        If Page.IsValid Then
            If Not verificarClave() Then
                notificacion = "La clave ingresada no coincide con nuestros registros.<br />Recordar que se diferencia entre mayúsculas y minúsculas."
                tipoNotificacion = TiposNotificacion.Advertencia
            Else
                cambiarClave()
            End If
        Else
            notificacion = "Los datos ingresados no son correctos. Verificalos e intentá nuevamente."
            tipoNotificacion = TiposNotificacion.Advertencia
        End If
    End Sub

    Private Sub cambiarClave()
        Try
            usr.ChangePassword(tb_claveActual.Text, tb_claveNueva.Text)
            notificacion = "La contraseña ha sido actualizada y podrás utilizarla la próxima vez que ingreses al sitio."
            tipoNotificacion = TiposNotificacion.OK
        Catch ex As MembershipPasswordException
            notificacion = "No se puede leer la contraseña actual. " + ex.Message
            tipoNotificacion = TiposNotificacion.Alerta
        Catch ex2 As Exception
            notificacion = "Ocurrió un error al cambiar la contraseña. " + ex2.Message
            tipoNotificacion = TiposNotificacion.Alerta
        End Try
    End Sub

    Private Function verificarClave() As Boolean
        usr = Membership.GetUser
        If usr.GetPassword.Equals(tb_claveActual.Text.Trim) Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
