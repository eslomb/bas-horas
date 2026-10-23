
Partial Class login
    Inherits PaginaAnonimos

    Private resultado As String
    Private tb_usr As TextBox
    Private tb_pwd As TextBox
    Private cb_Remember As CheckBox
    Private nombreDeCookie As String
    Private lb_norecordar As LinkButton
    Private usr As MembershipUser = Nothing

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        resultado = String.Empty
        nombreDeCookie = "BASHoras"
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Request.QueryString("logout") = "1" Then
            'FormsAuthentication.SignOut()   ' o tu método de PaginaApp
            ' Session.Abandon()
            Dim pagina As PaginaAdmin = new PaginaAdmin()
            pagina.cerrarSesion()
        End If
        
        tb_usr = Login2.FindControl("UserName")
        tb_pwd = Login2.FindControl("Password")
        cb_Remember = Login2.FindControl("RememberMe")
        lb_norecordar = Login2.FindControl("lb_norecordar")

        If Not Page.IsPostBack Then
            Dim nuevaCookie As HttpCookie = Request.Cookies(nombreDeCookie)
            If Request.Cookies(nombreDeCookie) IsNot Nothing Then
                tb_usr.Text = nuevaCookie("usr")
                tb_pwd.Attributes.Add("value", nuevaCookie("pwd"))
                Login2.RememberMeSet = True
                lb_norecordar.Visible = True
            End If
            tb_usr.Focus()

        End If
    End Sub

    Protected Sub btn_ingresar_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim usrName As String = Membership.GetUserNameByEmail(tb_usr.Text)

        If Not String.IsNullOrEmpty(usrName) Then
            usr = Membership.GetUser(usrName)
            If Membership.ValidateUser(usr.UserName, tb_pwd.Text) Then
                ingresar()
            Else
                resultado = "No se encontró una cuenta con los datos ingresados. <br>Por favor, verificá los mismos e intentá nuevamente."

                If Not usr Is Nothing Then
                    If usr.IsLockedOut Then
                        resultado = "<strong>Esta cuenta ha sido bloqueada</strong>."
                        'informarBloqueo()
                    End If
                End If
            End If
        Else
            resultado = "No se encontró una cuenta con los datos ingresados. <br>Por favor, verificá los mismos e intentá nuevamente."
        End If
    End Sub

    Private Sub ingresar()
        FormsAuthentication.SetAuthCookie(usr.UserName, False)
        modificarCookie()
        Dim qsReturnPath As String = String.Empty
        If Not String.IsNullOrEmpty(Request.QueryString("ReturnUrl")) Then
            qsReturnPath = "?redir=" + Server.UrlEncode(Request.QueryString("ReturnUrl"))
        End If
        Response.Redirect("app/redirector.aspx" + qsReturnPath)
    End Sub

    Private Sub modificarCookie()
        Dim nuevaCookie As HttpCookie = New HttpCookie(nombreDeCookie)
        If cb_Remember.Checked Then
            nuevaCookie("usr") = tb_usr.Text
            nuevaCookie("pwd") = tb_pwd.Text
            nuevaCookie.Expires = Date.Now.AddDays(30)
            Response.Cookies.Add(nuevaCookie)
        Else
            eliminarCookie()
        End If
    End Sub

    Private Sub informarBloqueo()
        'se informa a atencionalcliente del evento.
        Dim c As Correo = New Correo()
        c.Asunto = "Usuario " + usr.UserName.ToString + " bloqueado en Soporte"
        c.De = "respuestaautomatica@bas.com.ar"
        c.Mensaje = "El usuario " + usr.UserName.ToString + " ha intentado loguearse pero su cuenta fue bloqueada por realizar reiterados intentos fallidos de acceso. Para desbloquearlo se deberá ingresar al administrador (opción Datos de usuarios) e ingresar el código de cliente."
        c.enviar()
        'resultado = c.Mensaje
    End Sub

    Private Sub eliminarCookie()
        Dim nuevaCookie As HttpCookie = New HttpCookie(nombreDeCookie)
        If Request.Cookies(nombreDeCookie) IsNot Nothing Then
            nuevaCookie.Expires = DateTime.Now.AddDays(-1D)
            Response.Cookies.Add(nuevaCookie)
        End If
    End Sub

    Protected Sub lb_norecordarOnClick(ByVal sender As Object, ByVal e As EventArgs)
        eliminarCookie()
        tb_usr.Text = ""
        tb_pwd.Attributes.Add("value", "")
        cb_Remember.Checked = False
        lb_norecordar.Visible = False
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        l_Resultado.Text = resultado
        If resultado <> "" Then
            pnl_resultado.Visible = True
        End If
        'agregarRecurso(recursos.MagnificPopup)
    End Sub

End Class
