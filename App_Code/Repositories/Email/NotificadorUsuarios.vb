Imports System.Configuration
Imports System.IO
Imports System.Web
Imports Repositories.Interfaces

Namespace Repositories.Email
    ' Encapsula el armado/envío del mail de recuperación de contraseña sobre la clase Correo.
    Public Class NotificadorUsuarios
        Implements INotificadorUsuarios

        Private Const NombreArchivoLogDesarrollo As String = "recuperacion-password-dev.log"

        Public Sub EnviarLinkRecuperacion(email As String, link As String, minutosValidez As Integer) Implements INotificadorUsuarios.EnviarLinkRecuperacion
            If Not EnviarMailsHabilitado() Then
                RegistrarEnLogDeDesarrollo(email, link)
                Return
            End If

            Dim c As New Correo()

            c.Asunto = "Recuperación de contraseña"
            c.De = "BASHoras <respuestaautomatica@bas.com.ar>"
            c.Para = email

            Dim linkHtml = HttpUtility.HtmlAttributeEncode(link)
            Dim msg As String = ""
            msg += "<h4>Mensaje de BASHoras</h4><br>"
            msg += "Recibimos un pedido para restablecer tu contraseña. Para elegir una nueva, ingresá a este link:<br><br>"
            msg += "<a href=""" & linkHtml & """>" & linkHtml & "</a><br><br>"
            msg += "El link vence en " & minutosValidez & " minutos y sirve una sola vez. "
            msg += "Si no lo pediste vos, ignorá este mensaje: tu contraseña actual sigue funcionando.<br><br><br>"
            msg += "No respondas este mensaje ya que ha sido generado automáticamente.<hr>"

            c.Mensaje = msg
            c.enviar()
        End Sub

        Private Function EnviarMailsHabilitado() As Boolean
            Dim valor = ConfigurationManager.AppSettings("EnviarMailsHabilitado")
            Dim habilitado As Boolean

            ' Si la key no existe o tiene un valor inválido, por defecto se envía (comportamiento seguro para producción).
            If Not Boolean.TryParse(valor, habilitado) Then Return True

            Return habilitado
        End Function

        ' Con el envío de mails deshabilitado (desarrollo), deja constancia acá para poder usar el link.
        ' Se escribe en el temp del sistema (fuera de la carpeta del sitio) para que un deploy/backup de la carpeta
        ' del sitio nunca se lleve links vigentes de paso.
        Private Sub RegistrarEnLogDeDesarrollo(email As String, link As String)
            Dim linea = String.Format("{0:yyyy-MM-dd HH:mm:ss} | {1} | {2}" & Environment.NewLine,
                                       DateTime.Now, email, link)

            Dim carpetaLog = Path.Combine(Path.GetTempPath(), "BASHoras")
            Directory.CreateDirectory(carpetaLog)

            Dim rutaLog = Path.Combine(carpetaLog, NombreArchivoLogDesarrollo)
            File.AppendAllText(rutaLog, linea)
        End Sub
    End Class
End Namespace
