Imports System.Net
Imports System.Net.Http
Imports System.Web.Http.Controllers
Imports Utils

Namespace Auth

    ' JWT + rol Administrador + usuario con acceso a remuneraciones (Utils.AccesoRemuneraciones).
    ' Quien está autenticado pero no cumple recibe 403 (el AuthorizeAttribute base respondería 401,
    ' que el frontend interpreta como sesión vencida).
    Public Class RemuneracionesAuthorizeAttribute
        Inherits JwtAuthorizeAttribute

        Public Sub New()
            Roles = "Administrador"
        End Sub

        Protected Overrides Function IsAuthorized(actionContext As HttpActionContext) As Boolean
            If Not MyBase.IsAuthorized(actionContext) Then Return False

            Dim principal = actionContext.RequestContext.Principal
            Return principal IsNot Nothing AndAlso principal.Identity IsNot Nothing AndAlso
                   AccesoRemuneraciones.EsNivelFull(principal.Identity.Name)
        End Function

        Protected Overrides Sub HandleUnauthorizedRequest(actionContext As HttpActionContext)
            Dim principal = actionContext.RequestContext.Principal

            If principal Is Nothing OrElse principal.Identity Is Nothing OrElse Not principal.Identity.IsAuthenticated Then
                MyBase.HandleUnauthorizedRequest(actionContext)
                Return
            End If

            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Forbidden,
                New With {
                    .status = "Error",
                    .mensaje = "No tiene permisos para gestionar remuneraciones."
                })
        End Sub
    End Class
End Namespace
