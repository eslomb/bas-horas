Imports System.Linq
Imports System.Data.Entity.Infrastructure

Partial Class app_cargar_horas
    Inherits PaginaApp

    Protected ReadOnly Property CantTareas() As Int16
        Get
            Return 5
        End Get
    End Property
    Protected registros As List(Of vRegistros)
    Protected modulos As New List(Of Ctrl_tarea)
    Protected cph As ContentPlaceHolder
    Private resultado As ResultadoOperaciones

    Private Structure ResultadoOperaciones
        Property cod As Int16
        Property descripcion As String
    End Structure


#Region "Page events"

    Protected Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        agregarRecurso(recursos.JqueryUI)
        If Not usuarioEsAdmin Then
            tipoCalendario = TiposCalendario.Restrictivo
        End If

    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init

        cph = Me.Master.FindControl("cph_contenido")
        agregarRecurso(recursos.Chosen)
        agregarRecurso(recursos.Datepicker)
        agregarScript("js/tareas.js")
        If Not bll.permitirCargas Then
            pnl_cargas.Visible = False
            pnl_controles.Visible = pnl_cargas.Visible

            l_result.Text = "Disculpas, en este momento no está habilitada la carga de tareas."
        End If
        If Not usuarioEsAdmin Then
            agregarJavascript("datepicker_settings.daysOfWeekDisabled= '0,6';")
        End If
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        If Not IsPostBack Then
            If usuarioEsBasico Then
                pnl_novedad.Visible = True
            End If

            ddl_usuarios.DataSource = bll.listarUsuarios()
            ddl_usuarios.DataBind()
            ddl_usuarios.SelectedValue = nombreUsuarioLogueado

            If Not usuarioEsAdmin Then
                ddl_usuarios.Enabled = False

            End If
            If usuarioEsBasico Then
                ddl_usuarios.CssClass = "hidden"
            End If
            generarDiasPrevios()
        End If

        Dim modulo As Ctrl_tarea
        For j As Int16 = 1 To CantTareas
            modulo = cph.FindControl("Tarea" & (j).ToString)
            If Not IsPostBack Then
                modulo.listadoPaises = bll.listarPaises().ToList

            End If
            modulos.Add(modulo)
        Next

        'mod temporal para abrir fechas 26, 27 y 28/6/2019 a Basicos 
        'Dim hoy = Date.Now
        'If hoy.Year = 2019 And hoy.Month = 6 And hoy.Day >= 25 Then
        '    Dim diasDelMes As Int16 = (System.DateTime.DaysInMonth(hoy.Year, 6))
        '    cargaTareasHasta = diasDelMes - hoy.Day

        'End If
    End Sub

