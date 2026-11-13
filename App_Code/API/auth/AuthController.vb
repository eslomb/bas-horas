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

        Private ReadOnly servicioRecuperacion As IRecuperacionPasswordService

        Public Sub New(servicioRecuperacion As IRecuperacionPasswordService)
            Me.servicioRecuperacion = servicioRecuperacion
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

            ' Clave corta que nadie cambió desde la migración a Hashed (solo con ForzarCambioPasswordCorta=true):
            ' no se entrega JWT, sino un token para la pantalla de restablecer contraseña.
            If servicioRecuperacion.RequiereCambioObligatorio(userName, request.Password) Then
                Return Content(HttpStatusCode.Forbidden, New With {
                    .mensaje = "Tenés que cambiar tu contraseña antes de ingresar.",
                    .codigo = "CambioPasswordRequerido",
                    .tokenCambio = servicioRecuperacion.EmitirTokenCambioObligatorio(userName)
                })
            End If

            Dim tokenString As String = GenerarTokenJWT(userName)

            Return Ok(New With {
            .token = tokenString,
            .username = userName
        })
        End Function

        <HttpPost>
        <Route("api/auth/sso")>
        Public Function Sso() As IHttpActionResult
            ' sirve para autenticar desde Nuxt con JWT, cuando se loguea con ASP, y luego quiere acceder a Nuxt
            ' identidad que dejó FormsAuthenticationModule a partir de .ASPXAUTH
            Dim identidad = HttpContext.Current.User.Identity
            If identidad Is Nothing OrElse Not identidad.IsAuthenticated Then
                Return Unauthorized()
            End If

            Dim userName As String = identidad.Name
            Dim usuario = Membership.GetUser(userName)
            If usuario Is Nothing OrElse usuario.IsLockedOut OrElse Not usuario.IsApproved Then
                Return Unauthorized()
            End If

            Return Ok(New With {
                .token = GenerarTokenJWT(userName),
                .username = userName
            })
        End Function

        ' Un administrador entra como otro usuario sin su contraseña. El token lleva el claim "act" con el
        ' administrador y dura menos que el normal. Volver a la cuenta propia lo resuelve el frontend con su token original.
        <HttpPost>
        <JwtAuthorize(Roles:="Administrador")>
        <Route("api/auth/suplantar")>
        Public Function Suplantar(<FromBody> request As SuplantarInputDTO) As IHttpActionResult
            If request Is Nothing OrElse String.IsNullOrWhiteSpace(request.UserName) Then
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "Debe indicar el usuario."})
            End If

            Dim admin As String = User.Identity.Name
            If Not PuedeSuplantar(admin) Then
                Return Content(HttpStatusCode.Forbidden, New With {.mensaje = "No tiene permiso para entrar como otro usuario."})
            End If
            If String.Equals(request.UserName.Trim(), admin, StringComparison.OrdinalIgnoreCase) Then
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "No puede entrar como su propio usuario."})
            End If

            Dim usuario = Membership.GetUser(request.UserName.Trim())
            If usuario Is Nothing Then
                Return Content(HttpStatusCode.NotFound, New With {.mensaje = "El usuario no existe."})
            End If
            If Not usuario.IsApproved OrElse usuario.IsLockedOut Then
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "El usuario " & usuario.UserName & " está deshabilitado o bloqueado."})
            End If
            If Roles.IsUserInRole(usuario.UserName, Aplicacion.Grupos.Administrador) Then
                Return Content(HttpStatusCode.Forbidden, New With {.mensaje = "No se puede entrar como otro administrador."})
            End If

            Return Ok(New With {
                .token = GenerarTokenJWT(usuario.UserName, admin, HorasSuplantacion),
                .username = usuario.UserName
            })
        End Function

        ' appSettings "UsuariosSuplantacion": administradores habilitados, separados por coma.
        ' Vacío o ausente = cualquier administrador.
        Private Shared Function PuedeSuplantar(userName As String) As Boolean
            Dim lista As String = ConfigurationManager.AppSettings("UsuariosSuplantacion")
            If String.IsNullOrWhiteSpace(lista) Then Return True
            Return lista.Split(","c).Any(Function(u) String.Equals(u.Trim(), userName, StringComparison.OrdinalIgnoreCase))
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
            If request Is Nothing OrElse String.IsNullOrWhiteSpace(request.Email) Then
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "Debe proporcionar el email."})
            End If

            Try
                servicioRecuperacion.Solicitar(request.Email)
            Catch
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = "No se pudo procesar el pedido. Intentá nuevamente más tarde."})
            End Try

            ' Misma respuesta exista o no el email, para no revelar qué cuentas existen.
            Return Ok(New With {.mensaje = "Si el email está registrado, te enviamos un link para restablecer la contraseña."})
        End Function

        <HttpPost>
        <Route("api/auth/restablecer-password/validar")>
        Public Function ValidarTokenRestablecer(<FromBody> request As ValidarTokenInputDTO) As IHttpActionResult
            If request Is Nothing OrElse Not servicioRecuperacion.Validar(request.Token) Then
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = MensajeTokenInvalido})
            End If
            Return Ok()
        End Function

        <HttpPost>
        <Route("api/auth/restablecer-password")>
        Public Function RestablecerPassword(<FromBody> request As RestablecerPasswordInputDTO) As IHttpActionResult
            If request Is Nothing Then
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "Debe enviar el token y la nueva contraseña."})
            End If

            Select Case servicioRecuperacion.Restablecer(request.Token, request.Password)
                Case ResultadoRestablecerPassword.Ok
                    Return Ok(New With {.mensaje = "La contraseña se cambió. Ya podés ingresar con la nueva."})
                Case ResultadoRestablecerPassword.TokenInvalido
                    Return Content(HttpStatusCode.BadRequest, New With {.mensaje = MensajeTokenInvalido})
                Case ResultadoRestablecerPassword.PasswordNoValida
                    Return Content(HttpStatusCode.BadRequest, New With {.mensaje = String.Format(
                        "La contraseña no cumple con la política requerida (mínimo {0} caracteres).",
                        Membership.MinRequiredPasswordLength)})
                Case Else
                    Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = "No se pudo cambiar la contraseña. Pedí un link nuevo."})
            End Select
        End Function

        Private Const MensajeTokenInvalido As String = "El link no es válido, ya se usó o venció. Pedí uno nuevo."

        ' el frontend vuelve solo a la cuenta del administrador al mismo plazo
        Private Const HorasSuplantacion As Integer = 6

        ' actor: administrador que entra como `username` (claim "act"); horas: vigencia del token
        Private Function GenerarTokenJWT(username As String, Optional actor As String = Nothing, Optional horas As Integer = 8) As String
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

            If Not String.IsNullOrEmpty(actor) Then
                claims.Add(New Claim("act", actor))
            End If

            If Roles.Enabled Then
                Dim userRoles = Roles.GetRolesForUser(username)
                For Each userRole In userRoles
                    claims.Add(New Claim(ClaimTypes.Role, userRole))
                Next
            End If

            ' 4. Crear el descriptor del Token con los datos del web.config
            Dim tokenDescriptor As New SecurityTokenDescriptor With {
        .Subject = New ClaimsIdentity(claims),
        .Expires = DateTime.UtcNow.AddHours(horas),
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