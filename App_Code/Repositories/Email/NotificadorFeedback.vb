Imports System.Configuration
Imports System.IO
Imports System.Web
Imports Repositories.Interfaces

Namespace Repositories.Email
    ' Encapsula el armado/envío del mail con la respuesta de un Administrador a un feedback, sobre la clase Correo.
    Public Class NotificadorFeedback
        Implements INotificadorFeedback

        Private Const NombreArchivoLogDesarrollo As String = "feedback-respuestas-dev.log"

        Public Sub EnviarRespuesta(email As String, respuesta As String, comentarioOriginal As String) Implements INotificadorFeedback.EnviarRespuesta
            If Not EnviarMailsHabilitado() Then
                RegistrarEnLogDeDesarrollo(email, respuesta)
                Return
            End If

            Dim c As New Correo()

            c.Asunto = "Respuesta a tu comentario"
            c.De = "BASHoras <respuestaautomatica@bas.com.ar>"
            c.Para = email

            Dim msg As String = ""
            msg += "<h4>Mensaje de BASHoras</h4><br>"
            msg += ATextoHtml(respuesta) & "<br><br>"
            msg += "<hr>"
            msg += "<b>Tu comentario:</b><br>"
            msg += "<i>" & ATextoHtml(comentarioOriginal) & "</i><br><br><br>"
            msg += "No respondas este mensaje ya que ha sido generado automáticamente.<hr>"

            c.Mensaje = msg
            c.enviar()

            ' Correo.enviar no lanza: deja el error en errMsg.
            If Not String.IsNullOrEmpty(c.errMsg) Then Throw New InvalidOperationException(c.errMsg)
        End Sub

        ' Los textos los escriben usuarios: se escapan y se respetan los saltos de línea.
        Private Shared Function ATextoHtml(texto As String) As String
            Return HttpUtility.HtmlEncode(If(texto, "")).Replace(vbCrLf, vbLf).Replace(vbLf, "<br>")
        End Function

        Private Function EnviarMailsHabilitado() As Boolean
            Dim valor = ConfigurationManager.AppSettings("EnviarMailsHabilitado")
            Dim habilitado As Boolean

            ' Si la key no existe o tiene un valor inválido, por defecto se envía (comportamiento seguro para producción).
            If Not Boolean.TryParse(valor, habilitado) Then Return True

            Return habilitado
        End Function

        ' Con el envío de mails deshabilitado (desarrollo), deja constancia del mail que se hubiera enviado.
        ' Mismo lugar que el log de recuperación de contraseña (temp del sistema, fuera de la carpeta del sitio).
        Private Sub RegistrarEnLogDeDesarrollo(email As String, respuesta As String)
            Dim linea = String.Format("{0:yyyy-MM-dd HH:mm:ss} | {1} | {2}" & Environment.NewLine,
                                       DateTime.Now, email, If(respuesta, "").Replace(vbCr, " ").Replace(vbLf, " "))

            Dim carpetaLog = Path.Combine(Path.GetTempPath(), "BASHoras")
            Directory.CreateDirectory(carpetaLog)

            Dim rutaLog = Path.Combine(carpetaLog, NombreArchivoLogDesarrollo)
            File.AppendAllText(rutaLog, linea)
        End Sub
    End Class
End Namespace
