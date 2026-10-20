Imports System.Web.Http
Imports System.Web.Security
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Microsoft.IdentityModel.Tokens
Imports System.Text
Imports System.Net
Imports DTOs.Usuarios
Imports Services.Interfaces

Namespace Auth

    Public Class AuthController
        Inherits ApiController

        Private ReadOnly servicioUsuarios As IUsuarioService

        Public Sub New(servicioUsuarios As IUsuarioService)
            Me.servicioUsuarios = servicioUsuarios
        End Sub

        <HttpPost>
        <Route("api/auth/login")>
        Public Function Login(<FromBody> request As LoginRequest) As IHttpActionResult

            If request Is Nothing OrElse String.IsNullOrEmpty(request.Email) OrElse String.IsNullOrEmpty(request.Password) Then
                Return BadRequest("Debe proporcionar usuario y contraseña.")
            End If

            Dim userName As String = Membership.GetUserNameByEmail(request.Email)

            ' Validar credenciales usando la clase Membership tradicional
            Dim isValid As Boolean = Membership.ValidateUser(userName, request.Password)

            If Not isValid Then
                Return Unauthorized()
            End If

            Dim tokenString As String = GenerarTokenJWT(userName)

            Return Ok(New With {
            .token = tokenString,
            .username = userName
        })
        End Function

        <HttpPost>
        <JwtAuthorize>
        <Route("api/auth/refresh")>
        Public Function RefreshToken() As IHttpActionResult
            ' 1. Extraer el nombre de usuario desde el token actual (gracias al filtro JwtAuthorize)
            Dim claimsPrincipal = TryCast(User, ClaimsPrincipal)
            'Dim username As String = claimsPrincipal?.Identity?.Name
            Dim username As String = If(claimsPrincipal IsNot Nothing AndAlso claimsPrincipal.Identity IsNot Nothing, claimsPrincipal.Identity.Name, Nothing)



            If String.IsNullOrEmpty(username) Then
                Return Unauthorized()
            End If

            ' 2. Opcional: Verificar que el usuario siga activo en Membership
            Dim userObj = Membership.GetUser(username)
            If userObj Is Nothing OrElse userObj.IsLockedOut OrElse Not userObj.IsApproved Then
                Return Unauthorized()
            End If

            ' 3. Generar un NUEVO token JWT con una nueva fecha de expiración
            Dim nuevoToken As String = GenerarTokenJWT(username)

            ' 4. Devolver el nuevo token
            Return Ok(New With {
            .token = nuevoToken,
            .username = username
        })
        End Function

        <HttpPost>
        <Route("api/auth/recuperar-password")>
        Public Function RecuperarPassword(<FromBody> request As RecuperarPasswordInputDTO) As IHttpActionResult
            If request Is Nothing OrElse String.IsNullOrEmpty(request.Email) Then
                Return BadRequest("Debe proporcionar el email.")
            End If

            Dim resultado = servicioUsuarios.RecuperarPassword(request.Email)

            Select Case resultado
                Case ResultadoRecuperarPassword.Ok
                    Return Ok(New With {.mensaje = "Se envió una nueva contraseña al email registrado."})
                Case ResultadoRecuperarPassword.EmailNoEncontrado
                    Return Content(HttpStatusCode.NotFound, New With {.mensaje = "No existe un usuario con ese email."})
                Case ResultadoRecuperarPassword.UsuarioInactivo
                    Return Content(HttpStatusCode.Forbidden, New With {.mensaje = "El usuario no está activo."})
                Case ResultadoRecuperarPassword.CuentaBloqueada
                    Return Content(HttpStatusCode.Forbidden, New With {.mensaje = "La cuenta está bloqueada."})
                Case Else
                    Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = "No se pudo generar la nueva contraseña."})
            End Select
        End Function

        Private Function GenerarTokenJWT(username As String) As String
            ' 1. Leer valores desde appSettings en web.config
            Dim secretKey As String = ConfigurationManager.AppSettings("JwtSecretKey")
            Dim issuer As String = ConfigurationManager.AppSettings("JwtIssuer")
            Dim audience As String = ConfigurationManager.AppSettings("JwtAudience")

            ' Validar que la clave exista
            If String.IsNullOrEmpty(secretKey) Then
                Throw New InvalidOperationException("La clave 'JwtSecretKey' no está configurada en el web.config.")
            End If

            ' 2. Convertir la clave a bytes
            Dim keyBytes = Encoding.UTF8.GetBytes(secretKey)
            Dim symmetricKey = New SymmetricSecurityKey(keyBytes)
            Dim signingCredentials = New SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256)

            ' 3. Definir Claims (Roles e Identidad)
            Dim claims As New List(Of Claim) From {
        New Claim(ClaimTypes.Name, username),
        New Claim(ClaimTypes.NameIdentifier, username),
        New Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    }

            If Roles.Enabled Then
                Dim userRoles = Roles.GetRolesForUser(username)
                For Each userRole In userRoles
                    claims.Add(New Claim(ClaimTypes.Role, userRole))
                Next
            End If

            ' 4. Crear el descriptor del Token con los datos del web.config
            Dim tokenDescriptor As New SecurityTokenDescriptor With {
        .Subject = New ClaimsIdentity(claims),
        .Expires = DateTime.UtcNow.AddHours(8),
        .SigningCredentials = signingCredentials,
        .Issuer = issuer,
        .Audience = audience
    }

            Dim tokenHandler As New JwtSecurityTokenHandler()
            Dim token = tokenHandler.CreateToken(tokenDescriptor)

            Return tokenHandler.WriteToken(token)
        End Function

    End Class
End Namespace