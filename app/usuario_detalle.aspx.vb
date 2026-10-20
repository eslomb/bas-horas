
Partial Class app_usuario_detalle
    Inherits PaginaApp


    Private usuario As MembershipUser 'usuario cuyo codigo se obtiene segun el mail ingresado por qs
    Private email As String


    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init

        email = HttpUtility.UrlDecode(Request.QueryString("email"))
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
       
        usuario = BuscarUsuario(email)

        If usuario Is Nothing Then
            'notificacion = "El usuario no existe."
            'tipoNotificacion = TiposNotificacion.Advertencia
            'multiview.ActiveViewIndex = -1
            multiview.SetActiveView(v_alta)
        End If

        If Not IsPostBack Then
            Dim _roles() As String = Roles.GetAllRoles()
            cbl_edicion_roles.DataSource = _roles
            cbl_edicion_roles.DataBind()
            cbl_alta_roles.DataSource = _roles
            cbl_alta_roles.DataBind()

            Dim cntx As New BASHorasEntities
            Dim centros = (From c In cntx.CENTROS
                           Order By c.Nombre
                          Select c).ToList

            ddl_edicion_centro.DataSource = centros
            ddl_edicion_centro.DataBind()
            ddl_alta_centro.DataSource = centros
            ddl_alta_centro.DataBind()

            buscarDatos()
        End If

        lb_alta_cancelar.Visible = Not String.IsNullOrEmpty(email)
    End Sub


    Private Function BuscarUsuario(ByVal pEmail As String) As MembershipUser
        Dim userEmail As String = Membership.GetUserNameByEmail(pEmail)
        Dim us As MembershipUser = Nothing
        If Not String.IsNullOrEmpty(userEmail) Then
            us = Membership.GetUser(userEmail)
        End If
        Return us
    End Function

    Private Sub buscarDatos()
        If usuario IsNot Nothing Then

            tb_edicion_nombre.Text = usuario.UserName
            Dim _pwd As TextBox = multiview.Views(1).FindControl("tb_edicion_clave")

            lb_edicion_actualizar.Visible = True
            If usuario.IsLockedOut Then
                'tb_edicion_clave.Text = "[usuario bloqueado]"
                lb_edicion_actualizar.Visible = False
                tb_edicion_clave.Enabled = False
                notificacion = "El usuario está bloqueado"
                tipoNotificacion = TiposNotificacion.Advertencia
            Else
                'tb_edicion_clave.Text = usuario.GetPassword()
            End If
            btn_desbloquear.Visible = usuario.IsLockedOut

            tb_edicion_email.Text = usuario.Email
            If Not String.IsNullOrEmpty(usuario.Comment) Then
                tb_edicion_comentarios.Text = usuario.Comment
            Else
                tb_edicion_comentarios.Text = ""
            End If
            For Each it As ListItem In cbl_edicion_roles.Items
                If Roles.IsUserInRole(usuario.UserName, it.Text) Then
                    it.Selected = True
                End If
            Next
            Dim _profile As ProfileCommon = Profile.GetProfile(usuario.UserName)
            tb_edicion_legajo.Text = _profile.Legajo
            tb_edicion_modalidad.Text = String.Format("{0:N}", _profile.Modalidad)
            ddl_edicion_centro.SelectedValue = _profile.Centro
            rbl_edicion_habilitado.SelectedValue = Convert.ToString(usuario.IsApproved)
            tb_edicion_periodoAbierto.Text = _profile.PeriodoAbierto
            tb_edicion_CUIL.Text = _profile.CUIL
            tb_edicion_dependencia.Text = _profile.Dependencia

        End If
    End Sub

    Private Sub actualiarDatos()
        'en modo edicion actualiza los datos ingresados

        If Not usuario Is Nothing Then
            If Not String.IsNullOrEmpty(tb_edicion_clave.Text) And Not usuario.IsLockedOut Then
                usuario.ChangePassword(usuario.GetPassword(), tb_edicion_clave.Text)
            End If
            usuario.Email = tb_edicion_email.Text
            usuario.Comment = tb_edicion_comentarios.Text
            usuario.IsApproved = rbl_edicion_habilitado.SelectedValue
            Membership.UpdateUser(usuario)

            'actualizacion de roles
            For Each it As ListItem In cbl_edicion_roles.Items
                If Not it.Selected And Roles.IsUserInRole(usuario.UserName, it.Text) Then
                    Roles.RemoveUserFromRole(usuario.UserName, it.Text)
                End If
                If it.Selected And Not Roles.IsUserInRole(usuario.UserName, it.Text) Then
                    Roles.AddUserToRole(usuario.UserName, it.Text)
                End If
            Next

            'actualizacion de legajo
            Dim _profile As ProfileCommon = Profile.GetProfile(usuario.UserName)
            _profile.Legajo = tb_edicion_legajo.Text.Trim
            If tb_edicion_modalidad.Text <> "" Then
                _profile.Modalidad = Convert.ToDecimal(tb_edicion_modalidad.Text.Replace(".", ","))
            End If

            _profile.Centro = ddl_edicion_centro.SelectedValue
            If tb_edicion_periodoAbierto.Text <> "" Then
                _profile.PeriodoAbierto = Convert.ToInt16(tb_edicion_periodoAbierto.Text)
            Else
                _profile.PeriodoAbierto = 0
            End If
            _profile.CUIL = tb_edicion_CUIL.Text
            _profile.Dependencia = tb_edicion_dependencia.Text

            _profile.Save()
            'If _profile.Legajo <> tb_edicion_legajo.Text.Trim Then
            'End If

            notificacion = "Datos actualizados"
            tipoNotificacion = TiposNotificacion.OK
        End If
    End Sub

    Private Sub nuevosDatos()
        'en modo alta genera un nuevo usuario
        usuario = Nothing

        Try
            usuario = Membership.CreateUser(tb_alta_nombre.Text.Trim, _
                                             tb_alta_clave.Text, _
                                             tb_alta_email.Text.Trim)

        Catch ex1 As MembershipCreateUserException
            notificacion = String.Format("ERROR: {0}", ex1.Message)
            tipoNotificacion = TiposNotificacion.Alerta
        Catch ex As Exception
            notificacion = String.Format("ERROR: {0}", ex.Message)
            tipoNotificacion = TiposNotificacion.Alerta

        End Try
        If usuario IsNot Nothing Then
            usuario.Comment = tb_alta_comentarios.Text
            usuario.IsApproved = rbl_alta_habilitado.SelectedValue
            Membership.UpdateUser(usuario)

            'actualizacion de roles
            For Each it As ListItem In cbl_alta_roles.Items
                If it.Selected Then
                    Roles.AddUserToRole(usuario.UserName, it.Text)
                End If
            Next

            'valores de perfil
            Dim _profile As ProfileCommon = Profile.GetProfile(usuario.UserName)

            _profile.Legajo = tb_alta_legajo.Text.Trim
            If tb_alta_modalidad.Text <> "" Then
                _profile.Modalidad = Convert.ToDecimal(tb_alta_modalidad.Text.Replace(".", ","))
            End If

            _profile.Centro = ddl_alta_centro.SelectedValue
            If tb_alta_periodoAbierto.Text <> "" Then
                _profile.PeriodoAbierto = Convert.ToInt16(tb_edicion_periodoAbierto.Text)
            Else
                _profile.PeriodoAbierto = 0
            End If
            _profile.CUIL = tb_alta_CUIL.Text
            _profile.Dependencia = tb_alta_dependencia.Text

            _profile.Save()
            notificacion = "Usuario creado"
            tipoNotificacion = TiposNotificacion.OK

        End If
    End Sub

    Private Sub eliminarUsuario()
        'elimina definitivamente el usuario
        If usuario IsNot Nothing Then
            Dim rolesDeUsuario() As String = Roles.GetRolesForUser(usuario.UserName)
            For Each rol As String In rolesDeUsuario
                Roles.RemoveUserFromRole(usuario.UserName, rol)
            Next
            Try
                Membership.DeleteUser(usuario.UserName, True)
                notificacion = "El usuario ha sido eliminado."
                tipoNotificacion = TiposNotificacion.OK
                multiview.ActiveViewIndex = -1
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

