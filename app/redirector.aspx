<%@ Page Language="VB" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim inactivo As Boolean = False 'si un usuario está inactivo en Negocios
        Dim lineasActivas As String = String.Empty ' lineas que tiene el usuario (S:LES, N:LEN, T:Ambas, X:No tiene)
        Dim relacion As Char = String.Empty 'tipo de relacion del usuario: C(Cliente), A(Asesor), D(Dealer), I(Uso interno)
        Dim urlDestino As String = String.Empty
        'Dim bll As New Logica
        Dim dt As DataTable = Nothing
        
        
        urlDestino = "cargar_horas.aspx"
        
        
            
            
        
        
        
        
        If Not String.IsNullOrEmpty(urlDestino) Then
            Response.Redirect(urlDestino)
        End If
    End Sub
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
