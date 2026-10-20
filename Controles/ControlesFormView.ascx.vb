
Partial Class Controles_ControlesFormView
    Inherits System.Web.UI.UserControl

    Private _modo As String
    Public Event Cancel_Click As System.EventHandler

    Public Property Modo() As String
        Get
            Return _modo
        End Get
        Set(ByVal value As String)
            _modo = value
        End Set
    End Property


    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        If Modo = "Update" Then
            UpdateButton.Visible = True
        End If
        If Modo = "Insert" Then
            InsertButton.Visible = True
        End If
    End Sub

    Protected Sub CancelButton_Click(sender As Object, e As EventArgs) Handles CancelButton.Click
        Dim p As PaginaAdmin = CType(Me.Parent.Parent.Parent.Page, PaginaAdmin)
        p.mostrarListado()
        RaiseEvent Cancel_Click(sender, e)
    End Sub
End Class