#Region "eventos de controles"

    Protected Sub btnEdicionActualizar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lb_edicion_actualizar.Click
        If Page.IsValid Then
            If tb_edicion_periodoAbierto.Text <> "" AndAlso Int16.Parse(tb_edicion_periodoAbierto.Text) > maxDiasPermitidos Then
                notificacion = String.Format("Ingresar un numero de Periodo Abierto menor a {0}", maxDiasPermitidos)
                tipoNotificacion = TiposNotificacion.Advertencia
                Exit Sub
            End If
            actualiarDatos()
        End If
    End Sub

    Protected Sub lb_nuevo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lb_nuevo.Click
        multiview.SetActiveView(v_alta)
    End Sub

    Protected Sub lb_alta_cancelar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lb_alta_cancelar.Click
        multiview.SetActiveView(v_edicion)
    End Sub

    Protected Sub lb_alta_aceptar_Click(sender As Object, e As EventArgs) Handles lb_alta_aceptar.Click
        If Page.IsValid Then
            If tb_alta_periodoAbierto.Text <> "" AndAlso Int16.Parse(tb_alta_periodoAbierto.Text) > maxDiasPermitidos Then
                notificacion = String.Format("Ingresar un numero de Periodo Abierto menor a {0}", maxDiasPermitidos)
                tipoNotificacion = TiposNotificacion.Advertencia
                Exit Sub
            End If
            nuevosDatos()
        End If
    End Sub

    Protected Sub lb_edicion_eliminar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lb_edicion_eliminar.Click
        eliminarUsuario()
    End Sub

    Protected Sub btn_desbloquear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_desbloquear.Click
        usuario.UnlockUser()
        buscarDatos()
    End Sub
#End Region
End Class
