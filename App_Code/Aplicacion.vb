Imports Microsoft.VisualBasic
Imports System.Linq
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Validation
Imports ClosedXML.Excel
Imports System.Globalization
Imports DTOs.Usuarios

Namespace Aplicacion


    Public Class Logica

        Private cntx As BASHorasEntities

        Private _errMsg As String
        Public ReadOnly Property ErrMsg() As String
            Get
                Return _errMsg
            End Get
        End Property

        Private _opMsg As String
        Public ReadOnly Property OpMsg() As String
            Get
                Return _opMsg
            End Get
        End Property



        Public Sub New()
            cntx = New BASHorasEntities
            _errMsg = String.Empty
            _opMsg = String.Empty
        End Sub

        Public Function verProductosActivos() As DbQuery(Of PRODUCTOS)
            Dim q = From p In cntx.PRODUCTOS
                    Where p.Activo = True
                    Order By p.Producto
                    Select p

            Return q
        End Function

        Public Function ReportesDeUsuario(usuario As String) As DbQuery(Of REPORTES)
            'no se usa pero la dejo por ahora
            Return From r In cntx.REPORTES
                   Where r.Usuario.Contains(usuario)
                   Select r
        End Function

        Public Function reportePorID(idr As String) As REPORTES
            Return (From r In cntx.REPORTES
                    Where r.IDRep.ToString.Equals(idr)
                    Select r).FirstOrDefault
        End Function


        Public Function reporteGeneral(filtros As DTOs.Reportes.FiltroReporte) As List(Of spReporteGeneral_Result)

            ' Obtener los registros desde SQL
            Dim registros = cntx.spReporteGeneral(
            filtros.Desde,
            filtros.Hasta,
            filtros.Usuario,
            filtros.Idprod,
            filtros.IdAct,
            filtros.CodPais
        ).ToList()


            ' Si no hay filtro por Centro, devolver directamente
            If String.IsNullOrWhiteSpace(filtros.Centro) Then
                Return registros
            End If

            ' Centros seleccionados (HashSet = búsqueda muy rápida)
            Dim centros As New HashSet(Of String)(filtros.Centro.Replace(" ", "").Split(","c), StringComparer.OrdinalIgnoreCase)

            ' Obtener la lista de usuarios únicos
            Dim usuarios = registros.
        Select(Function(r) r.USUARIO).
        Where(Function(u) Not String.IsNullOrWhiteSpace(u)).
        Distinct().
        ToList()

            ' Cache de perfiles
            Dim perfiles As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each usuario In usuarios
                Dim profile = CType(ProfileBase.Create(usuario, True), ProfileCommon)
                perfiles(usuario) = profile.Centro
            Next

            ' Filtrar por Centro
            Return registros.Where(Function(r) perfiles.ContainsKey(r.USUARIO) AndAlso
                          Not String.IsNullOrEmpty(perfiles(r.USUARIO)) AndAlso
                          centros.Contains(perfiles(r.USUARIO))).
        ToList()

        End Function


        Public Function reporteGeneral_borrar(filtros As DTOs.Reportes.FiltroReporte) As List(Of spReporteGeneral_Result)

            Dim registros = (From rg In cntx.spReporteGeneral(filtros.Desde, filtros.Hasta, filtros.Usuario, filtros.Idprod, filtros.IdAct, filtros.CodPais)
                             Select rg)

            Dim centros As New List(Of String)
            If filtros.Centro IsNot Nothing Then
                centros = filtros.Centro.Replace(" ", "").Split(",").ToList()
            End If
            Dim listaPorCentro As New List(Of spReporteGeneral_Result)

            If filtros.Centro IsNot Nothing Then
                For Each item In registros
                    Dim _profile As ProfileCommon = ProfileBase.Create(item.USUARIO, True)
                    Dim targetProfile As ProfileCommon = _profile.GetProfile(item.USUARIO)

                    If centros.Contains(targetProfile.Centro) Then
                        listaPorCentro.Add(item)
                    End If

                Next
                Return listaPorCentro.ToList
            Else
                Return registros.ToList
            End If
        End Function

        Public Function analisisActividades(Optional concepto As String = Nothing, _
                                            Optional desde As String = Nothing, _
                                            Optional hasta As String = Nothing) As List(Of spAnalisisActividades_Result)
            Return cntx.spAnalisisActividades(concepto, desde, hasta).ToList
        End Function

        Public Function consultarRegistros(usuario As String, Optional fecha As DateTime = Nothing) As DbQuery(Of vRegistros)
            If fecha = Nothing Then
                fecha = Date.Now
            End If
            Return (From registros In cntx.vRegistros
                Select registros).Where(Function(p) p.Usuario.Contains(usuario) And p.Fecha = fecha).OrderBy(Function(p) p.IDReg)
        End Function

        Public Function verRegistro(idregistro As Int32) As REGISTROS
            Return (From r In cntx.REGISTROS
                    Where r.IDReg.Equals(idregistro)
                    Select r).FirstOrDefault
        End Function

        Public Function ExisteRegistro(registro As REGISTROS) As Boolean
            Dim q = (From r In cntx.REGISTROS
                   Where r.Fecha = registro.Fecha And r.idProducto = registro.idProducto And r.idActividad = registro.idActividad And r.Usuario = registro.Usuario And _
                   r.CodPais = registro.CodPais And r.Horas = registro.Horas And r.Minutos = registro.Minutos And r.Observaciones = registro.Observaciones
                   Select r)
            Dim cant = q.Count
            'If cant > 0 Then
            '    Current.Response.Write(q.FirstOrDefault.IDReg)
            'End If
            Return cant > 0
        End Function

        Public Sub ingresarRegistro(r As REGISTROS)
            cntx.REGISTROS.Add(r)
            cntx.SaveChanges()
            Try
                _opMsg = "Tarea registrada correctamente"
            Catch ex As Exception
                _errMsg = "Error al ingresar la tarea: " & ex.InnerException.InnerException.Message
            End Try
        End Sub

        Public Sub eliminarRegistro(idReg As Int32)
            Dim registro = (From r In cntx.REGISTROS
                     Where r.IDReg.Equals(idReg)
                     Select r).FirstOrDefault

            If registro.IDReg > 0 Then
                Try
                    cntx.REGISTROS.Remove(registro)
                    cntx.SaveChanges()
                    _opMsg = "Tarea eliminada"
                Catch ex As Exception
                    _errMsg = "No se puede eliminar la tarea. " & ex.InnerException.InnerException.Message
                End Try

            End If
        End Sub

        Public Sub actualizarRegistro(reg As REGISTROS)
            cntx.REGISTROS.Attach(reg)
            cntx.Entry(reg).State = Entity.EntityState.Modified
            Try
                cntx.SaveChanges()
                _opMsg = "Tarea actualizada correctamente"
            Catch ev As DbEntityValidationException
                _errMsg = "Error al modificar la tarea: " & ev.Message

            Catch ex As Exception
                _errMsg = "Error al modificar la tarea: " & ex.Message
            End Try
        End Sub

        Public Function listarUsuarios() As MembershipUserCollection
            Dim usuarios As MembershipUserCollection = Membership.GetAllUsers()

            Return usuarios
        End Function

        Public Function listarPaises() As DbQuery(Of PAISES)
            Return (From p In cntx.PAISES
                   Order By p.Preferido Descending, p.Pais
                   Select p)
        End Function

        Public Function verPais(Cod As String) As PAISES
            Return (From p In listarPaises()
                    Where p.Codigo.Equals(Cod)
                    Select p).FirstOrDefault
        End Function

        Public Function listarProductos() As DbQuery(Of PRODUCTOS)
            Return (From p In cntx.PRODUCTOS
                    Order By p.Producto
                    Select p)
        End Function

        Public Function listarProductosVisibles() As DbQuery(Of PRODUCTOS)
            Return (From p In listarProductos()
                    Where p.Activo
                    Select p)
        End Function

        Public Function verProducto(idprod As Int32) As PRODUCTOS
            Return (From p In listarProductos()
                    Where p.IDPro.Equals(idprod)
                    Select p).FirstOrDefault
        End Function

        Public Function listarActividades() As DbQuery(Of ACTIVIDADES)
            Return (From a In cntx.ACTIVIDADES
                    Order By a.Actividad
                    Select a)
        End Function

        Public Function listarActividadesHabilitadas() As DbQuery(Of ACTIVIDADES)
            Return (From a In listarActividades() Where a.Habilitada = True Select a)
        End Function

        Public Function verActividad(idact As Int32) As ACTIVIDADES
            Return (From a In listarActividades()
                    Where a.IDAct.Equals(idact)
                    Select a).FirstOrDefault
        End Function

        Public Function verActividad(codigo As String) As ACTIVIDADES
            Return (From a In listarActividades()
                    Where a.IDAct.Equals(codigo)
                    Select a).FirstOrDefault
        End Function

        Public Function listarCentros() As List(Of CENTROS)
            Return (From cc In cntx.CENTROS
                    Order By cc.Nombre
                    Select cc).ToList
        End Function

        Public Function remuneracionesDeUsuario(nombre As String) As List(Of REMUNERACIONES)
            Dim q = (From r In cntx.REMUNERACIONES Where r.Usuario = nombre Order By r.Anio Descending, r.Mes Descending Select r)
            Return q.ToList
        End Function

        Public Function FechasUltimasTareas(usuario As String, Optional cantidad As Int16 = 5) As Data.Entity.Core.Objects.ObjectResult(Of String)
            Return cntx.spFechasUltimasTareas(usuario, cantidad)
        End Function

        Public Function periodoCargas() As Int16
            Return Convert.ToInt16(OpcionConfig("PERIODO_CARGAS").Valor)
        End Function

        Public Function EsFechaLaborable(d As Date) As Boolean
            Dim ok As Boolean = True
            Dim dia As String = d.ToString("ddd", New System.Globalization.CultureInfo("es-ES"))
            Dim qFeriados = (From f In verFeriados()
                    Where f.Feriado.Equals(d)
                    Select f).ToList

            If dia.Equals("sá.") Or dia.Equals("do.") Or qFeriados.Count > 0 Then
                ok = False
            End If
            Return ok
        End Function

        Public Function verFeriados() As DbQuery(Of FERIADOS)
            Return (From f In cntx.FERIADOS
                    Order By f.Feriado Descending
                    Select f)
        End Function

        Public Function permitirCargas() As Boolean
            Return Convert.ToBoolean(OpcionConfig("PERMITIR_CARGAS").Valor)
        End Function

        Public Function perimitirEdicionFProyIni() As Boolean
            Return Convert.ToBoolean(OpcionConfig("PERMITIR_EDI_FP_INI").Valor)
        End Function

        Public Function OpcionConfig(codigo As String) As CONFIGURACIONES
            Dim q = (From c In cntx.CONFIGURACIONES
                    Where c.CodConfig.Equals(codigo)
                    Select c).FirstOrDefault
            Return q
        End Function

        Public Function actualizarConfiguracion(cod As String, valor As String) As String
            Dim resultado As String = String.Empty
            Dim config = OpcionConfig(cod)

            Try
                config.Valor = valor
                cntx.SaveChanges()
                resultado = "Configuración actualizada"
            Catch ex As Exception
                resultado = ex.Message
            End Try
            Return resultado
        End Function

        Public Function habilitarRemuneraciones(usuario As String) As Boolean
            Dim habilitar As Boolean = False
            If usuario.StartsWith("ROCCHI") Or usuario.StartsWith("CARRACEDO") Then
                habilitar = True
            End If
            Return habilitar
        End Function

        Public Function diasHabiles() As DbQuery(Of DIAS_HABILES)
            Return (From d In cntx.DIAS_HABILES Select d)
        End Function
    End Class


    Public Class Pagina
        Inherits Page

        Private estilos As New ArrayList
        Private scripts As New ArrayList
        Private carpetaRecursos As String
        Public bll As Logica

        Public Enum recursos
            Basicos
            Jquery
            JqueryUI
            Bootstrap
            Font_awesome
            MagnificPopup
            Toastr
            Datatables
            CKEditor
            Chosen
            Datepicker
            Clockpicker
            Footables
        End Enum


        Public Sub New()
            carpetaRecursos = "_recursos"
            agregarRecurso(recursos.Basicos)
            bll = New Logica
        End Sub


        Public Sub agregarRecurso(ByVal rec As Int16)
            Select Case rec
                Case recursos.Basicos
                    agregarRecurso(recursos.Jquery)
                    agregarRecurso(recursos.Bootstrap)
                    agregarRecurso(recursos.MagnificPopup)
                    agregarRecurso(recursos.Font_awesome)


                Case recursos.Jquery
                    agregarScript("js/jquery-1.11.2.min.js")

                Case recursos.JqueryUI
                    agregarEstilo("jqueryui/jquery-ui-1.11.4.custom/jquery-ui.min.css")
                    agregarScript("jqueryui/jquery-ui-1.11.4.custom/jquery-ui.min.js")
                    'agregarEstilo("http://code.jquery.com/ui/1.11.4/themes/smoothness/jquery-ui.css")
                    'agregarScript("http://code.jquery.com/ui/1.11.4/jquery-ui.js")


                Case recursos.Bootstrap
                    agregarEstilo("css/bootstrap-v3.3.4.min.css")
                    agregarScript("js/bootstrap-v3.3.4.min.js")

                Case recursos.Font_awesome
                    agregarEstilo("assets/font-awesome/css/font-awesome.css")


                Case recursos.MagnificPopup
                    agregarEstilo("assets/Magnific-Popup/magnific-popup.css")
                    agregarScript("assets/Magnific-Popup/jquery.magnific-popup.min.js")
                    agregarScript("assets/Magnific-Popup/config.js")

                Case recursos.Toastr
                    agregarEstilo("assets/toastr/build/toastr.min.css")
                    agregarScript("assets/toastr/build/toastr.min.js")

                Case recursos.Datatables
                    agregarEstilo("assets/DataTables/media/css/jquery.dataTables.min.css")
                    agregarEstilo("assets/DataTables/extensions/Responsive/css/dataTables.responsive.css")
                    agregarScript("assets/DataTables/media/js/jquery.dataTables.min.js")
                    agregarScript("assets/DataTables/extensions/Responsive/js/dataTables.responsive.min.js")


                Case recursos.CKEditor
                    agregarScript("assets/ckeditor/ckeditor.js")
                    agregarScript("assets/ckeditor/lang/es.js")

                Case recursos.Chosen
                    agregarEstilo("assets/chosen/chosen.css")
                    agregarScript("assets/chosen/chosen.jquery.min.js")

                Case recursos.Datepicker
                    agregarEstilo("assets/bootstrap-datepicker/css/datepicker.css")
                    agregarScript("assets/bootstrap-datepicker/js/bootstrap-datepicker.js")

                Case recursos.Clockpicker
                    agregarEstilo("assets/clockpicker/bootstrap-clockpicker.min.css")
                    agregarScript("assets/clockpicker/bootstrap-clockpicker.min.js")

                Case recursos.Footables
                    agregarEstilo("assets/FooTable-2/css/footable.core.min.css")
                    agregarScript("assets/FooTable-2/js/footable.js")
            End Select
        End Sub

        Public Sub agregarEstilo(ByVal e As String)
            If Not estilos.Contains(e) Then
                estilos.Add(e)
            End If
        End Sub

        Public Sub agregarScript(ByVal s As String)
            If Not scripts.Contains(s) Then
                scripts.Add(s)
            End If
        End Sub


        Public Sub publicarEstilos()

            For Each e As String In estilos
                Dim link As New HtmlLink
                Dim nl As New LiteralControl
                nl.Text = Environment.NewLine

                If e.Contains("app") Then
                    link.Attributes.Add("href", String.Format("~/{0}", e))
                ElseIf e.Contains("http") Then
                    link.Attributes.Add("href", e)
                Else
                    link.Attributes.Add("href", String.Format("~/{0}/{1}", carpetaRecursos, e))
                End If
                link.Attributes.Add("type", "text/css")
                link.Attributes.Add("rel", "stylesheet")
                If e.Contains("_print") Then
                    link.Attributes.Add("media", "print")
                End If
                Me.Header.Controls.Add(link)
                Me.Header.Controls.Add(nl)
            Next
        End Sub

        Public Sub publicarScripts()
            For Each s As String In scripts
                Dim script As New HtmlGenericControl("script")
                Dim nl As New LiteralControl
                nl.Text = Environment.NewLine
                If s.Contains("http") Then
                    script.Attributes.Add("src", s)
                Else
                    script.Attributes.Add("src", ResolveUrl(String.Format("~/{0}/{1}", carpetaRecursos, s)))
                End If

                Me.Header.Controls.Add(script)
                Me.Header.Controls.Add(nl)
            Next
        End Sub

        Public Sub Pagina_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete
            agregarEstilo("css/style.css")
            publicarEstilos()

            'agregar css para IE8<
            Dim ie8 As String = "<!--[if lt IE 9]> " _
                                & "<script src=""" & ResolveUrl("~/_recursos/js/html5shiv.js") & """></script> " _
                                & "<script src=""" & ResolveUrl("~/_recursos/js/respond.min.js") & """></script> " _
                                & "<link href=""" & ResolveUrl("~/_recursos/css/ie8.css") & """ rel=""stylesheet"" />" _
                                & " <![endif]-->" & Environment.NewLine
            Dim ie9 As String = "<!--[if lte IE 9]> " _
                                & "<script src=""" & ResolveUrl("~/_recursos/js/placeholders.min.js") & """></script>" _
                                & " <![endif]-->" & Environment.NewLine

            Me.Header.Controls.Add(New LiteralControl(ie8))
            Me.Header.Controls.Add(New LiteralControl(ie9))

            publicarScripts()
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub


        Public Overridable Sub ExportarXSLX(gv As GridView, nombreReporte As String)
            ' 1. Guardar el estado de paginación actual y desactivarla temporalmente
            Dim estabaPaginado As Boolean = gv.AllowPaging
            If estabaPaginado Then
                gv.AllowPaging = False
                ' Re-enlazar datos aquí si tu arquitectura lo requiere
            End If

            Dim dt As New DataTable(nombreReporte)
            Dim indicesAExcluir As New List(Of Integer)()

            ' ==============================================================
            ' 1. ANALIZAR Y CREAR COLUMNAS (RECORRIDO VERTICAL DE SEGURIDAD)
            ' ==============================================================
            Dim n As Int16 = 0
            For i As Integer = 0 To gv.HeaderRow.Cells.Count - 1
                Dim headercell As TableCell = gv.HeaderRow.Cells(i)

                If headercell.CssClass.Contains("noexportar") OrElse headercell.Text = "&nbsp;" Then
                    indicesAExcluir.Add(i)
                    Continue For
                End If

                Dim datacol As New DataColumn()

                ' --- ESCANEO INTELIGENTE DE TODA LA COLUMNA ---
                Dim esDecimal As Boolean = False
                Dim esEntero As Boolean = False

                ' Recorremos todas las filas del GridView para esta columna en específico
                For Each fila As GridViewRow In gv.Rows
                    If fila.RowType = DataControlRowType.DataRow Then
                        Dim textoCelda As String = fila.Cells(i).Text

                        ' Extraer texto si hay controles internos
                        If fila.Cells(i).Controls.Count > 0 Then
                            textoCelda = ""
                            For Each ctrl As Control In fila.Cells(i).Controls
                                If TypeOf ctrl Is Label Then textoCelda &= CType(ctrl, Label).Text
                                If TypeOf ctrl Is TextBox Then textoCelda &= CType(ctrl, TextBox).Text
                                If TypeOf ctrl Is Literal Then textoCelda &= CType(ctrl, Literal).Text
                            Next
                        End If

                        textoCelda = System.Net.WebUtility.HtmlDecode(textoCelda).Replace("&nbsp;", "").Trim()

                        ' Si la celda no está vacía, evaluamos su tipo numérico
                        If Not String.IsNullOrEmpty(textoCelda) Then
                            Dim dVal As Decimal
                            If Decimal.TryParse(textoCelda, dVal) Then
                                ' Si tiene puntos o comas lo marcamos como decimal prioritario
                                If textoCelda.Contains(",") OrElse textoCelda.Contains(".") Then
                                    esDecimal = True
                                    Exit For ' Encontró un decimal, no hace falta buscar más en esta columna
                                Else
                                    esEntero = True ' Es un número entero largo (como un ID o código)
                                End If
                            Else
                                ' Si encontramos un texto real (ej: "No aplica"), la columna DEBE ser String obligatoriamente
                                esDecimal = False
                                esEntero = False
                                Exit For
                            End If
                        End If
                    End If
                Next

                ' Asignar el tipo de datos final según el análisis de toda la columna
                If esDecimal Then
                    datacol.DataType = GetType(Decimal)
                ElseIf esEntero Then
                    datacol.DataType = GetType(Long)
                Else
                    datacol.DataType = GetType(String)
                End If
                ' ----------------------------------------------------

                Dim nombreColumna As String = System.Net.WebUtility.HtmlDecode(headercell.Text).Trim()
                If dt.Columns.Contains(nombreColumna) Then
                    datacol.ColumnName = nombreColumna & "_" & n.ToString()
                Else
                    datacol.ColumnName = nombreColumna
                End If

                dt.Columns.Add(datacol)
                n += 1
            Next

            ' ==============================================================
            ' 2. PROCESAR FILAS DE DATOS (CONVERSIÓN SEGURA CON TRYPARSE)
            ' ==============================================================
            For Each row As GridViewRow In gv.Rows
                If row.RowType = DataControlRowType.DataRow Then
                    Dim dr As DataRow = dt.NewRow()
                    Dim celdaDestino As Integer = 0

                    For i As Integer = 0 To row.Cells.Count - 1
                        If indicesAExcluir.Contains(i) Then Continue For

                        Dim valorCelda As String = ""

                        If row.Cells(i).Controls.Count > 0 Then
                            For Each ctrl As Control In row.Cells(i).Controls
                                If TypeOf ctrl Is Label Then valorCelda &= CType(ctrl, Label).Text
                                If TypeOf ctrl Is Literal Then valorCelda &= CType(ctrl, Literal).Text
                                If TypeOf ctrl Is TextBox Then valorCelda &= CType(ctrl, TextBox).Text
                            Next
                        End If

                        If String.IsNullOrEmpty(valorCelda) Then
                            valorCelda = row.Cells(i).Text
                        End If

                        valorCelda = System.Net.WebUtility.HtmlDecode(valorCelda).Replace("&nbsp;", "").Trim()
                        Dim tipoColumna As Type = dt.Columns(celdaDestino).DataType

                        ' Si la celda actual está vacía (o tiene texto inválido para el tipo numérico), ponemos DBNull
                        If String.IsNullOrEmpty(valorCelda) Then
                            dr(celdaDestino) = DBNull.Value
                        Else
                            ' Conversión controlada usando TryParse por fila para evitar caídas del sistema
                            If tipoColumna Is GetType(Decimal) Then
                                Dim vDec As Decimal
                                If Decimal.TryParse(valorCelda, vDec) Then dr(celdaDestino) = vDec Else dr(celdaDestino) = DBNull.Value
                            ElseIf tipoColumna Is GetType(Long) Then
                                Dim vLng As Long
                                If Long.TryParse(valorCelda, vLng) Then dr(celdaDestino) = vLng Else dr(celdaDestino) = DBNull.Value
                            Else
                                dr(celdaDestino) = valorCelda
                            End If
                        End If

                        celdaDestino += 1
                    Next
                    dt.Rows.Add(dr)
                End If
            Next

            ' Restaurar la paginación original
            If estabaPaginado Then gv.AllowPaging = True

            ' ==============================================================
            ' 3. GENERAR EL ARCHIVO CON CLOSEDXML
            ' ==============================================================
            Using wb As New XLWorkbook()
                Dim ws = wb.Worksheets.Add(dt)
                ws.Columns().AdjustToContents()

                Response.Clear()
                Response.ClearHeaders()
                Response.Buffer = True
                Response.Charset = "UTF-8"
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"

                Dim nombreArchivo As String = Regex.Replace(nombreReporte, "[^\w\s]", "").Replace(" ", "_").Trim()
                Response.AddHeader("content-disposition", String.Format("attachment; filename={0}.xlsx", nombreArchivo))

                Using MyMemoryStream As New MemoryStream()
                    wb.SaveAs(MyMemoryStream)
                    MyMemoryStream.WriteTo(Response.OutputStream)
                    Response.Flush()
                    Response.End()
                End Using
            End Using
        End Sub

        Private Sub ExportarXSLX_borrar(gv As GridView, nombreReporte As String)
            ' Exportar un gridview a XSLX
            ' https://www.aspsnippets.com/Articles/Solution-ASPNet-GridView-Export-to-Excel-The-file-you-are-trying-to-open-is-in-a-different-format-than-specified-by-the-file-extension.aspx
            ' Instalar OpenXMLSDKv2.msi que está en SAC/originales
            ' Instalar el pack ClosedXML via NuGet: PM> Install-Package ClosedXML

            ' agregar columnas teniendo en cuenta que las cabeceras vacío no se exportaran 
            Dim dt As New DataTable(nombreReporte)
            Dim n As Int16 = 0
            For Each headercell As TableCell In gv.HeaderRow.Cells
                Dim datacol As New DataColumn()
                datacol.ColumnName = System.Net.WebUtility.HtmlDecode(headercell.Text)
                If headercell.Text = "&nbsp;" Then
                    datacol.ColumnName = "noexportar" + n.ToString
                End If
                dt.Columns.Add(datacol)
                n += 1
            Next

            ' agrego las filas 
            For Each row As GridViewRow In gv.Rows
                dt.Rows.Add()
                For i As Integer = 0 To row.Cells.Count - 1
                    dt.Rows(dt.Rows.Count - 1)(i) = row.Cells(i).Text
                Next
            Next

            ' elimino las columnas que no se exportan
            For x As Int16 = dt.Columns.Count - 1 To 0 Step -1
                Dim col As DataColumn = dt.Columns(x)
                If col.ColumnName.Contains("noexportar") Then
                    dt.Columns.Remove(col)
                End If
            Next

            Using wb As New XLWorkbook()
                wb.Worksheets.Add(dt)

                Response.Clear()
                Response.Buffer = True
                Response.Charset = ""
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                Response.AddHeader("content-disposition", String.Format("attachment; filename={0}.xlsx", Regex.Replace(nombreReporte, "[^\w\s]", "").Replace(" ", "").Trim()))
                Using MyMemoryStream As New MemoryStream()
                    wb.SaveAs(MyMemoryStream)
                    MyMemoryStream.WriteTo(Response.OutputStream)
                    Response.Flush()
                    Response.End()
                End Using
            End Using
        End Sub


    End Class

    Public Class PaginaAnonimos
        Inherits Pagina


    End Class

    Public Class PaginaApp
        Inherits Pagina
        'todas las pagina de la aplicacion

        Public nombreUsuarioLogueado As String
        Public usuarioEsAdmin As Boolean
        Public usuarioEsAvanzado As Boolean
        Public usuarioEsBasico As Boolean
        Public cargaTareasDesde As Int16 ' cantidad de dias permitidos desde que un usuario puede ingresar tareas.
        Public cargaTareasHasta As Int16 ' cantidad de dias permitidos hasta un usuario puede ingresar tareas.
        Public maxDiasPermitidos As Int16 = 360
        Public notificacion As String
        Public tipoNotificacion As String
        Public tipoCalendario As Int16
        Public usuarios As Usuarios
        Private scriptFinal As String 'si en una pagina se quiere agregar un script antes de cerrar el <head>, se agrega a traves de agregarJavascript() en el Page_init

        Public Enum TiposNotificacion As Short
            NoInformar
            Informacion
            Advertencia
            Alerta
            OK
        End Enum

        Public Enum TiposCalendario
            Irrestricto 'se puede elegir cualquier fecha
            Restrictivo 'solo permite a los usuarios basicos seleccionar una fecha del mes vigente o del periodo determinado por el admin
        End Enum

        Public Sub New()
            nombreUsuarioLogueado = User.Identity.Name
            tipoNotificacion = TiposNotificacion.Informacion
            tipoCalendario = TiposCalendario.Irrestricto
            scriptFinal = String.Empty
            usuarios = New Usuarios
        End Sub

        Public Sub agregarJavascript(scrTxt As String)
            scriptFinal += scrTxt + Environment.NewLine
        End Sub

        Private Sub Notificar()
            Dim scriptText As String = String.Empty
            Dim tnotific As String = "info"
            Select Case tipoNotificacion
                Case TiposNotificacion.OK
                    tnotific = "success"
                Case TiposNotificacion.Advertencia
                    tnotific = "warning"
                Case TiposNotificacion.Alerta
                    tnotific = "error"
                Case TiposNotificacion.Informacion
                    tnotific = "info"
            End Select

            scriptText = String.Format("notificarEvento('{0}', '{1}');", notificacion, tnotific)

            ScriptManager.RegisterStartupScript(Me, Me.GetType, Guid.NewGuid.ToString, scriptText, True)

        End Sub

        Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
            usuarioEsAdmin = Roles.IsUserInRole(nombreUsuarioLogueado, Grupos.Administrador)
            usuarioEsAvanzado = Roles.IsUserInRole(nombreUsuarioLogueado, Grupos.Avanzado)
            usuarioEsBasico = Roles.IsUserInRole(nombreUsuarioLogueado, Grupos.Basico)
            Dim periodoCargas As Int16 = 0
            Dim _profile As ProfileCommon = CType(HttpContext.Current.Profile, ProfileCommon)

            Int32.TryParse(bll.OpcionConfig("PERIODO_CARGAS").Valor, periodoCargas)
            cargaTareasDesde = _profile.PeriodoAbierto
            cargaTareasHasta = 0


            If usuarioEsAdmin Then
                cargaTareasDesde = maxDiasPermitidos
                cargaTareasHasta = cargaTareasDesde
            Else
                If cargaTareasDesde = 0 Then
                    cargaTareasDesde = Date.Now.Day - 1
                    If Date.Now.Day < 8 Then
                        cargaTareasDesde += 7
                    End If
                End If
                If tipoCalendario = TiposCalendario.Irrestricto Then
                    cargaTareasDesde = maxDiasPermitidos
                    cargaTareasHasta = cargaTareasDesde
                End If
            End If

            If periodoCargas > cargaTareasDesde Then
                'si se ingresa un numero mayor en el admin
                cargaTareasDesde = periodoCargas
            End If
        End Sub

        Private Sub PaginaApp_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
            agregarRecurso(recursos.Font_awesome)
            agregarRecurso(recursos.Toastr)
            agregarScript("js/funciones.js")
            agregarScript("js/common-scripts.js")
        End Sub

        Private Sub PaginaApp_PreRenderComplete(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
            If tipoNotificacion <> TiposNotificacion.NoInformar And Not String.IsNullOrEmpty(notificacion) Then
                Notificar()
            End If

            'configuracion de calendario para seleccionar fecha
            If Not usuarioEsAdmin And tipoCalendario = TiposCalendario.Restrictivo Then
                agregarJavascript(String.Format("datepicker_settings.startDate = getPreviousDate({0}); datepicker_settings.endDate = getNextDate({1});", cargaTareasDesde, cargaTareasHasta))
            End If


            If Not String.IsNullOrEmpty(scriptFinal) Then
                Dim scriptTag As New HtmlGenericControl("script")
                scriptTag.Attributes.Add("type", "text/javascript")
                scriptTag.InnerHtml = scriptFinal
                Me.Header.Controls.Add(scriptTag)
            End If

        End Sub

        Public Sub ExportToExcel(gv As GridView, nombreReporte As String)
            'Queda como alias de ExportXLSX

            ExportarXSLX(gv, nombreReporte)
            Exit Sub






            Response.Clear()
            Response.Buffer = True
            Response.ContentType = "application/vnd.ms-excel"
            Response.AddHeader("content-disposition", String.Format("attachment; filename={0}.xls", Regex.Replace(nombreReporte, "[^\w\s]", "").Replace(" ", "").Trim()))
            Response.Charset = ""
            Me.EnableViewState = False

            Dim sw As System.IO.StringWriter = New System.IO.StringWriter()
            Dim htw As System.Web.UI.HtmlTextWriter = New System.Web.UI.HtmlTextWriter(sw)


            gv.RenderControl(htw)

            Response.Write(sw.ToString())
            Response.End()
        End Sub

        Public Overrides Sub VerifyRenderingInServerForm(control As Control)

        End Sub

        Public Sub cerrarSesion()
            FormsAuthentication.SignOut()
            Response.Redirect("~/")
        End Sub

    End Class

    Public Class PaginaAdmin
        Inherits PaginaApp

        'las paginas que permitan un ABM tendran estos Webcontrols
        Protected lb_nuevo_ctrl As LinkButton
        Protected lb_exportar_ctrl As LinkButton
        Protected gv_listado_ctrl As GridView
        Protected fv_abm_ctrl As FormView
        Protected cph_content As ContentPlaceHolder

        Private Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit

            Dim fileName As String = Path.GetFileName(Request.PhysicalPath)

            If Not Roles.IsUserInRole(nombreUsuarioLogueado, Grupos.Administrador) And fileName <> "reportes.aspx" Then
                Response.Write("Acceso no permitido")
                Response.End()
            End If
        End Sub

        Private Sub PaginaAdmin_Init(sender As Object, e As EventArgs) Handles Me.Init
            cph_content = Me.Master.FindControl("cph_contenido")
            gv_listado_ctrl = cph_content.FindControl("gv_listado")
            fv_abm_ctrl = cph_content.FindControl("fv_abm")
            lb_nuevo_ctrl = cph_content.FindControl("lb_nuevo")
            lb_exportar_ctrl = cph_content.FindControl("lb_exportar")
            If gv_listado_ctrl IsNot Nothing Then
                AddHandler gv_listado_ctrl.RowUpdated, AddressOf gv_listado_ctrl_RowUpdated
                AddHandler gv_listado_ctrl.RowDeleted, AddressOf gv_listado_ctrl_RowDeleted
            End If
            If fv_abm_ctrl IsNot Nothing Then
                fv_abm_ctrl.RenderOuterTable = False
                AddHandler fv_abm_ctrl.ItemInserted, AddressOf fv_abm_ctrl_ItemInserted
            End If
            If lb_nuevo_ctrl IsNot Nothing Then
                AddHandler lb_nuevo_ctrl.Click, AddressOf lb_nuevo_ctrl_Click
            End If
            If lb_exportar_ctrl IsNot Nothing Then
                AddHandler lb_exportar_ctrl.Click, AddressOf Exportar
            End If

        End Sub

        Private Sub PaginAdmin_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If fv_abm_ctrl IsNot Nothing Then
                fv_abm_ctrl.DataBind()
            End If
        End Sub

        Protected Sub lb_nuevo_ctrl_Click(sender As Object, e As EventArgs)
            mostrarABM()
        End Sub

        Private Sub fv_abm_ctrl_ItemInserted(sender As Object, e As System.Web.UI.WebControls.FormViewInsertedEventArgs)
            If e.Exception Is Nothing Then
                tipoNotificacion = TiposNotificacion.OK
                notificacion = Resources.Global.Registro_ingresado
                gv_listado_ctrl.DataBind()
                mostrarListado()
            Else
                tipoNotificacion = TiposNotificacion.Alerta
                notificacion = "ERROR: " + e.Exception.Message

                'e.ExceptionHandled = True
                'e.KeepInInsertMode = True
            End If
        End Sub

        Private Sub gv_listado_ctrl_RowUpdated(sender As Object, e As GridViewUpdatedEventArgs)
            If e.Exception Is Nothing Then
                tipoNotificacion = TiposNotificacion.OK
                notificacion = Resources.Global.Registro_actualizado
                gv_listado_ctrl.DataBind()
            Else
                tipoNotificacion = TiposNotificacion.Alerta
                notificacion = "ERROR. " + e.Exception.Message

                e.ExceptionHandled = True
                e.KeepInEditMode = True
            End If
        End Sub

        Private Sub gv_listado_ctrl_RowDeleted(sender As Object, e As System.Web.UI.WebControls.GridViewDeletedEventArgs)
            If e.Exception Is Nothing Then
                tipoNotificacion = TiposNotificacion.OK
                notificacion = Resources.Global.Registro_eliminado
                gv_listado_ctrl.DataBind()
            Else
                tipoNotificacion = TiposNotificacion.Alerta
                notificacion = "ERROR. " + e.Exception.Message

                e.ExceptionHandled = True
            End If

        End Sub

        Public Sub mostrarListado()
            gv_listado_ctrl.Visible = True

            If lb_exportar_ctrl IsNot Nothing Then
                lb_exportar_ctrl.Visible = True
            End If
            fv_abm_ctrl.ChangeMode(FormViewMode.ReadOnly)
        End Sub

        Public Sub mostrarABM()
            If gv_listado_ctrl IsNot Nothing Then
                gv_listado_ctrl.Visible = False
            End If
            If fv_abm_ctrl IsNot Nothing Then
                If fv_abm_ctrl.CurrentMode <> FormViewMode.Insert Then
                    fv_abm_ctrl.ChangeMode(FormViewMode.Insert)
                End If
            End If
            If lb_exportar_ctrl IsNot Nothing Then
                lb_exportar_ctrl.Visible = False
            End If
        End Sub

        Public Overridable Sub Exportar()
            gv_listado_ctrl.AllowPaging = False
            gv_listado_ctrl.PageSize = 8000
            gv_listado_ctrl.DataBind()
            ExportToExcel(gv_listado_ctrl, lb_exportar_ctrl.CommandArgument)
        End Sub


    End Class

    Public Class Utilidades

        Public Shared Function GenerarPassword(ByVal PasswordLength As Integer) As String
            Dim _allowedChars As String = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789"
            Dim randNum As New Random()
            Dim chars(PasswordLength - 1) As Char
            Dim allowedCharCount As Integer = _allowedChars.Length

            For i As Integer = 0 To PasswordLength - 1
                chars(i) = _allowedChars.Chars(CInt(Fix((_allowedChars.Length) * randNum.NextDouble())))
            Next i

            Return New String(chars)
        End Function

        Public Shared Function leerArchivo(ByVal pRutaFisica As String) As String
            'abrir un archivo para lectura
            Dim contenido As String = String.Empty
            Dim objStreamReader As System.IO.StreamReader
            If System.IO.File.Exists(pRutaFisica) Then
                objStreamReader = System.IO.File.OpenText(pRutaFisica)
                contenido = objStreamReader.ReadToEnd()
                objStreamReader.Close()
            End If
            Return contenido
        End Function
    End Class






    Public NotInheritable Class Grupos

        Public Shared Property Administrador As String = "Administrador"
        Public Shared Property Avanzado As String = "Avanzado"
        Public Shared Property Basico As String = "Basico"

    End Class
End Namespace
