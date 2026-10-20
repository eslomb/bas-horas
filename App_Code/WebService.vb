Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Web.Script.Services

<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class WebService
    Inherits System.Web.Services.WebService


    <WebMethod> _
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)> _
    Public Function Actividades(pre As String) As List(Of String)
        Dim cntx As New BASHorasEntities
        Dim strList As New List(Of String)
        Dim lista = From p In cntx.ACTIVIDADES
                    Order By p.Actividad
                    Where (p.Actividad.Contains(pre) Or p.Codigo.Contains(pre)) And p.Habilitada = True
                    Select New With {.Actividad = "(" & p.Codigo & ") " & p.Actividad.Trim & "|" & p.IDAct.ToString}
        'Select New With {.Actividad = p.Actividad.Trim & "-" & p.IDAct.ToString}


        For Each obj In lista
            strList.Add(obj.Actividad)
        Next

        Return strList
    End Function

    <WebMethod> _
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)> _
    Public Function Productos(pre As String) As List(Of String)
        Dim cntx As New BASHorasEntities
        Dim strList As New List(Of String)
        Dim lista = (From p In cntx.PRODUCTOS
                     Order By p.Producto
                    Where p.Producto.Contains(pre) And p.Activo
                    Select New With {.Producto = p.Producto.Trim & "|" & p.IDPro})

        For Each obj In lista
            strList.Add(obj.Producto)
        Next

        Return strList
    End Function


End Class