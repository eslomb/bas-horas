Imports DTOs.Usuarios

Partial Class app_usuarios_dependientes
    Inherits PaginaApp

    Private resultado As String


    Public Property Datos() As List(Of DatosUsuario)
        Get
            Return ViewState("datosUsuario")
        End Get
        Set(ByVal value As List(Of DatosUsuario))
            ViewState("datosUsuario") = value
        End Set
    End Property

    Public Property Legajo() As String
        Get
            Return ViewState("Legajo")
        End Get
        Set(ByVal value As String)
            ViewState("Legajo") = value
        End Set
    End Property


    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        If Me.usuarioEsBasico Then
            Response.Write("No tienes permiso habilitado para ingresar a esta sección")
            Response.End()
        End If
        If Not Me.usuarioEsAdmin Then
            pnl_centros.Visible = False
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            listarUsuariosSuperiores()

            'obtener Legajo de usuario actual
            Dim _profile As ProfileCommon = Profile.GetProfile(nombreUsuarioLogueado)
            If _profile IsNot Nothing Then
                Legajo = _profile.Legajo
            End If
            Try
                ddl_usuarios.SelectedValue = Legajo
            Catch ex As Exception

            End Try

            listarUsuariosDependientes()
        End If
    End Sub

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        If Not String.IsNullOrEmpty(resultado) Then
            l_resultado.Text = String.Format("<div class=""alert alert-success"">{0}</div>", resultado)
        End If
        l_legajo.Text = Legajo
    End Sub

    Private Sub listarUsuariosSuperiores()
        ddl_usuarios.DataSource = usuarios.listarUsuariosSuperiores()
        ddl_usuarios.DataBind()

    End Sub

    Private Sub listarUsuariosDependientes()
        Datos = Me.usuarios.listarUsuariosDependientes(Legajo)
        gvUsuarios.DataSource = Datos
        gvUsuarios.DataBind()
    End Sub


    Private Sub actualizarDatos()
        For Each gvrow As GridViewRow In gvUsuarios.Rows
            If gvrow.RowType = DataControlRowType.DataRow Then
                Dim hf_periodo As HiddenField = gvrow.FindControl("hf_periodo")
                Dim tb_periodo As TextBox = gvrow.FindControl("tb_periodo")
                Dim user As String = Server.HtmlDecode(gvrow.Cells(0).Text)

                If hf_periodo.Value <> tb_periodo.Text Then
                    Dim nuevoPeriodo As Int16 = Int16.Parse(tb_periodo.Text)

                    Dim _profile As ProfileCommon = Profile.GetProfile(user)
                    If _profile Is Nothing Then
                        resultado += "No se pudo actualizar " + user + "<br>"
                    Else

                        _profile.PeriodoAbierto = nuevoPeriodo
                        _profile.Save()
                        hf_periodo.Value = tb_periodo.Text

                        resultado += String.Format("Se actualizó el Período de {0} a {1}<br>", user, nuevoPeriodo)
                    End If
                End If
            End If

        Next
    End Sub


    Protected Sub lb_actualizar_datos_Click(sender As Object, e As EventArgs) Handles lb_actualizar_datos.Click
        actualizarDatos()
    End Sub

    Protected Sub ddl_usuarios_TextChanged(sender As Object, e As EventArgs) Handles ddl_usuarios.TextChanged
        Dim ddl As DropDownList = CType(sender, DropDownList)
        Legajo = ddl.SelectedValue
        listarUsuariosDependientes()
    End Sub
End Class
