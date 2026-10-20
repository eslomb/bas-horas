Imports System.Linq
Imports System.Data.Entity.Infrastructure
Imports DTOs.Usuarios

Imports ClosedXML.Excel
Partial Class app_reporte
    Inherits PaginaApp

    Private modoActual As Int16
    Private Enum Modos
        Listado
        Edicion
    End Enum

    Public ReadOnly Property IDReporte() As String
        Get
            If Request.QueryString("idrep") <> "" Then
                Return Request.QueryString("idrep")
            End If
            Return ""
        End Get

    End Property

    Private reporteActual As REPORTES
    Private permiteEditar As Boolean
    Private listadoUsuarios As List(Of DTOs.Usuarios.DatosUsuario)
    Private diasHabiles As List(Of DIAS_HABILES)

    Private Class ReporteExportacion
        Inherits spReporteGeneral_Result

        Property Costo As Nullable(Of Decimal)

        Private _duracion As String
        Public ReadOnly Property Duracion() As Double
            Get
                Return HORAS + (MINUTOS / 60.0)
            End Get
        End Property

    End Class


#Region "Page Events"

    Protected Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        modoActual = Modos.Listado
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        permiteEditar = bll.permitirCargas
        listadoUsuarios = Me.usuarios.listarUsuarios()
        'diasHabiles = bll.diasHabiles.ToList()
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        reporteActual = bll.reportePorID(IDReporte)
        If reporteActual Is Nothing Then
            Exit Sub
        End If
        If Not IsPostBack Then
            l_tituloReporte.Text = String.Format("""{0}""", reporteActual.Titulo)
            ctrlTarea.listadoPaises = bll.listarPaises().ToList

            generarReporte()
        End If

        If Not usuarioEsAdmin Then
            tipoCalendario = TiposCalendario.Restrictivo
        End If
    End Sub

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        agregarRecurso(recursos.JqueryUI)
        agregarRecurso(recursos.Datepicker)
        agregarScript("js/tareas.js")
        If Not usuarioEsAdmin Then
            agregarJavascript("datepicker_settings.daysOfWeekDisabled= '0,6';")
        End If

        pnl_edicion.Visible = (modoActual = Modos.Edicion)
        pnl_controles.Visible = (modoActual = Modos.Listado And gv_reporte.Rows.Count > 0)
        gv_reporte.Visible = (modoActual = Modos.Listado)
    End Sub

#End Region

