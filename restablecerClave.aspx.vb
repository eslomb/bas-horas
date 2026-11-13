Imports DTOs.Usuarios

' Destino del link de recuperación (appSetting UrlRestablecerPassword = ~/restablecerClave.aspx) y del
' cambio obligatorio de claves cortas de login.aspx. Misma lógica que POST api/auth/restablecer-password:
' token de un solo uso, desbloquea la cuenta y marca PasswordMigrada.
' El token viaja en ?token= y se mantiene en los postbacks (el form postea a la misma URL).
Partial Class restablecerClave
    Inherits PaginaAnonimos

    Private ReadOnly Property Token As String
        Get
            Return Request.QueryString("token")
        End Get
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        l_minimo.Text = MinimoPassword().ToString()

        If Not Page.IsPostBack Then
            Using contexto As New BASHorasEntities()
                If Not RecuperacionPasswordPaginas.CrearServicio(contexto).Validar(Token) Then
                    mv_estado.SetActiveView(v_tokenInvalido)
                    Return
                End If
            End Using
            tb_nueva.Focus()
        End If
    End Sub

    Protected Sub btn_Guardar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_Guardar.Click
        If Not Page.IsValid Then Return

        Dim resultado As ResultadoRestablecerPassword
        Try
            Using contexto As New BASHorasEntities()
                resultado = RecuperacionPasswordPaginas.CrearServicio(contexto).Restablecer(Token, tb_nueva.Text)
            End Using
        Catch
            resultado = ResultadoRestablecerPassword.Fallo
        End Try

        Select Case resultado
            Case ResultadoRestablecerPassword.Ok
                mv_estado.SetActiveView(v_listo)
            Case ResultadoRestablecerPassword.TokenInvalido
                mv_estado.SetActiveView(v_tokenInvalido)
            Case ResultadoRestablecerPassword.PasswordNoValida
                mostrarError("La contraseña no cumple los requisitos: tiene que tener al menos " & MinimoPassword() & " caracteres.")
            Case Else
                mostrarError("No se pudo cambiar la contraseña. Intentá nuevamente más tarde.")
        End Select
    End Sub

    Private Sub mostrarError(ByVal mensaje As String)
        l_error.Text = Server.HtmlEncode(mensaje)
        pnl_error.Visible = True
    End Sub

    Private Shared Function MinimoPassword() As Integer
        Return Membership.MinRequiredPasswordLength
    End Function
End Class
