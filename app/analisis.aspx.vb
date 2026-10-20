
Partial Class app_analisis
    Inherits PaginaApp

    Protected NombrePrimerColumna As String

    Private _concepto As String
    Public Property Concepto() As String
        Get
            Return _concepto
        End Get
        Set(ByVal value As String)
            _concepto = value
        End Set
    End Property

    Protected Sub Page_Init1(sender As Object, e As EventArgs) Handles Me.Init
        Concepto = IIf(Request.QueryString("concepto") <> "", Request.QueryString("concepto"), "producto")
        agregarRecurso(recursos.Footables)
    End Sub


    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Dim ahora As DateTime = Date.Now
            tb_desde.Text = String.Format("{0}/{1}/{2}", "01", ahora.Month.ToString.PadLeft(2, "0"), ahora.Year)
            filtrar()
        End If
    End Sub


    Private Sub filtrar()
        Dim an As New Analisis()
        Dim conceptos As New List(Of Conceptos)

        If Concepto.Equals("producto") Then
            Dim productos As List(Of PRODUCTOS) = bll.listarProductosVisibles.ToList
            For Each p In productos
                Dim c As New Conceptos
                c.ID = p.IDPro
                c.Concepto = p.Producto
                conceptos.Add(c)
            Next
            NombrePrimerColumna = "Producto"
        End If

        If Concepto.Equals("pais") Then
            Dim paises As List(Of PAISES) = bll.listarPaises.ToList
            For Each p In paises
                Dim c As New Conceptos
                c.ID = p.Codigo
                c.Concepto = p.Pais
                conceptos.Add(c)
            Next
            NombrePrimerColumna = "País"
        End If
        If Concepto.Equals("usuario") Then
            Dim usuarios As MembershipUserCollection = bll.listarUsuarios
            For Each usr As MembershipUser In usuarios
                Dim c As New Conceptos
                c.ID = usr.UserName
                c.Concepto = c.ID
                conceptos.Add(c)
            Next
            NombrePrimerColumna = "Usuario"
        End If

        an.inicializar(conceptos, bll.listarActividades.ToList)
        an.cargarDatos(bll.analisisActividades(Concepto, _
                                               convertirNulos(tb_desde.Text), _
                                               convertirNulos(tb_hasta.Text)))
        gvReporte.DataSource = an.datosFinales()
        gvReporte.DataBind()

        Dim i As Int16 = 0
        For Each tcel As TableCell In gvReporte.HeaderRow.Cells
            If i > 0 Then
                tcel.Attributes.Add("data-hide", "phone")
            End If
            If i > 7 Then
                tcel.Attributes.Add("data-hide", "phone,tablet")
            End If
            If i > 12 Then
                tcel.Attributes.Add("data-hide", "all")
            End If
            i += 1

        Next

    End Sub

    Protected Sub lb_filtrar_Click(sender As Object, e As EventArgs) Handles lb_filtrar.Click
        filtrar()
    End Sub

    Public Function convertirNulos(str As String) As Object
        Return IIf(String.IsNullOrEmpty(str), Nothing, str)
    End Function

    Protected Sub lb_exportar_Click(sender As Object, e As EventArgs) Handles lb_exportar.Click
        Me.ExportToExcel(gvReporte, "Analisis_Actividades")
    End Sub

    Protected Sub gvReporte_DataBound(sender As Object, e As EventArgs) Handles gvReporte.DataBound
        Dim gv As GridView = CType(sender, GridView)
        gv.HeaderRow.Cells(0).Text = NombrePrimerColumna
    End Sub

    Protected Sub gvReporte_PreRender(sender As Object, e As EventArgs) Handles gvReporte.PreRender
        Dim gv As GridView = CType(sender, GridView)
        If gv.Rows.Count > 0 Then
            gv.HeaderRow.TableSection = TableRowSection.TableHeader
        End If
    End Sub
End Class


Public Class Analisis
    Private actividades As ACTIVIDADES
    Private horasCalculadas As spAnalisisActividades_Result
    Private dtReporte As DataTable

    Public Sub New()
        dtReporte = New DataTable
        dtReporte.Columns.Add(New DataColumn("ID", GetType(String)))
        dtReporte.Columns.Add(New DataColumn("Concepto", GetType(String)))
    End Sub

    Public Sub inicializar(conceptos As List(Of Conceptos), listaActividades As List(Of ACTIVIDADES))
        For Each a As ACTIVIDADES In listaActividades
            'agregar columnas de actividades
            dtReporte.Columns.Add(New DataColumn(a.Codigo, GetType(String)))
        Next

        For Each c In conceptos
            'agregar filas según el concepto
            dtReporte.Rows.Add(c.ID, c.Concepto)
        Next
    End Sub

    Public Sub cargarDatos(reporte As List(Of spAnalisisActividades_Result))
        For Each r In reporte.ToList
            Dim i As Int32 = -1
            Dim fila As Int16 = -1
            For Each dr As DataRow In dtReporte.Rows
                If dr("ID") = r.ID.Trim Then
                    fila = i + 1
                    Exit For
                End If
                i += 1
            Next
            If fila >= 0 Then
                'solo muestra datos de filas encontradas
                dtReporte.Rows(fila)(r.CodActiv) = r.Recuento
            End If
        Next
    End Sub

    Public Function datosFinales() As DataTable
        dtReporte.Columns.RemoveAt(0)
        Return dtReporte
    End Function
    
End Class

Public Class Conceptos
    Public ID As String
    Public Concepto As String
End Class