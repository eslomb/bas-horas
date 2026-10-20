Imports Microsoft.VisualBasic
Imports System.ComponentModel

Namespace Controles


    Public Class SubmitButton
        Inherits LinkButton

        Private Sub SubmitButton_Init(sender As Object, e As EventArgs) Handles Me.Init
            Me.Attributes.Add("data-loading-text", "Un momento...")
            Me.Attributes.Add("autocomplete", "off")
            Me.CssClass = "btn btn-success btn-submit"
        End Sub


    End Class

    Public Class GridView
        Inherits UI.WebControls.GridView

        Public EsDatatable As Boolean

        Public Sub New()
            EsDatatable = False
        End Sub
        Private Sub DataTable_Init(sender As Object, e As EventArgs) Handles Me.Init

            If EsDatatable Then
                'si se usa con jquery Datatables
                Me.AllowPaging = False
                Me.AllowSorting = False
                Me.GridLines = WebControls.GridLines.None
                Me.CssClass = "table table-striped dataTable"
                Me.EnableViewState = False
            Else
                Me.AllowPaging = True
                Me.AllowSorting = True
                Me.GridLines = WebControls.GridLines.None
                Me.CssClass = "table table-striped"
                Me.PagerStyle.CssClass = "paginador"
                Me.SortedAscendingHeaderStyle.CssClass = "paginador-sort-asc"
                Me.SortedDescendingHeaderStyle.CssClass = "paginador-sort-desc"

            End If

        End Sub

        Private Sub DataTable_Load(sender As Object, e As EventArgs) Handles Me.Load
            If EsDatatable Then
                'si se usa con jquery Datatables

                Dim pag As PaginaApp = CType(Me.Page, PaginaApp)
                If Not pag Is Nothing Then
                    pag.agregarRecurso(pag.recursos.Datatables)
                End If
            End If
        End Sub


        Private Sub DataTable_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
            If Me.Rows.Count > 0 Then
                Me.HeaderRow.TableSection = TableRowSection.TableHeader
            End If
        End Sub

    End Class



    Public Class CheckboxList
        Inherits UI.WebControls.CheckBoxList

        Private Sub CheckboxList_Init(sender As Object, e As EventArgs) Handles Me.Init
            Me.CssClass = "input-awesome"
            Me.RepeatLayout = WebControls.RepeatLayout.Flow
        End Sub
    End Class

    Public Class Checkbox
        Inherits UI.WebControls.CheckBox

        Private Sub Checkbox_Init(sender As Object, e As EventArgs) Handles Me.Init
            Me.CssClass = "input-awesome"
        End Sub
    End Class

    Public Class RadioButtonList
        Inherits UI.WebControls.RadioButtonList


        Private Sub RadioButtonList_Init(sender As Object, e As EventArgs) Handles Me.Init
            Me.CssClass = "input-awesome"
            Me.RepeatLayout = WebControls.RepeatLayout.Flow
        End Sub
    End Class

    Public Class RadioButton
        Inherits UI.WebControls.RadioButton

        Private Sub RadioButton_Init(sender As Object, e As EventArgs) Handles Me.Init
            Me.CssClass = "input-awesome"
        End Sub
    End Class

    Public Class Datepicker
        Inherits TextBox

        Private Sub Datepicker_Init(sender As Object, e As EventArgs) Handles Me.Init
            If Me.CssClass = "" Then
                Me.CssClass = "form-control default-date-picker"
            End If

            Me.MaxLength = "10"

            Dim pag As PaginaApp = CType(Me.Page, PaginaApp)
            If Not pag Is Nothing Then
                pag.agregarRecurso(pag.recursos.Datepicker)
            End If
        End Sub
    End Class

    Public Class Calendar
        Inherits System.Web.UI.WebControls.Calendar

        Public Sub New()

        End Sub

        Private Sub Calendar_Init(sender As Object, e As EventArgs) Handles Me.Init
            Me.CssClass = "calendar"
            Me.NextPrevFormat = WebControls.NextPrevFormat.ShortMonth
            Me.DayStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#990000")
            Me.SelectedDayStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#990000")
            Me.SelectedDayStyle.ForeColor = Drawing.Color.White
            Me.OtherMonthDayStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#999999")
            Me.NextPrevStyle.Font.Bold = False
            Me.NextPrevStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#808080")
            Me.DayHeaderStyle.Font.Bold = False
            Me.TitleStyle.Font.Bold = False
            Me.TitleStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#f3f3f3")

        End Sub
    End Class

    Public Class PasosDeInforme
        Inherits Web.UI.HtmlControls.HtmlGenericControl

        'Usado en el formulario de Informe de Servicios
        'el resultado final es:
        '<div class="stepy-tab screen">
        '    <ul class="stepy-titles clearfix">
        '        <li class="current-step">
        '            <div>Cargar</div>
        '        </li>
        '        <li>
        '            <div>Verificar</div>
        '        </li>
        '        <li>
        '            <div>Finalizar</div>
        '        </li>
        '    </ul>
        '</div>
        Public PasoActual As Int16
        Public TextoPaso1 As String
        Public TextoPaso2 As String
        Public TextoPaso3 As String


        Public Sub New(tag As String)
            Me.TagName = "div"
            PasoActual = 1
            TextoPaso1 = "Cargar"
            TextoPaso2 = "Verificar"
            TextoPaso3 = "Finalizar"
        End Sub


        Private Sub PasosDeInforme_Load(sender As Object, e As EventArgs) Handles Me.Load
            Me.Attributes.Add("class", "stepy-tab screen")
            Dim sb As New StringBuilder
            Dim txt As String = TextoPaso1

            sb.Append("<ul class=""stepy-titles clearfix"">")
            For i As Int16 = 1 To 3
                Select Case i
                    Case 2
                        txt = TextoPaso2
                    Case 3
                        txt = TextoPaso3
                End Select

                sb.Append("<li")
                If i = PasoActual Then
                    sb.Append(" class=""current-step""")
                End If

                sb.AppendFormat("><div>{0}</div>", txt)

                sb.Append("</li>")
            Next
            sb.Append("</ul>")
            Me.InnerHtml = sb.ToString
        End Sub

    End Class



    ' si quiero usar un Templated Control
    Public Class Message
        Inherits System.Web.UI.WebControls.Panel
        Implements System.Web.UI.INamingContainer
        Private _name As String
        Private _text As String
        Public Sub New(ByVal name As String, ByVal text As String)
            _text = text
            _name = name
        End Sub
        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property
        Public ReadOnly Property Text() As String
            Get
                Return _text
            End Get
        End Property
    End Class

    <DefaultProperty("Text")> _
    <ToolboxData("<{0}:TemplatedControl runat=server></{0}:TemplatedControl>")> _
    Public Class TemplatedControl
        Inherits System.Web.UI.WebControls.WebControl
        Private _name As String
        Private _text As String
        Private _message As Message
        Private _messageTemplate As ITemplate

        <Browsable(True)> Public ReadOnly Property Message() As Message
            Get
                EnsureChildControls()
                Return _message
            End Get
        End Property

        <PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(GetType(Message))> _
        Public Property MessageTemplate() As ITemplate
            Get
                Return _messageTemplate
            End Get
            Set(ByVal value As ITemplate)
                _messageTemplate = value
            End Set
        End Property

        <Bindable(True), DefaultValue("")> Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        <Bindable(True), DefaultValue("")> Public Property Text() As String
            Get
                Return _text
            End Get
            Set(ByVal value As String)
                _text = value
            End Set
        End Property

        Public Overrides Sub DataBind()
            CreateChildControls()
            ChildControlsCreated = True
            MyBase.DataBind()
        End Sub

        Protected Overrides Sub CreateChildControls()
            Me.Controls.Clear()
            _message = New Message(Name, Text)
            Dim template As ITemplate = MessageTemplate
            template.InstantiateIn(_message)
            Controls.Add(_message)
        End Sub

    End Class


End Namespace
