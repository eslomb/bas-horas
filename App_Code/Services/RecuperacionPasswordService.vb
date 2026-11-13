Imports System.Configuration
Imports System.Web.Security
Imports DTOs.Usuarios
Imports Repositories.Interfaces
Imports Services.Interfaces
Imports Utils

Namespace Services
    ' Recuperación de contraseña por link de un solo uso, y cambio obligatorio de claves cortas al loguearse.
    ' El token en claro solo viaja en el link; en PASSWORD_RESET_TOKENS se guarda su SHA-256.
    Public Class RecuperacionPasswordService
        Implements IRecuperacionPasswordService

        Public Const OrigenEmail As String = "Email"
        Public Const OrigenLogin As String = "Login"

        ' Pedidos por mail permitidos por usuario en una hora; los que exceden se ignoran sin avisar.
        Private Const MaxPedidosPorHora As Integer = 3
        Private Const MinutosValidezPorDefecto As Integer = 60
        Private Const MinutosValidezCambioObligatorio As Integer = 15
        ' Claves de menos de esto se consideran cortas para el cambio obligatorio (ForzarCambioPasswordCorta).
        Private Const LongitudMinimaSegura As Integer = 8

        Private ReadOnly tokens As IPasswordResetTokenRepository
        Private ReadOnly usuarios As IUsuarioRepository
        Private ReadOnly notificador As INotificadorUsuarios

        Public Sub New(tokens As IPasswordResetTokenRepository, usuarios As IUsuarioRepository, notificador As INotificadorUsuarios)
            Me.tokens = tokens
            Me.usuarios = usuarios
            Me.notificador = notificador
        End Sub

        ' No informa si el email existe: el controller responde siempre lo mismo. Usuarios inexistentes o
        ' deshabilitados no reciben nada. Las cuentas bloqueadas sí: el link las desbloquea al usarse.
        Public Sub Solicitar(email As String) Implements IRecuperacionPasswordService.Solicitar
            If String.IsNullOrWhiteSpace(email) Then Return

            Dim usuario = usuarios.ObtenerPorEmail(email.Trim())
            If usuario Is Nothing OrElse Not usuario.IsApproved Then Return

            If tokens.ContarRecientes(usuario.UserName, OrigenEmail, DateTime.Now.AddHours(-1)) >= MaxPedidosPorHora Then Return

            Dim minutos = MinutosValidez()
            Dim token = EmitirToken(usuario.UserName, OrigenEmail, minutos)
            notificador.EnviarLinkRecuperacion(usuario.Email, ArmarLinkRestablecer(token), minutos)
        End Sub

        Public Function Validar(token As String) As Boolean Implements IRecuperacionPasswordService.Validar
            Return ObtenerVigente(token) IsNot Nothing
        End Function

        Public Function Restablecer(token As String, password As String) As ResultadoRestablecerPassword Implements IRecuperacionPasswordService.Restablecer
            ' --- Validaciones, antes de escribir nada ---
            Dim registro = ObtenerVigente(token)
            If registro Is Nothing Then Return ResultadoRestablecerPassword.TokenInvalido

            ' ForzarNuevaPassword hace ResetPassword antes de ChangePassword: una clave inválida dejaría la
            ' cuenta con una temporal desconocida.
            If Not usuarios.CumplePoliticaPassword(password) Then Return ResultadoRestablecerPassword.PasswordNoValida

            ' Se gasta el token antes de cambiar la clave: si dos pedidos llegan a la vez, solo uno sigue.
            If Not tokens.MarcarUsado(registro.IdToken) Then Return ResultadoRestablecerPassword.TokenInvalido

            Try
                Dim usuario = usuarios.ObtenerPorUserName(registro.UserName)
                ' ResetPassword falla en cuentas bloqueadas; el link prueba que es el dueño del mail.
                If usuario.IsLockedOut Then usuarios.Desbloquear(registro.UserName)

                If Not usuarios.ForzarNuevaPassword(registro.UserName, password) Then
                    Return ResultadoRestablecerPassword.Fallo
                End If
            Catch
                Return ResultadoRestablecerPassword.Fallo
            End Try

            ' La clave ya cambió: lo que sigue no debe convertir el resultado en error.
            Try
                usuarios.MarcarPasswordMigrada(registro.UserName)
            Catch
            End Try
            Try
                tokens.InvalidarDelUsuario(registro.UserName)
            Catch
            End Try

            Return ResultadoRestablecerPassword.Ok
        End Function

        ' Regla única para el login de la API y login.aspx. La contraseña en claro solo se tiene en el login,
        ' por eso el largo se chequea ahí y no se puede calcular sobre el hash.
        Public Function RequiereCambioObligatorio(userName As String, password As String) As Boolean Implements IRecuperacionPasswordService.RequiereCambioObligatorio
            If Not LeerBooleano("ForzarCambioPasswordCorta", False) Then Return False

            Dim minimo = Math.Max(LongitudMinimaSegura, System.Web.Security.Membership.MinRequiredPasswordLength)
            If If(password, "").Length >= minimo Then Return False

            Return Not usuarios.ObtenerPerfil(userName).PasswordMigrada
        End Function

        Public Function EmitirTokenCambioObligatorio(userName As String) As String Implements IRecuperacionPasswordService.EmitirTokenCambioObligatorio
            Return EmitirToken(userName, OrigenLogin, MinutosValidezCambioObligatorio)
        End Function

        ' appSetting UrlRestablecerPassword: pantalla que recibe ?token=. Puede ser absoluta (frontend nuevo) o
        ' relativa al sitio con "~/" (hoy: ~/restablecerClave.aspx); la relativa se completa con el host del
        ' pedido actual, así el mismo valor sirve en todos los ambientes.
        Public Function ArmarLinkRestablecer(token As String) As String Implements IRecuperacionPasswordService.ArmarLinkRestablecer
            Dim baseUrl = ConfigurationManager.AppSettings("UrlRestablecerPassword")
            If String.IsNullOrWhiteSpace(baseUrl) Then
                Throw New InvalidOperationException("La clave 'UrlRestablecerPassword' no está configurada en el web.config.")
            End If
            baseUrl = baseUrl.Trim()
            If baseUrl.StartsWith("~/") Then
                Dim actual = System.Web.HttpContext.Current.Request.Url
                baseUrl = actual.GetLeftPart(UriPartial.Authority) & System.Web.VirtualPathUtility.ToAbsolute(baseUrl)
            End If

            Dim separador = If(baseUrl.Contains("?"), "&", "?")
            Return baseUrl & separador & "token=" & Uri.EscapeDataString(token)
        End Function

        ' Solo vale el último token: al emitir uno nuevo se invalidan los pendientes del usuario.
        Private Function EmitirToken(userName As String, origen As String, minutos As Integer) As String
            tokens.InvalidarDelUsuario(userName)

            Dim token = TokenSeguro.Generar()
            tokens.Crear(New PASSWORD_RESET_TOKENS With {
                .UserName = userName,
                .TokenHash = TokenSeguro.Hash(token),
                .Origen = origen,
                .FechaExpiracion = DateTime.Now.AddMinutes(minutos)
            })
            Return token
        End Function

        ' Nothing si el token no existe, ya se usó, venció o el usuario ya no está habilitado.
        Private Function ObtenerVigente(token As String) As PASSWORD_RESET_TOKENS
            If String.IsNullOrWhiteSpace(token) Then Return Nothing

            Dim registro = tokens.ObtenerPorHash(TokenSeguro.Hash(token.Trim()))
            If registro Is Nothing OrElse registro.FechaUso.HasValue OrElse registro.FechaExpiracion <= DateTime.Now Then Return Nothing

            Dim usuario = usuarios.ObtenerPorUserName(registro.UserName)
            If usuario Is Nothing OrElse Not usuario.IsApproved Then Return Nothing

            Return registro
        End Function

        ' Vigencia del link por mail (appSetting MinutosValidezLinkPassword). Pública para mostrarla en recuperaClave.aspx.
        Public Shared Function MinutosValidez() As Integer
            Dim minutos As Integer
            If Integer.TryParse(ConfigurationManager.AppSettings("MinutosValidezLinkPassword"), minutos) AndAlso minutos > 0 Then
                Return minutos
            End If
            Return MinutosValidezPorDefecto
        End Function

        Private Shared Function LeerBooleano(clave As String, porDefecto As Boolean) As Boolean
            Dim valor As Boolean
            Return If(Boolean.TryParse(ConfigurationManager.AppSettings(clave), valor), valor, porDefecto)
        End Function
    End Class
End Namespace
