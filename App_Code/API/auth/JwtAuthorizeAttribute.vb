Imports System.Web.Http
Imports System.Web.Http.Controllers
Imports System.Net
Imports System.Net.Http
Imports System.IdentityModel.Tokens.Jwt
Imports Microsoft.IdentityModel.Tokens
Imports System.Security.Claims
Imports System.Text
Imports System.Configuration
Imports System.Threading
Imports System.Web

Namespace Auth

    Public Class JwtAuthorizeAttribute
        Inherits AuthorizeAttribute

        Public Overrides Sub OnAuthorization(actionContext As HttpActionContext)
            ' 1. Obtener la cabecera Authorization
            Dim authHeader = actionContext.Request.Headers.Authorization

            ' Validar que la cabecera exista y empiece por 'Bearer '
            If authHeader Is Nothing OrElse authHeader.Scheme <> "Bearer" OrElse String.IsNullOrEmpty(authHeader.Parameter) Then
                ReturnUnauthorized(actionContext, "Acceso no autorizado: Token no proporcionado.")
                Return
            End If

            Dim tokenString As String = authHeader.Parameter

            Try
                ' 2. Leer credenciales desde el web.config
                Dim secretKey As String = ConfigurationManager.AppSettings("JwtSecretKey")
                Dim issuer As String = ConfigurationManager.AppSettings("JwtIssuer")
                Dim audience As String = ConfigurationManager.AppSettings("JwtAudience")

                Dim keyBytes = Encoding.UTF8.GetBytes(secretKey)
                Dim symmetricKey = New SymmetricSecurityKey(keyBytes)

                ' 3. Definir reglas de validación del token
                Dim validationParameters As New TokenValidationParameters With {
                .ValidateIssuerSigningKey = True,
                .IssuerSigningKey = symmetricKey,
                .ValidateIssuer = True,
                .ValidIssuer = issuer,
                .ValidateAudience = True,
                .ValidAudience = audience,
                .ValidateLifetime = True,
                .ClockSkew = TimeSpan.FromMinutes(5)
            }
                ' Margen de tolerancia para diferencia de reloj

                ' 4. Validar firma y expiración del Token
                Dim handler As New JwtSecurityTokenHandler()
                Dim securityToken As SecurityToken = Nothing
                Dim principal As ClaimsPrincipal = handler.ValidateToken(tokenString, validationParameters, securityToken)

                ' 5. Establecer el usuario autenticado en el contexto de Web API y HTTP
                Thread.CurrentPrincipal = principal
                actionContext.RequestContext.Principal = principal

                If HttpContext.Current IsNot Nothing Then
                    HttpContext.Current.User = principal
                End If

                ' Permitir que la ejecución continúe hacia el Controlador
                MyBase.OnAuthorization(actionContext)

            Catch ex As SecurityTokenExpiredException
                ReturnUnauthorized(actionContext, "El token ha expirado. Debe iniciar sesión nuevamente.")
            Catch ex As Exception
                ReturnUnauthorized(actionContext, "Token de autorización inválido.")
            End Try
        End Sub

        Private Sub ReturnUnauthorized(actionContext As HttpActionContext, message As String)
            actionContext.Response = actionContext.Request.CreateResponse(
            HttpStatusCode.Unauthorized,
            New With {
                .status = "Error",
                .mensaje = message
            }
        )
        End Sub
    End Class
End Namespace