
Partial Class app_configuraciones
    Inherits PaginaApp

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            cargarConfiguraciones()
        End If

    End Sub

    Private Sub cargarConfiguraciones()
        rbl_permitir.SelectedValue = bll.permitirCargas
        tb_periodoCarga.Text = bll.periodoCargas().ToString
        rbl_FPIni.SelectedValue = bll.perimitirEdicionFProyIni
        tb_activ_Observacion.Text = bll.OpcionConfig("ACTIV_CON_OBSERV").Valor
    End Sub

    Protected Sub lb_actualizar_periodo_Click(sender As Object, e As EventArgs) Handles lb_actualizar_periodo.Click
        If Page.IsValid Then
            notificacion = bll.actualizarConfiguracion("PERIODO_CARGAS", tb_periodoCarga.Text)
        End If
    End Sub

    Protected Sub lb_habilitar_cargas_Click(sender As Object, e As EventArgs) Handles lb_habilitar_cargas.Click
        notificacion = bll.actualizarConfiguracion("PERMITIR_CARGAS", rbl_permitir.SelectedValue)
    End Sub

    Protected Sub lb_habilitar_FPIni_Click(sender As Object, e As EventArgs) Handles lb_habilitar_FPIni.Click
        notificacion = bll.actualizarConfiguracion("PERMITIR_EDI_FP_INI", rbl_FPIni.SelectedValue)
    End Sub

    Private Sub lb_activ_Observacion_Click(sender As Object, e As EventArgs) Handles lb_activ_Observacion.Click
        notificacion = bll.actualizarConfiguracion("ACTIV_CON_OBSERV", tb_activ_Observacion.Text)
    End Sub
End Class
