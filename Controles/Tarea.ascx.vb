
Partial Class Ctrl_tarea
    Inherits System.Web.UI.UserControl

    Protected maxLengthObs As Int16 = 100

#Region "propiedades"

    Public Property IDRegistro() As Int32
        Get
            If ViewState("IDReg") IsNot Nothing Then
                Return Convert.ToInt32(ViewState("IDReg"))
            Else
                Return 0
            End If
        End Get
        Set(ByVal value As Int32)
            ViewState("IDReg") = value
        End Set
    End Property

    Public Property Fecha() As DateTime
        Get
            If tb_fecha.Text <> "" Then
                Return CType(tb_fecha.Text, DateTime)
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As DateTime)
            tb_fecha.Text = value.ToShortDateString
        End Set
    End Property

    Public Property IdActividad() As Int32
        Get
            Dim _id As Int32 = 0
            Int32.TryParse(hf_idActividad.Value, _id)
            Return _id
        End Get
        Set(ByVal value As Int32)
            hf_idActividad.Value = value
        End Set
    End Property

    Public Property Actividad() As String
        Get
            Return tb_actividad.Text
        End Get
        Set(ByVal value As String)
            tb_actividad.Text = value
        End Set
    End Property

    Public Property IdProducto() As Int32
        Get
            Dim _id As Int32 = 0
            Int32.TryParse(hf_idproducto.Value, _id)
            Return _id
        End Get
        Set(value As Int32)
            hf_idproducto.Value = value.ToString
        End Set
    End Property

    Public Property Producto() As String
        Get
            Return tb_producto.Text
        End Get
        Set(ByVal value As String)
            tb_producto.Text = value
        End Set
    End Property

    Public Property CodPais() As String
        Get
            Return ddl_pais.SelectedValue
        End Get
        Set(ByVal value As String)
            If ddl_pais.Items.Count > 0 Then
                ddl_pais.SelectedValue = value
            End If
        End Set
    End Property

    Public ReadOnly Property Pais() As String
        Get
            Return ddl_pais.SelectedItem.Text
        End Get
    End Property

    Public Property Horas() As String
        Get
            Return tb_Horas.Text
        End Get
        Set(ByVal value As String)
            tb_Horas.Text = value
        End Set
    End Property

    Public Property Minutos() As String
        Get
            Return tb_minutos.Text
        End Get
        Set(ByVal value As String)
            tb_minutos.Text = value
        End Set
    End Property

    Public Property Observaciones() As String
        Get
            Return tb_observaciones.Text
        End Get
        Set(ByVal value As String)
            tb_observaciones.Text = value
        End Set
    End Property


    Public WriteOnly Property listadoPaises() As List(Of PAISES)
        Set(ByVal value As List(Of PAISES))
            ddl_pais.DataSource = value
            ddl_pais.DataBind()
            ddl_pais.SelectedIndex = 0
        End Set
    End Property

    Public Property Modificable() As Boolean
        Get
            If ViewState("modificable") IsNot Nothing Then
                Return ViewState("modificable")
            Else
                Return True
            End If
        End Get
        Set(ByVal value As Boolean)
            ViewState("modificable") = value
        End Set
    End Property

    Public Resultado As String
    Public Estado As Int16 ' 0=Inicial, 1=OK, 2=Error, 3=Advertencia

#End Region


    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not IsPostBack And tb_fecha.Text = "" Then
            Fecha = Date.Now.ToShortDateString
        End If
        Estado = 0
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        For Each li As ListItem In ddl_pais.Items
            If li.Value = CodPais Then
                li.Selected = True
                Exit For
            End If
        Next

    End Sub


    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        filaDatos.Attributes.Remove("class")
        

        Select Case Estado
            Case 1
                filaDatos.Attributes.Add("class", "alert alert-success")
            Case 2
                filaDatos.Attributes.Add("class", "alert alert-danger")
            Case 3
                filaDatos.Attributes.Add("class", "alert alert-warning")
        End Select

        If Estado > 0 Then
            pnl_resultado.Visible = True
            l_resultado.Text = Resultado
        End If

        tb_fecha.Enabled = Modificable

        tb_observaciones.Attributes.Add("maxlength", maxLengthObs.ToString)
    End Sub

    Public Sub limpiarDatos()
        Fecha = Date.Now.ToShortDateString
        IdActividad = 0
        Actividad = String.Empty
        IdProducto = 0
        Producto = String.Empty
        ddl_pais.SelectedIndex = 0
        Horas = "0"
        Minutos = "0"
        Observaciones = String.Empty
        Estado = 0
        Resultado = String.Empty
    End Sub

    Protected Sub lb_limpiar_Click(sender As Object, e As EventArgs) Handles lb_limpiar.Click
        limpiarDatos()
    End Sub
End Class
