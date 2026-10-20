Imports System.Linq

Partial Class app_diasHabiles
    Inherits PaginaAdmin

    Protected Enum Modos
        Edicion
        Alta
    End Enum

    Public Property modoActual() As Int16
        Get
            If ViewState Is Nothing Then
                Return Modos.Alta
            End If
            Return ViewState("modoActual")
        End Get
        Set(ByVal value As Int16)
            ViewState("modoActual") = value
        End Set
    End Property

    Private Property EditId As Nullable(Of Int32)
        Get
            If ViewState("EditId") Is Nothing Then
                Return Nothing
            End If
            Return Int32.Parse(ViewState("EditId"))
        End Get
        Set(value As Nullable(Of Int32))
            ViewState("EditId") = value
        End Set
    End Property

    Private entities As BASHorasEntities

#Region "Page_Events"
    Private Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        entities = New BASHorasEntities

        If Not IsPostBack Then
            CargarValores()
            BindGrid()
        End If

    End Sub

    Private Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        Dim modo As String = ""
        Select Case modoActual
            Case Modos.Alta
                modo = "Nuevo Registro"
            Case Modos.Edicion
                modo = "Editando " & NombreMes(ddl_Mes.SelectedValue, tb_anio.Text) & " " & tb_anio.Text
        End Select
        l_modo.Text = modo
        pnl_abm.CssClass = "panel-abm " & (IIf(modoActual = Modos.Edicion, "edicion", ""))

        Dim esModoEdicion As Boolean = (modoActual = Modos.Edicion)

        btn_Cancelar.Visible = esModoEdicion
        tb_anio.Enabled = Not esModoEdicion
        ddl_Mes.Enabled = Not esModoEdicion
    End Sub

#End Region

#Region "Page Methods"
    Private Sub CargarValores()

        Dim hoy As DateTime = DateTime.Today
        Dim inicio As DateTime = (New DateTime(hoy.Year, 1, 1))

        ddl_Mes.Items.Clear()

        For i As Integer = 0 To 11
            Dim fecha As DateTime = inicio.AddMonths(i)
            Dim valor As String = String.Format("{0}", fecha.Month)
            Dim texto As String = fecha.ToString("MMMM", New System.Globalization.CultureInfo("es-AR"))

            Dim li As ListItem = New ListItem(texto, valor)

            li.Selected = (fecha.ToString("MM") = hoy.ToString("MM"))
            ddl_Mes.Items.Add(li)
        Next

        tb_anio.Text = hoy.Year.ToString
    End Sub

    Private Sub BindGrid()

        Dim datos = entities.DIAS_HABILES.OrderByDescending(Function(d) d.Anio) _
                    .ThenByDescending(Function(d) d.Mes) _
                    .ToList()

        gvMeses.DataSource = datos
        gvMeses.DataBind()

        modoActual = Modos.Alta
    End Sub

#End Region

#Region "Control Events"

    Protected Sub btn_Guardar_Click(sender As Object, e As EventArgs) Handles btn_Guardar.Click
        ' -----------------------------------------------
        ' Guardar (alta o edición)
        ' -----------------------------------------------

        ' Validar cantidad de días
        Dim dias As Integer
        If Not Integer.TryParse(txtDias.Text.Trim(), dias) OrElse dias < 1 OrElse dias > 31 Then
            MostrarError("Ingresá una cantidad de días válida (1 a 31).")
            Return
        End If

        If Not IsNumeric(tb_anio.Text) Then
            MostrarError("Ingresá un año válido")
            Return
        End If

        If Convert.ToInt16(tb_anio.Text) < 2000 Then
            MostrarError("Ingresá un año mayor a 2000")
            Return
        End If

        Dim anio As Integer = Integer.Parse(tb_anio.Text)
        Dim mes As Integer = Integer.Parse(ddl_Mes.SelectedValue)

        ' Verificar duplicado
        Dim duplicado As Boolean = entities.DIAS_HABILES.Any(Function(d) d.Anio = anio AndAlso d.Mes = mes AndAlso d.Id <> EditId)

        If duplicado Then
            MostrarError(String.Format("Ya existe un registro para {0} {1}.", ddl_Mes.SelectedItem.Text, tb_anio.Text))
            Return
        End If

        Dim registro As DIAS_HABILES

        If EditId > 0 Then
            registro = entities.DIAS_HABILES.Find(EditId)
            If registro Is Nothing Then Return
        Else
            registro = New DIAS_HABILES
            entities.DIAS_HABILES.Add(registro)
        End If

        registro.Anio = anio
        registro.Mes = mes
        registro.CantDias = dias

        Try
            entities.SaveChanges()
            LimpiarFormulario()
            BindGrid()
        Catch ex As Exception
            MostrarError(ex.Message)
        End Try

    End Sub

    Protected Sub gvMeses_RowEditing(sender As Object, e As GridViewCommandEventArgs) Handles gvMeses.RowCommand

        Dim id As Integer = Convert.ToInt32(e.CommandArgument)

        If e.CommandName = "Editar" Then
            'el valor no puede ser Edit, porque tomaría el evento RowEditing no controlado
            Dim reg = entities.DIAS_HABILES.Find(id)
            If reg Is Nothing Then Return

            EditId = reg.Id
            txtDias.Text = reg.CantDias.ToString()
            ddl_Mes.SelectedValue = reg.Mes
            tb_anio.Text = reg.Anio.ToString

            modoActual = Modos.Edicion

        ElseIf e.CommandName = "Eliminar" Then

            Dim reg = entities.DIAS_HABILES.Find(id)
            If reg Is Nothing Then Return
            entities.DIAS_HABILES.Remove(reg)
            entities.SaveChanges()
            LimpiarFormulario()
            BindGrid()
            'MostrarToast("Registro eliminado.", "info")
        End If
    End Sub

    Private Sub gvMeses_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gvMeses.PageIndexChanging
        Dim gv As GridView = CType(sender, GridView)
        gv.PageIndex = e.NewPageIndex
        BindGrid()
    End Sub

#End Region

#Region "helpers"

    Private Sub LimpiarFormulario()
        EditId = Nothing
        txtDias.Text = ""
    End Sub

    Private Sub MostrarError(msg As String)
        lblError.Text = msg
    End Sub

    Protected Function NombreMes(mes As Object, anio As Object) As String
        ' Usado desde el markup de la grilla
        Return New DateTime(CInt(anio), CInt(mes), 1) _
                .ToString("MMMM", New System.Globalization.CultureInfo("es-AR"))
    End Function

    Private Sub btn_Cancelar_Click(sender As Object, e As EventArgs) Handles btn_Cancelar.Click
        modoActual = Modos.Alta
        LimpiarFormulario()
    End Sub



#End Region

End Class
