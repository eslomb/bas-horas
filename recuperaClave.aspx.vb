
Partial Class recuperaClave
    Inherits PaginaAnonimos

    ' Con passwordFormat=Hashed ya no se puede mandar la clave por mail: se manda un link de un solo uso a
    ' restablecerClave.aspx (misma lógica que POST api/auth/recuperar-password). Con EnviarMailsHabilitado=false
    ' el link queda en el log de desarrollo en vez de enviarse.
    ' La respuesta es siempre la misma, exista o no el email, para no revelar qué cuentas existen.
    Protected Sub btn_Enviar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_Enviar.Click
        If Not Page.IsValid Then Return

        Try
            Using contexto As New BASHorasEntities()
                RecuperacionPasswordPaginas.CrearServicio(contexto).Solicitar(tb_email.Text.Trim())
            End Using
        Catch
            pnl_error.Visible = True
            Return
        End Try

        l_minutos.Text = Services.RecuperacionPasswordService.MinutosValidez().ToString()

        pnl_formulario.Visible = False
        pnl_enviado.Visible = True
    End Sub
End Class