#Region "Page methods"

    Public Sub generarReporte()
        Dim consulta As List(Of spReporteGeneral_Result) = bll.reporteGeneral(crearFiltros())

        gv_reporte.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue)
        gv_reporte.DataSource = consulta
        gv_reporte.DataBind()
    End Sub

    Public Sub generarReporteExportacion()
        Dim query As List(Of spReporteGeneral_Result) = bll.reporteGeneral(crearFiltros())

        Dim consulta As List(Of ReporteExportacion)

        consulta = query.Select(Function(r) New ReporteExportacion With {
                                    .IDReg = r.IDReg,
                                    .ACTIVIDAD = r.ACTIVIDAD,
                                    .CENTRO = r.CENTRO,
                                    .COD_ACTIVIDAD = r.COD_ACTIVIDAD,
                                    .CUIL = r.CUIL,
                                    .FECHA_ACTIVIDAD = r.FECHA_ACTIVIDAD,
                                    .HORAS = r.HORAS,
                                    .MINUTOS = r.MINUTOS,
                                    .ID_PRODUCTO = r.ID_PRODUCTO,
                                    .JORNADA = r.JORNADA,
                                    .LEGAJO = r.LEGAJO,
                                    .OBSERVACIONES = r.OBSERVACIONES,
                                    .PAIS = r.PAIS,
                                    .PRODUCTO = r.PRODUCTO,
                                    .REMUNERACION = r.REMUNERACION,
                                    .USUARIO = r.USUARIO,
                                    .Costo = CalcularCostoTarea(r.REMUNERACION, r.USUARIO, r.FECHA_ACTIVIDAD, r.HORAS, r.MINUTOS)
                                    }).ToList()

        gv_export.DataSource = consulta
        gv_export.DataBind()

        Dim habilita As Boolean = bll.habilitarRemuneraciones(nombreUsuarioLogueado)
        Dim cssColumna As String = ""
        If Not habilita Then
            cssColumna = "noexportar"
        End If

        ocultarColumna(4, cssColumna)
        ocultarColumna(12, cssColumna)
        ocultarColumna(13, cssColumna)
        ocultarColumna(14, cssColumna)

        ExportarXSLX(gv_export, nombreReporte:=l_tituloReporte.Text.Trim)

    End Sub




    Private Sub ocultarColumna(indice As Int32, css As String)
        gv_export.HeaderRow.Cells(indice).CssClass = css
        gv_export.Columns(indice).Visible = False

    End Sub

    Public Function crearFiltros() As DTOs.Reportes.FiltroReporte
        Dim filtros As New DTOs.Reportes.FiltroReporte With {
            .Desde = reporteActual.Desde,
            .Hasta = reporteActual.Hasta,
            .CodPais = reporteActual.CodPais
            }

        If reporteActual.Actividad_id > 0 Then
            filtros.IdAct = reporteActual.Actividad_id
        End If
        If reporteActual.Producto_id > 0 Then
            filtros.Idprod = reporteActual.Producto_id
        End If

        'los parametros de Usuario y Centro se determina de acuerdo al grupo al que pertenece el usuario y a los valores guardados para el reporte en cuestion
        'Los Basicos solo pueden ver sus propias actividades
        'Los Avanzados pueden ver usuarios de su propio Centro 
        'Los Administradores pueden ver de todos los usuarios y Centros
        'Se hace una excepción con Irma Vazquez para que pueda ver los registros del Centro que tenga el Reporte

        If usuarioEsAdmin Then
            If reporteActual.DeUsuario IsNot Nothing Then
                filtros.Usuario = reporteActual.DeUsuario
            End If
            filtros.Centro = reporteActual.DeCentro
        End If

        If usuarioEsAvanzado Then
            If reporteActual.DeUsuario IsNot Nothing Then
                filtros.Usuario = reporteActual.DeUsuario
            End If

            Dim _profile As ProfileCommon = Profile.GetProfile(nombreUsuarioLogueado)
            filtros.Centro = _profile.Centro

            Dim esGerente As Boolean = nombreUsuarioLogueado.StartsWith("LOPEZ KARINA")
            If esGerente Then
                filtros.Centro = IIf(reporteActual.DeCentro IsNot Nothing, reporteActual.DeCentro, "PV0,PV1,PV2,PV3,PV4,SIT")
            End If

            'si intenta obtener registros de otros centros
            If Not esGerente Then
                If reporteActual.DeCentro <> _profile.Centro And Not String.IsNullOrEmpty(reporteActual.DeCentro) Then
                    notificacion = "Este reporte tiene acceso solo a registros del Centro " + _profile.Centro
                    tipoNotificacion = TiposNotificacion.Advertencia
                End If
            End If
        End If

        If usuarioEsBasico Then
            filtros.Usuario = reporteActual.Usuario
        End If

        Return filtros

    End Function

    Private Sub eliminarSeleccion()
        Dim cb As New CheckBox
        Dim hf As New HiddenField
        For Each gvr As GridViewRow In gv_reporte.Rows
            If gvr.RowType = DataControlRowType.DataRow Then
                cb = gvr.FindControl("cb_eliminar")
                hf = gvr.FindControl("hf_eliminar")
                If cb IsNot Nothing Then
                    If cb.Checked Then
                        ods_listado.DeleteParameters("idReg").DefaultValue = Convert.ToInt32((hf.Value))
                        ods_listado.Delete()
                    End If
                End If
            End If
        Next
        gv_reporte.DataBind()
    End Sub

    Private Sub cargarTarea(idreg As Int32)
        Dim r = bll.verRegistro(idreg)
        Dim modulo As Ctrl_tarea = ctrlTarea
        modulo.Modificable = True

        modulo.IDRegistro = r.IDReg
        modulo.Fecha = r.Fecha
        modulo.IdActividad = r.idActividad
        modulo.Actividad = r.ACTIVIDADES.Actividad
        modulo.IdProducto = r.idProducto
        modulo.Producto = r.PRODUCTOS.Producto
        modulo.CodPais = r.CodPais
        modulo.Horas = r.Horas
        modulo.Minutos = r.Minutos
        modulo.Observaciones = r.Observaciones

        modoActual = Modos.Edicion
    End Sub

    Private Sub actualizarTarea()
        Dim modulo As Ctrl_tarea = ctrlTarea
        Dim actualizacion As REGISTROS = bll.verRegistro(modulo.IDRegistro)

        actualizacion.Fecha = modulo.Fecha
        actualizacion.idActividad = modulo.IdActividad
        actualizacion.idProducto = modulo.IdProducto
        actualizacion.CodPais = modulo.CodPais
        actualizacion.Horas = modulo.Horas
        actualizacion.Minutos = modulo.Minutos
        actualizacion.Observaciones = modulo.Observaciones

        If (modulo.Actividad <> "" Or modulo.Producto <> "") Then
            If modulo.IdActividad > 0 And modulo.IdProducto > 0 Then
                If (modulo.Horas > 0 Or modulo.Minutos > 0) Then
                    If Not bll.ExisteRegistro(actualizacion) Then
                        If actualizacion.Fecha < Date.Now.AddDays(-cargaTareasDesde).ToShortDateString Or actualizacion.Fecha > Date.Now.AddDays(cargaTareasHasta + 1).ToShortDateString Then
                            tipoNotificacion = TiposNotificacion.Advertencia
                            notificacion = "No se permite guardar esta tarea en la fecha especificada."
                        Else
                            If usuarioEsAdmin Then
                                'no hay restricción para administradores
                                bll.actualizarRegistro(actualizacion)
                            Else
                                If bll.EsFechaLaborable(modulo.Fecha) Then
                                    bll.actualizarRegistro(actualizacion)
                                Else
                                    tipoNotificacion = TiposNotificacion.Alerta
                                    notificacion = "No se permite cargar tareas en fecha no laborable."
                                End If
                            End If
                        End If
                    End If
                Else
                    tipoNotificacion = TiposNotificacion.Alerta
                    notificacion = "No se puede ingresar una tarea sin definir su duración."
                End If
            Else
                tipoNotificacion = TiposNotificacion.Advertencia
                notificacion = "No se reconoce la Actividad o Producto, intentalo nuevamente."
            End If
        End If


        If bll.ErrMsg <> "" Then
            notificacion = bll.ErrMsg
            tipoNotificacion = TiposNotificacion.Alerta
        End If
        If bll.OpMsg <> "" Then
            notificacion = bll.OpMsg
            tipoNotificacion = TiposNotificacion.OK
            gv_reporte.DataBind()
            modoActual = Modos.Listado
        End If
    End Sub

    Protected Function CantidadDiasHabiles(fecha As Date) As Int16?
        Dim mes As Int16 = fecha.Month
        Dim anio As Int16 = fecha.Year
        Dim q = diasHabiles.Where(Function(d) d.Mes.Equals(mes) And d.Anio.Equals(anio)).FirstOrDefault

        If q IsNot Nothing Then
            Return q.CantDias
        End If
        Return Nothing
    End Function

    Public Function CalcularCostoTarea(remuneracion As Decimal?, userName As String, fechaTarea As Date, horas As Int16, minutos As Int16) As Nullable(Of Decimal)
        ' ValorHora = Remuneracion / DiasHabiles / Jornada
        ' Costo = valorHora × (Horas + Minutos / 60)

        If Not remuneracion.HasValue Then Return Nothing

        Dim diasHabiles As Nullable(Of Int16) = CantidadDiasHabiles(fechaTarea)
        If diasHabiles Is Nothing Then Return Nothing

        Dim usuarios As New Usuarios()
        Dim datos As DatosUsuario = usuarios.obtenerDatos(userName)
        Dim jornada As Decimal = datos.Modalidad
        If jornada <= 0 Then Return Nothing

        Dim valorHora As Decimal = remuneracion / diasHabiles / jornada
        Dim duracion As Decimal = horas + (minutos / 60D)

        Return Decimal.Round(valorHora * duracion, 2)
    End Function

