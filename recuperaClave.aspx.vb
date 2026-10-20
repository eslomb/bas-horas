
Partial Class recuperaClave
    Inherits PaginaAnonimos


    Private Sub verificar()
        Dim nombre As String = Membership.GetUserNameByEmail(tb_email.Text.Trim)
        Dim pass As String = String.Empty


        If Not String.IsNullOrEmpty(nombre) Then
            Dim usuario As MembershipUser = Membership.GetUser(nombre)
            If usuario.IsApproved Then
                If Not usuario.IsLockedOut Then
                    'envio mail con los datos
                    pass = usuario.GetPassword
                    enviarClave(usuario.Email, pass)

                    'l_resultado.Text = "email: " + usuario.Email + ", clave: " + pass
                    mv_respuestas.SetActiveView(v_claveGenerada)

                Else
                    mv_respuestas.SetActiveView(v_cuentaBloqueada)
                End If
            Else
                mv_respuestas.SetActiveView(v_usuarioInactivo)
            End If

        Else
            mv_respuestas.SetActiveView(v_EmailNoExiste)
        End If

    End Sub

    Private Sub enviarClave(ByVal pEmail As String, ByVal pClave As String)
        Dim c As Correo = New Correo()
        Dim msg As String = ""
        Dim err As String = ""

        c.Asunto = "Recuperación de contraseña"
        c.De = "BASHoras <respuestaautomatica@bas.com.ar>"
        c.Para = pEmail

        msg += "<h4>Mensaje de BASHoras</h4><br>"
        msg += "La clave de el acceso es: " + pClave + "<br><br><br>"
        msg += "No respondas este mensaje ya que ha sido generado automáticamente. Si crees que lo recibiste por error, por favor ignoralo.<hr>"

        c.Mensaje = msg

        c.enviar()
    End Sub

    Protected Sub btn_Enviar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_Enviar.Click
        If Page.IsValid Then
            verificar()
        End If
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Not mv_respuestas.GetActiveView Is Nothing Then
            pnl_resultado.Visible = True

        End If

    End Sub
End Class
