
Partial Class app_reportes
    Inherits PaginaAdmin

    Private esGerente As Boolean

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        agregarRecurso(recursos.Chosen)
        esGerente = nombreUsuarioLogueado.StartsWith("VAZQUEZ")

    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ddl_usuarios.DataSource = bll.listarUsuarios()
            ddl_usuarios.DataBind()
            ddl_usuarios.SelectedValue = nombreUsuarioLogueado
            ddl_usuarios.Visible = usuarioEsAdmin

            'por default para usuario Basicos no se muestran los Usuarios ni los Centros
            Dim colUsuarios As Int16 = 5 - 2
            Dim colCentros As Int16 = 6 - 2

            If usuarioEsAvanzado Or usuarioEsAdmin Then
                gv_listado.Columns(colUsuarios).HeaderStyle.CssClass = "ver"
                gv_listado.Columns(colUsuarios).ItemStyle.CssClass = "ver"
            End If

            If usuarioEsAdmin Or esGerente Then
                gv_listado.Columns(colCentros).HeaderStyle.CssClass = "ver"
                gv_listado.Columns(colCentros).ItemStyle.CssClass = "ver"
            End If
        End If
    End Sub

    Protected Sub CampoUsuario_Load(sender As Object, e As EventArgs)
        Dim tb As HiddenField = CType(sender, HiddenField)
        If usuarioEsAdmin Then
            tb.Value = ddl_usuarios.SelectedValue
        Else
            tb.Value = nombreUsuarioLogueado
        End If

    End Sub

    Protected Sub fv_abm_ItemInserting(sender As Object, e As FormViewInsertEventArgs) Handles fv_abm.ItemInserting
        e.Values.Add("IDRep", Guid.NewGuid.ToString)
        For i As Int16 = 0 To e.Values.Count - 1
            If e.Values(i).ToString = String.Empty Then
                e.Values(i) = Nothing
            End If
        Next
    End Sub

    Protected Function NombreProducto(idprod As Nullable(Of Int32)) As String
        Dim prod As PRODUCTOS = bll.verProducto(IIf(idprod Is Nothing, 0, idprod))
        If prod IsNot Nothing Then
            Return prod.Producto
        Else
            Return "Todos"
        End If
    End Function

    Protected Function NombreActividad(idact As Nullable(Of Int32)) As String
        Dim act As ACTIVIDADES = bll.verActividad(IIf(idact Is Nothing, 0, idact))
        If act IsNot Nothing Then
            Return act.Actividad
        Else
            Return "Todas"
        End If
    End Function

    Protected Function NombrePais(codpais As String) As String
        Dim pais As PAISES = bll.verPais(codpais)
        If pais IsNot Nothing Then
            Return pais.Pais
        Else
            Return "Todos"
        End If
    End Function

    Protected Sub pnl_insertUsuarioReporte_Load(sender As Object, e As EventArgs)
        Dim Pnl As Panel = CType(sender, Panel)
        If usuarioEsAvanzado Or usuarioEsAdmin Then
            Pnl.Visible = True
        End If
    End Sub

    Protected Sub pnl_insertCentroReporte_Load(sender As Object, e As EventArgs)
        Dim Pnl As Panel = CType(sender, Panel)
        If usuarioEsAdmin Then
            Pnl.Visible = True
        End If
    End Sub

    Protected Sub ddl_deCentro_DataBound(sender As Object, e As EventArgs)
        Dim ddl As DropDownList = CType(sender, DropDownList)

        If esGerente Then
            Dim centrosHabilitados As New List(Of String)
            'centrosHabilitados.Add("")
            centrosHabilitados.Add("SIT")
            centrosHabilitados.Add("PV0")
            centrosHabilitados.Add("PV1")
            centrosHabilitados.Add("PV3")
            centrosHabilitados.Add("PV4")

            For Each it As ListItem In ddl.Items
                If centrosHabilitados.Contains(it.Value) Then
                    it.Enabled = True
                Else
                    it.Enabled = False
                End If
            Next
        End If


    End Sub

End Class
