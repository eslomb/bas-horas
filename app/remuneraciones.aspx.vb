Imports System.Data.OleDb
Imports DTOs.Usuarios

Partial Class app_remuneraciones
    Inherits PaginaAdmin

    Private meses(11) As String
    Private cantAnios As Int16 = 3
    Private anios(cantAnios) As Int16



    Protected Sub Page_Init_Child(sender As Object, e As EventArgs) Handles Me.Init

        If Not bll.habilitarRemuneraciones(nombreUsuarioLogueado) Then
            Response.Write("Se requieren habilitar permisos para acceder a esta seccion. Consultar con Carlos Rocchi.")
            Response.End()
        End If
        agregarRecurso(recursos.Chosen)
        meses(0) = "Enero"
        meses(1) = "Febrero"
        meses(2) = "Marzo"
        meses(3) = "Abril"
        meses(4) = "Mayo"
        meses(5) = "Junio"
        meses(6) = "Julio"
        meses(7) = "Agosto"
        meses(8) = "Septiembre"
        meses(9) = "Octubre"
        meses(10) = "Noviembre"
        meses(11) = "Diciembre"

        Dim ini As Int16 = Date.Now.Year
        Dim n As Int16 = 0
        For i As Int16 = ini To ini - cantAnios Step -1
            anios(n) = i
            n += 1
        Next


        'actualizarLegajos()
    End Sub

    Protected Sub ddl_meses_OnLoad(sender As Object, e As EventArgs)
        Dim ddl As DropDownList = CType(sender, DropDownList)
        If ddl.Items.Count <= 1 Then
            Dim mesActual As Int16 = Date.Now.Month

            Dim li As ListItem
            For i As Int16 = 0 To meses.Length - 1
                li = New ListItem(meses(i), i + 1)
                If ddl.CssClass.Contains("seleccionar-actual") And i + 1 = mesActual Then
                    li.Selected = True
                End If
                ddl.Items.Add(li)
            Next
        End If
    End Sub

    Protected Sub ddl_anios_OnLoad(sender As Object, e As EventArgs)
        Dim ddl As DropDownList = CType(sender, DropDownList)
        If ddl.Items.Count <= 1 Then
            Dim anioActual As Int16 = Date.Now.Year

            Dim li As ListItem
            For i As Int16 = 0 To anios.Length - 1
                li = New ListItem(anios(i), anios(i))
                If ddl.CssClass.Contains("seleccionar-actual") And anios(i) = anioActual Then
                    li.Selected = True
                End If
                ddl.Items.Add(li)
            Next
        End If

    End Sub

    Private Sub exportar_usuarios_activos()
        Dim listaRemuneraciones As New List(Of Object)
        Dim mes As Int16 = Date.Now.Month
        Dim anio As Int16 = Date.Now.Year
        Dim qUsuarios = From u In Me.usuarios.listarUsuariosActivos

        For Each u As DatosUsuario In Me.usuarios.listarUsuariosActivos
            Dim qRemun = (From r In bll.remuneracionesDeUsuario(u.UserName)
                          Select r).FirstOrDefault

            Dim ultRemuneracion As Decimal = 0
            If qRemun IsNot Nothing Then
                ultRemuneracion = qRemun.Remuneracion
            End If

            listaRemuneraciones.Add(New With {
                                    .Anio = anio,
                                      .Mes = mes,
                                      .Usuario = u.UserName,
                                      .Remuneracion = ultRemuneracion,
                                      .Legajo = u.Legajo})
        Next

        gv_exportacion.DataSource = listaRemuneraciones
        gv_exportacion.DataBind()


        ExportarXSLX(gv_exportacion, "Remuneraciones")
    End Sub

    Private Sub ImportarSQL(sPath As String)
        ' Connect to Excel 2007 earlier version
        'Dim sSourceConstr As String = String.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; Extended Properties=""Excel 8.0;HDR=YES;""", sPath)
        ' Connect to Excel 2007 (and later) files with the Xlsx file extension 
        Dim sSourceConstr As String = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;HDR=YES;""", sPath)

        Dim sDestConstr As String = ConfigurationManager.ConnectionStrings("LocalSqlServer").ConnectionString
        Dim sSourceConnection As New OleDbConnection(sSourceConstr)
        Using sSourceConnection
            Dim sql As String = String.Format("Select [Mes], [Anio], [Remuneracion], [Usuario] From [Remuneraciones$] where Remuneracion is not null")
            Dim command As New OleDbCommand(sql, sSourceConnection)

            sSourceConnection.Open()
            Using dr As OleDbDataReader = command.ExecuteReader
                Using bulkCopy As New SqlBulkCopy(sDestConstr)
                    bulkCopy.DestinationTableName = "REMUNERACIONES"
                    bulkCopy.ColumnMappings.Add("Mes", "Mes")
                    bulkCopy.ColumnMappings.Add("Anio", "Anio")
                    bulkCopy.ColumnMappings.Add("Remuneracion", "Remuneracion")
                    bulkCopy.ColumnMappings.Add("Usuario", "Usuario")
                    Try
                        bulkCopy.WriteToServer(dr)
                        gv_listado.DataBind()
                        notificacion = "Los datos se importaron correctamente."
                    Catch ex As Exception
                        notificacion = "Error, no se pudieron importar los datos. " + ex.Message
                        tipoNotificacion = TiposNotificacion.Alerta
                    End Try
                End Using
            End Using
        End Using
    End Sub

    Protected Sub lb_importar_Click(sender As Object, e As EventArgs) Handles lb_importar.Click
        If fup_excel.HasFile Then
            Dim sPath As String = Server.MapPath("~/importaciones/" + fup_excel.FileName)
            fup_excel.SaveAs(sPath)
            ImportarSQL(sPath)
        Else
            notificacion = "Indicar el archivo a importar con el botón Examinar"
            tipoNotificacion = TiposNotificacion.Alerta
        End If
    End Sub

    Protected Sub lb_exportar_activos_Click(sender As Object, e As EventArgs) Handles lb_exportar_activos.Click
        exportar_usuarios_activos()
    End Sub

    Private Sub actualizarLegajos()
        'actualiza los valores de REMUNERACIONES.Legajo según los datos cargados en Membership
        Dim cntx As New BASHorasEntities
        Dim listadoUsuarios As List(Of DatosUsuario) = Me.usuarios.listarUsuarios
        Dim qRemu = From remu In cntx.REMUNERACIONES Select remu

        For Each r In qRemu
            Dim qq = From lu In listadoUsuarios Where lu.UserName.Equals(r.Usuario) Select lu
            r.Legajo = qq.First.Legajo
        Next
        cntx.SaveChanges()
    End Sub

    
End Class
