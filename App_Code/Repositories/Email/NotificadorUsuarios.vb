Imports System.Configuration
Imports System.IO
Imports Repositories.Interfaces

Namespace Repositories.Email
    ' Encapsula el armado/envío del mail de recuperación de contraseña sobre la clase Correo
    ' (misma idea que recuperaClave.aspx.vb, separada en su propia clase para uso desde la API).
    Public Class NotificadorUsuarios
        Implements INotificadorUsuarios

        Private Const NombreArchivoLogDesarrollo As String = "recuperacion-password-dev.log"

        Public Sub EnviarNuevaPassword(email As String, nuevaPassword As String) Implements INotificadorUsuarios.EnviarNuevaPassword
            If Not EnviarMailsHabilitado() Then
                RegistrarEnLogDeDesarrollo(email, nuevaPassword)
                Return
            End If

            Dim c As New Correo()

            c.Asunto = "Recuperación de contraseña"
            c.De = "BASHoras <respuestaautomatica@bas.com.ar>"
            c.Para = email

            Dim msg As String = ""
            msg += "<h4>Mensaje de BASHoras</h4><br>"
            msg += "Tu nueva clave de acceso es: " & nuevaPassword & "<br><br><br>"
            msg += "No respondas este mensaje ya que ha sido generado automáticamente. Si crees que lo recibiste por error, por favor ignoralo.<hr>"

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

        ' Con el envío de mails deshabilitado (desarrollo), deja constancia acá para poder ver la password generada.
        ' Se escribe en el temp del sistema (fuera de la carpeta del sitio) para que un deploy/backup de la carpeta
        ' del sitio nunca se lleve passwords en texto plano de paso.
        Private Sub RegistrarEnLogDeDesarrollo(email As String, nuevaPassword As String)
            Dim linea = String.Format("{0:yyyy-MM-dd HH:mm:ss} | {1} | {2}" & Environment.NewLine,
                                       DateTime.Now, email, nuevaPassword)

            Dim carpetaLog = Path.Combine(Path.GetTempPath(), "BASHoras")
            Directory.CreateDirectory(carpetaLog)

            Dim rutaLog = Path.Combine(carpetaLog, NombreArchivoLogDesarrollo)
            File.AppendAllText(rutaLog, linea)
        End Sub
    End Class
End Namespace
