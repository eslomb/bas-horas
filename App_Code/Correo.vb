Imports Microsoft.VisualBasic
Imports System.Net.Mail
Imports System.Net.Mail.SmtpClient
Imports System.IO

Public Class Correo
    Private msg As MailMessage
    Private cls_errMsg As String
    Private cls_Nombre As String
    Private cls_De As String
    Private cls_Para As String
    Private cls_CC As String
    Private cls_CCO As String
    Private cls_Asunto As String
    Private cls_Mensaje As String
    Private cls_Prioridad As MailPriority
    Private cls_Respuesta As String
    Private cls_Html As Boolean

#Region "propiedades"
    Public ReadOnly Property errMsg() As String
        Get
            Return cls_errMsg
        End Get
    End Property

    Public Property Nombre() As String
        Get
            Return cls_Nombre
        End Get
        Set(ByVal value As String)
            cls_Nombre = value
        End Set
    End Property
    Public Property De() As String
        Get
            Return cls_De
        End Get
        Set(ByVal value As String)
            Dim listamails() As String = value.Split(";")
            If listamails.Length > 1 Then
                cls_De = listamails(0)
            Else
                cls_De = value
            End If
        End Set
    End Property
    Public Property Para() As String
        Get
            Return cls_Para
        End Get
        Set(ByVal value As String)
            cls_Para = value
        End Set
    End Property
    Public Property CC() As String
        Get
            Return cls_CC
        End Get
        Set(ByVal value As String)
            cls_CC = value
        End Set
    End Property
    Public Property CCO() As String
        Get
            Return cls_CCO
        End Get
        Set(ByVal value As String)
            cls_CCO = value
        End Set
    End Property
    Public Property Asunto() As String
        Get
            Return cls_Asunto
        End Get
        Set(ByVal value As String)
            cls_Asunto = value
        End Set
    End Property
    Public Property Mensaje() As String
        Get
            Return cls_Mensaje
        End Get
        Set(ByVal value As String)
            cls_Mensaje = value
        End Set
    End Property
    Public Property Prioridad() As MailPriority
        Get
            Return cls_Prioridad
        End Get
        Set(ByVal value As MailPriority)
            cls_Prioridad = value
        End Set
    End Property
    Public Property Respuesta() As String
        Get
            Return cls_Respuesta
        End Get
        Set(ByVal value As String)
            cls_Respuesta = value
        End Set
    End Property
    Public Property Html() As Boolean
        Get
            Return cls_Html
        End Get
        Set(ByVal value As Boolean)
            cls_Html = value
        End Set
    End Property
#End Region


    Public Sub New()
        Nombre = ""
        De = "Buenos Aires Software <respuestaautomatica@bas.com.ar>"
        Para = "atencionalcliente@bas.com.ar"
        Asunto = "web soporte"
        Html = True
        Prioridad = MailPriority.Normal
        msg = New MailMessage()
    End Sub

    Public Sub adjuntar(ByVal pAttach As Stream, ByVal pAttachName As String)
        msg.Attachments.Add(New Attachment(pAttach, pAttachName))
    End Sub

    Public Sub copiarA(ByVal pDestino As String)
        msg.CC.Add(pDestino)
    End Sub

    Public Sub copiarOcultoA(ByVal pDestino As String)
        msg.Bcc.Add(pDestino)
    End Sub

    Public Function enviar() As String
        Dim err As String = "Mensaje enviado"
        Dim smtp As New SmtpClient("10.0.0.132")
        smtp.Host = "mail.bas.com.ar"
        smtp.Credentials = New System.Net.NetworkCredential("respuestaautomatica", "tripagorda")
        'smtp.Credentials = New System.Net.NetworkCredential("bas@lombardinet.com", "tripagorda")
        msg.From = New MailAddress(De)

        Dim listamails() As String = Para.Split(";")
        For Each m As String In listamails
            msg.To.Add(New MailAddress(m))
        Next
        msg.IsBodyHtml = Html
        If Not String.IsNullOrEmpty(CC) Then
            msg.CC.Add(New MailAddress(CC))
        End If
        If Not String.IsNullOrEmpty(CCO) Then
            msg.Bcc.Add(New MailAddress(CCO))
        End If
        msg.Subject = Asunto
        msg.Body = Mensaje
        msg.Priority = Me.Prioridad
        If Not String.IsNullOrEmpty(Respuesta) Then
            err = Respuesta
        End If
        cls_errMsg = Nothing

        Try
            smtp.Send(msg)
        Catch e As Exception
            err = e.ToString 'para conservar compatibilidad
            cls_errMsg = e.Message
        End Try

        Return err
    End Function


End Class
