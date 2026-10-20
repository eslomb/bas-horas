Partial Class app_usuarios
    Inherits PaginaAdmin

    Private totPorGrupo As Int32

    Public Property dtUsuarios() As DataTable
        Get
            Return ViewState("dtUsuarios")
        End Get
        Set(ByVal value As DataTable)
            ViewState("dtUsuarios") = value
        End Set
    End Property

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        totPorGrupo = 0
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Dim _roles() As String = Roles.GetAllRoles
            rbl_grupo.DataSource = _roles
            rbl_grupo.DataBind()
            'rbl_grupo.SelectedValue = Grupos.Administrador
            rbl_grupo.SelectedIndex = 0
            listarUsuarios()
        End If
    End Sub

    Private Sub listarUsuarios()
        Dim dt As DataTable = New DataTable
        Dim dr As DataRow
        Dim usuariosEnRol As New ArrayList

        If rbl_grupo.SelectedValue <> "" Then
            Dim u() As String = Roles.GetUsersInRole(rbl_grupo.SelectedValue)
            For Each usr As String In u
                usuariosEnRol.Add(usr)
            Next
        Else
            Dim usuarios As MembershipUserCollection = Membership.GetAllUsers()
            For Each usr As MembershipUser In usuarios
                usuariosEnRol.Add(usr.UserName)
            Next
        End If


        'dt.Columns.Add("Num")
        dt.Columns.Add("Nombre")
        dt.Columns.Add("Email")
        dt.Columns.Add("Legajo")
        dt.Columns.Add("Centro")
        dt.Columns.Add("Modalidad")
        dt.Columns.Add("Periodo")
        dt.Columns.Add("CUIL")
        dt.Columns.Add("Clave")
        dt.Columns.Add("Comentarios")
        dt.Columns.Add("Ver")
        dt.Columns.Add("Habilitado")
        dt.Columns.Add("Dependencia")
        Dim us As MembershipUser
        For Each str_user As String In usuariosEnRol
            us = Membership.GetUser(str_user)
            If us IsNot Nothing Then
                totPorGrupo += 1

                Dim _profile As ProfileCommon = Profile.GetProfile(us.UserName)
                dr = dt.NewRow

                'dr(0) = totPorGrupo.ToString
                dr("Nombre") = str_user
                dr("Email") = us.Email
                dr("Legajo") = _profile.Legajo
                dr("Centro") = _profile.Centro
                dr("Modalidad") = _profile.Modalidad
                If Not us.IsLockedOut Then
                    dr("Clave") = "*****" 'us.GetPassword
                Else
                    dr("Clave") = "BLOQUEADO"
                End If
                dr("Periodo") = _profile.PeriodoAbierto
                dr("CUIL") = _profile.CUIL
                dr("Comentarios") = us.Comment
                dr("Ver") = dr("Email")
                dr("Habilitado") = IIf(us.IsApproved, "SI", "NO")
                dr("Dependencia") = _profile.Dependencia
                dt.Rows.Add(dr)

            End If
        Next

        dtUsuarios = dt
        gvUsuarios.DataSource = dtUsuarios
        gvUsuarios.DataBind()

        'anulado porque el resultado se muestra en el plugin datatables
        'If totPorGrupo > 1 Then
        '    l_totalPorGrupo.Text = String.Format("Hay {0} usuarios en el grupo.", totPorGrupo.ToString)
        'ElseIf totPorGrupo = 1 Then
        '    l_totalPorGrupo.Text = "Hay 1 usuario en el grupo"
        'Else
        '    l_totalPorGrupo.Text = "No se encontraron usuarios en este grupo."
        'End If
        If totPorGrupo = 0 Then
            l_totalPorGrupo.Text = "No se encontraron usuarios en este grupo."
        End If
    End Sub

    Protected Sub rbl_grupo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles rbl_grupo.SelectedIndexChanged
        listarUsuarios()
    End Sub

    Protected Sub gvUsuarios_Init(sender As Object, e As EventArgs) Handles gvUsuarios.Init
        gvUsuarios.EnableViewState = True
    End Sub

    Protected Sub gvUsuarios_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gvUsuarios.PageIndexChanging
        Dim gv As GridView = CType(sender, GridView)
        gv.PageIndex = e.NewPageIndex
        gv.DataSource = dtUsuarios
        gv.DataBind()
    End Sub

    Protected Sub gvUsuarios_Sorting(sender As Object, e As GridViewSortEventArgs) Handles gvUsuarios.Sorting
        Dim gv As GridView = CType(sender, GridView)
        'gv.Sort(e.SortExpression & " " & GetSortingDirection(e.SortExpression))
    End Sub

    Private Function GetSortingDirection(ByVal column As String) As String
        ' By default, set the sort direction to ascending.
        Dim sortDirection = "ASC"
        ' Retrieve the last column that was sorted.
        Dim sortExpression = TryCast(ViewState("SortExpression"), String)
        If sortExpression IsNot Nothing Then
            ' Check if the same column is being sorted.
            ' Otherwise, the default value can be returned.
            If sortExpression = column Then
                Dim lastDirection = TryCast(ViewState("SortDirection"), String)
                If lastDirection IsNot Nothing _
                  AndAlso lastDirection = "ASC" Then
                    sortDirection = "DESC"
                End If
            End If
        End If
        ' Save new values in ViewState.
        ViewState("SortDirection") = sortDirection
        ViewState("SortExpression") = column
        Return sortDirection
    End Function

    Public Overrides Sub Exportar()
        gvUsuarios.AllowPaging = False
        gvUsuarios.PageSize = 8000
        'gvUsuarios.DataBind()
        ExportToExcel(gvUsuarios, lb_exportar_ctrl.CommandArgument)
    End Sub

    Protected Sub lb_eliminarUsuario_Click(sender As Object, e As EventArgs)
        Dim lb As LinkButton = CType(sender, LinkButton)
        eliminarUsuario(lb.CommandArgument)

    End Sub

    Private Sub eliminarUsuario(nombre As String)
        'elimina definitivamente el usuario
        Dim usuario As MembershipUser = Membership.GetUser(nombre)
        If usuario IsNot Nothing Then
            Dim rolesDeUsuario() As String = Roles.GetRolesForUser(usuario.UserName)
            For Each rol As String In rolesDeUsuario
                Roles.RemoveUserFromRole(usuario.UserName, rol)
            Next
            Try
                Membership.DeleteUser(usuario.UserName, True)
                notificacion = "El usuario ha sido eliminado."
                tipoNotificacion = TiposNotificacion.OK
                listarUsuarios()
            Catch ex As ArgumentNullException
                notificacion = "No se puede eliminar este usuario: username es nulo."
                tipoNotificacion = TiposNotificacion.Alerta
            Catch ex As ArgumentException
                notificacion = "No se puede eliminar este usuario: " + ex.Message
                tipoNotificacion = TiposNotificacion.Alerta
            Catch ex As Exception
                notificacion = "ERROR: " + ex.Message
                tipoNotificacion = TiposNotificacion.Alerta
            End Try
        Else
            notificacion = "El usuario no existe."
            tipoNotificacion = TiposNotificacion.Advertencia
        End If
    End Sub
End Class
