
Partial Class app_Productos
    Inherits PaginaAdmin

    Private _FPIniEditable As Boolean
    Public ReadOnly Property FPIniEditable() As Boolean
        Get
            Dim fpi As Boolean = ViewState("FPIniEditable")
            If fpi = Nothing Then
                'fpi = bll.perimitirEdicionFProyIni()
            End If
            Return bll.perimitirEdicionFProyIni()
        End Get
    End Property

    Protected Sub Page_Init1(sender As Object, e As EventArgs) Handles Me.Init

    End Sub


End Class