#End Region

#Region "Control Events"

    Protected Sub lb_eliminarSeleccion_Click(sender As Object, e As EventArgs) Handles lb_eliminarSeleccion.Click
        eliminarSeleccion()
    End Sub

    Protected Sub lb_editar_Click(sender As Object, e As EventArgs)
        Dim lb As LinkButton = CType(sender, LinkButton)
        cargarTarea(Convert.ToInt32(lb.CommandArgument))
    End Sub

    Protected Sub lb_actualizar_Click(sender As Object, e As EventArgs) Handles lb_actualizar.Click
        actualizarTarea()
    End Sub

    Protected Sub lb_cancelarActualizacion_Click(sender As Object, e As EventArgs) Handles lb_cancelarActualizacion.Click
        modoActual = Modos.Listado
    End Sub

    Protected Sub lb_exportar_Click(sender As Object, e As EventArgs) Handles lb_exportar.Click
        'Dim consulta As List(Of spReporteGeneral_Result) = bll.reporteGeneral(crearFiltros())
        'gv_export.DataSource = consulta
        'gv_export.DataBind()
        'ExportToExcel(gv_export, l_tituloReporte.Text)

        generarReporteExportacion()
    End Sub

    Protected Sub lb_editar_Load(sender As Object, e As EventArgs)
        Dim lb As LinkButton = CType(sender, LinkButton)
        lb.Visible = permiteEditar
    End Sub

    Protected Sub gv_reporte_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        Exit Sub




        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim fechaFila As Date = CType(e.Row.Cells(1).Text, Date)
            Dim nombreUsuarioActual As String = HttpUtility.HtmlDecode(e.Row.Cells(2).Text)

            Dim q = (From u In listadoUsuarios Where u.UserName.Equals(nombreUsuarioActual) Select u).FirstOrDefault
            'Dim q2 = (From r In bll.remuneracionesDeUsuario(nombreUsuarioActual) Where r.Mes = fechaFila.Month And r.Anio = fechaFila.Year Select r).FirstOrDefault

            If q IsNot Nothing Then
                e.Row.Cells(3).Text = q.Legajo
                e.Row.Cells(4).Text = q.Modalidad
                e.Row.Cells(5).Text = q.Centro
                e.Row.Cells(12).Text = q.CUIL
                'If q2 IsNot Nothing Then
                '    e.Row.Cells(12).Text = String.Format("${0:N2}", q2.Remuneracion)
                'End If

            End If
        End If

    End Sub

    Private Sub gv_reporte_PageIndexChanging(sender As Object, e As GridViewPageEventArgs) Handles gv_reporte.PageIndexChanging
        Dim gv As GridView = CType(sender, GridView)
        gv.PageIndex = e.NewPageIndex
        generarReporte()
    End Sub

    Protected Sub ddlPageSize_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        ' 1. Reiniciar el índice de página a la primera hoja para evitar desbordamientos
        gv_reporte.PageIndex = 0

        generarReporte()
    End Sub

    Protected Sub lb_eliminar_Click(sender As Object, e As EventArgs)
        Dim lb As LinkButton = CType(sender, LinkButton)
        Dim idRegistro As Int32 = Convert.ToInt32(lb.CommandArgument)
        bll.eliminarRegistro(idRegistro)
        If bll.ErrMsg <> "" Then
            notificacion = bll.ErrMsg
            tipoNotificacion = TiposNotificacion.Alerta
        Else
            notificacion = "Registro eliminado"
            tipoNotificacion = TiposNotificacion.Informacion

            generarReporte()
        End If
    End Sub


