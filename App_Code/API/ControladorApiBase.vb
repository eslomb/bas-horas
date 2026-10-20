Imports System.Web.Http
Imports System.Security.Claims

Public MustInherit Class ControladorApiBase
    Inherits ApiController

    Protected Function ObtenerUsuarioActual() As String
        Dim claimsPrincipal = TryCast(User, ClaimsPrincipal)
        Return If(claimsPrincipal IsNot Nothing AndAlso claimsPrincipal.Identity IsNot Nothing, claimsPrincipal.Identity.Name, Nothing)
    End Function
End Class