#End Region


    Private Sub generarDiasPrevios()
        ddl_CargaPordia.Items.Clear()
        ddl_CargaPordia.Enabled = True
        ddl_CargaPordia.DataSource = bll.FechasUltimasTareas(ddl_usuarios.SelectedValue, 10)
        ddl_CargaPordia.DataBind()
        If ddl_CargaPordia.Items.Count = 0 Then
            ddl_CargaPordia.Items.Add(New ListItem("No se encontraron datos", ""))
            ddl_CargaPordia.Enabled = False
        End If

    End Sub

    Private Sub precargarPorDia()
        Dim paises As DbQuery(Of PAISES) = bll.listarPaises()
        registros = bll.consultarRegistros(ddl_usuarios.SelectedValue, ddl_CargaPordia.SelectedValue).ToList

        For j As Int16 = 1 To CantTareas
            modulos(j - 1).limpiarDatos()
        Next
        Dim modulo As Ctrl_tarea
        For i As Int16 = 0 To registros.Count - 1
            modulo = modulos(i)
            Dim r As vRegistros = registros(i)
            If modulo IsNot Nothing Then
                'modulo.IDRegistro = r.IDReg
                modulo.IdActividad = r.idActividad
                modulo.Actividad = r.Actividad
                modulo.IdProducto = r.idProducto
                modulo.Producto = r.Producto
                modulo.CodPais = r.CodPais
                modulo.Horas = r.Horas
                modulo.Minutos = r.Minutos
                modulo.Observaciones = r.Observaciones
            End If
        Next
    End Sub

    Private Sub guardarDatos()
        For Each modulo As Ctrl_tarea In modulos
            
            If modulo.IDRegistro > 0 Then
                'actualiza tarea
                ' falta definir si es necesario o se hace a traves de los reportes
            Else
                'ingresa tarea
                If (modulo.Actividad <> "" Or modulo.Producto <> "") Then
                    If modulo.IdActividad > 0 And modulo.IdProducto > 0 Then
                        If (modulo.Horas > 0 Or modulo.Minutos > 0) Then
                            Dim nuevoRegistro As New REGISTROS
                            nuevoRegistro.idActividad = modulo.IdActividad
                            nuevoRegistro.idProducto = modulo.IdProducto
                            nuevoRegistro.Usuario = ddl_usuarios.SelectedValue
                            nuevoRegistro.Fecha = modulo.Fecha
                            nuevoRegistro.Horas = modulo.Horas
                            nuevoRegistro.Minutos = modulo.Minutos
                            nuevoRegistro.CodPais = modulo.CodPais
                            nuevoRegistro.Observaciones = modulo.Observaciones

                            If Not bll.ExisteRegistro(nuevoRegistro) Then
                                If nuevoRegistro.Fecha < Date.Now.AddDays(-cargaTareasDesde).ToShortDateString Or nuevoRegistro.Fecha > Date.Now.AddDays(cargaTareasHasta + 1).ToShortDateString Then
                                    modulo.Estado = 3
                                    modulo.Resultado = "No se permite guardar esta tarea en la fecha especificada."
                                    modulo.Resultado &= " Fecha desde permitida: " & Date.Now.AddDays(-cargaTareasDesde).ToShortDateString
                                    modulo.Resultado &= " Fecha hasta permitida: " & Date.Now.AddDays(cargaTareasHasta + 1).ToShortDateString

                                Else
                                    If usuarioEsAdmin Then
                                        'no hay restricción para administradores
                                        'bll.ingresarRegistro(nuevoRegistro)
                                        ingresarRegistro(modulo, nuevoRegistro)
                                    Else
                                        If bll.EsFechaLaborable(modulo.Fecha) Then
                                            'bll.ingresarRegistro(nuevoRegistro)
                                            ingresarRegistro(modulo, nuevoRegistro)
                                        Else
                                            modulo.Estado = 3
                                            modulo.Resultado = "No se permite cargar tareas en fecha no laborable."
                                        End If
                                    End If

                                End If
                            Else
                                modulo.Estado = 3
                                modulo.Resultado = "Esta tarea ya se encuentra guardada en el sistema."
                            End If

                            If bll.ErrMsg <> "" Then
                                modulo.Resultado = bll.ErrMsg
                                modulo.Estado = 2
                            End If
                            If bll.OpMsg <> "" Then
                                modulo.Resultado = bll.OpMsg
                                modulo.Estado = 1
                            End If
                        Else
                            modulo.Estado = 3
                            modulo.Resultado = "No se puede ingresar una tarea sin definir su duración."
                        End If
                    Else
                        modulo.Estado = 3
                        modulo.Resultado = "No se reconoce la Actividad o Producto, intentalo nuevamente."
                    End If
                End If
            End If
            'l_result.Text &= String.Format("- {0} - {1}<br>", modulo.IdActividad, modulo.IdProducto)
        Next
    End Sub

    Private Sub ingresarRegistro(ByRef modulo As Ctrl_tarea, ByVal nuevoReg As REGISTROS)
        '1– validar que, dentro de la vigencia del proyecto, no puedan cargarse las siguientes actividades:  DFIX, TFIX, DFFX, TFFX. 
        '2- validar que, vencida la fecha de finalizacion del proyecto, no se puedan cargar actividades  DVER y TVER.
        '3- validar que en las actividades   CAPE, COOR, INDE, INVE, ADMI, CALI se complete obligatoriamente el campo “observaciones”. 

        Dim ActivEnVigencia = {"DFIX", "TFIX", "DFFX", "TFFX"}
        Dim ActivFueraProyecto = {"DVER", "TVER"}
        Dim ActivConObs() As String = bll.OpcionConfig("ACTIV_CON_OBSERV").Valor.Replace(" ", "").Split(",") '{"CAPE", "COOR", "INDE", "INVE", "ADMI", "CALI"}
        Dim codActividadReg As String = bll.verActividad(nuevoReg.idActividad).Codigo
        Dim prod As PRODUCTOS = bll.verProducto(nuevoReg.idProducto)
        Dim permitido As Boolean = True



        If prod.FProyIni IsNot Nothing And prod.FProyFin IsNot Nothing Then
            If ActivEnVigencia.Contains(codActividadReg) Then
                If nuevoReg.Fecha >= prod.FProyIni And nuevoReg.Fecha <= prod.FProyFin Then
                    'validacion 1
                    permitido = False
                    modulo.Resultado = "Proyecto en curso; no se admite la actividad " + codActividadReg
                    modulo.Estado = 3
                End If
                If nuevoReg.Fecha < prod.FProyIni Then
                    permitido = False
                    modulo.Resultado = "La fecha debe ser igual o posterior a " + prod.FProyIni
                    modulo.Estado = 3
                End If
            End If

            If ActivFueraProyecto.Contains(codActividadReg) Then
                If nuevoReg.Fecha < prod.FProyIni Or nuevoReg.Fecha > prod.FProyFin Then
                    'validacion 2
                    permitido = False
                    modulo.Estado = 3
                    modulo.Resultado = "Proyecto cerrado; no admite DVER / TVER"
                End If
            End If

        End If

        If ActivConObs.Contains(codActividadReg) And nuevoReg.Observaciones.Trim = "" Then
            permitido = False
            modulo.Estado = 3
            modulo.Resultado = "La actividad requiere que se complete el campo de Observaciones"
        End If

        If permitido Then
            bll.ingresarRegistro(nuevoReg)
        End If
    End Sub

#Region "Controls events"

    Protected Sub lb_cargar_Click(sender As Object, e As EventArgs) Handles lb_cargar.Click
        guardarDatos()
    End Sub

    Protected Sub lb_precargarFecha_Click(sender As Object, e As EventArgs) Handles lb_precargarFecha.Click
        If ddl_CargaPordia.SelectedValue <> "" Then
            precargarPorDia()
        Else
            notificacion = "Seleccionar una fecha"
        End If

    End Sub

    Protected Sub ddl_usuarios_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddl_usuarios.SelectedIndexChanged
        generarDiasPrevios()
    End Sub

#End Region
    
End Class