#End Region


#Region "Gridvew helpers"

    Protected Function verLegajo(username As String) As String
        Dim q As DTOs.Usuarios.DatosUsuario = (From u In listadoUsuarios Where u.UserName.Equals(username) Select u).FirstOrDefault
        If q IsNot Nothing Then
            Return q.Legajo
        End If
        Return ""
    End Function

    Protected Function verModalidad(username As String) As String
        Dim q As DTOs.Usuarios.DatosUsuario = (From u In listadoUsuarios Where u.UserName.Equals(username) Select u).FirstOrDefault
        If q IsNot Nothing Then
            Return q.Modalidad.ToString()
        End If
        Return ""
    End Function

    Protected Function verCentro(username As String) As String
        Dim q As DTOs.Usuarios.DatosUsuario = (From u In listadoUsuarios Where u.UserName.Equals(username) Select u).FirstOrDefault
        If q IsNot Nothing Then
            Return q.Centro
        End If
        Return ""
    End Function

    Protected Function verCUIL(username As String) As String
        Dim q As DTOs.Usuarios.DatosUsuario = (From u In listadoUsuarios Where u.UserName.Equals(username) Select u).FirstOrDefault
        If q IsNot Nothing Then
            Return q.CUIL
        End If
        Return ""
    End Function


#End Region

End Class
