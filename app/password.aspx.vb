
Partial Class app_password
    Inherits PaginaApp


    Private usr As MembershipUser

    Protected Sub btnAceptar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAceptar.Click
        If Page.IsValid Then
            cambiarClave()
        Else
            notificacion = "Los datos ingresados no son correctos. Verificalos e intentá nuevamente."
            tipoNotificacion = TiposNotificacion.Advertencia
        End If
    End Sub

    ' Con passwordFormat=Hashed no hay GetPassword: ChangePassword ya verifica la clave actual
    ' (devuelve False si no coincide).
    Private Sub cambiarClave()
        Try
            usr = Membership.GetUser
            If Not usr.ChangePassword(tb_claveActual.Text, tb_claveNueva.Text) Then
                notificacion = "La clave ingresada no coincide con nuestros registros.<br />Recordar que se diferencia entre mayúsculas y minúsculas."
                tipoNotificacion = TiposNotificacion.Advertencia
                Return
            End If

            Profile.PasswordMigrada = True
            Profile.Save()

            notificacion = "La contraseña ha sido actualizada y podrás utilizarla la próxima vez que ingreses al sitio."
            tipoNotificacion = TiposNotificacion.OK
        Catch ex As Exception When TypeOf ex Is MembershipPasswordException OrElse TypeOf ex Is ArgumentException
            ' SqlMembershipProvider tira ArgumentException si la nueva es más corta que minRequiredPasswordLength.
            notificacion = String.Format("La nueva contraseña no cumple con la política requerida (mínimo {0} caracteres).", Membership.MinRequiredPasswordLength)
            tipoNotificacion = TiposNotificacion.Advertencia
        Catch ex2 As Exception
            notificacion = "Ocurrió un error al cambiar la contraseña. " + ex2.Message
            tipoNotificacion = TiposNotificacion.Alerta
        End Try
    End Sub
End Class
