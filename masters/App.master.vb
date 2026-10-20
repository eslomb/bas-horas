
Partial Class masters_App
    Inherits System.Web.UI.MasterPage


    Protected pagina As PaginaApp
    Protected bll As New Logica

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        pagina = CType(Me.Page, PaginaApp)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        l_Usuario.Text = pagina.nombreUsuarioLogueado

        If Not pagina.usuarioEsAdmin Then
            li_administrar.Visible = False

        End If

        If Not (pagina.usuarioEsAdmin) Then
            li_analisis.Visible = False
        End If

        If pagina.usuarioEsBasico Then
            li_usuarios_dependientes.Visible = False
        End If

        li_remuneraciones.Visible=bll.habilitarRemuneraciones(pagina.nombreUsuarioLogueado)

    End Sub


    Protected Sub lb_salir_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lb_salir.Click
        pagina.cerrarSesion()
    End Sub

End Class

