Imports System.Web.Http
Imports System.Net
Imports Auth

Public Class TestController
    Inherits ApiController

    <HttpGet>
    Public Function GetStatus() As IHttpActionResult
        ' Devolvemos un objeto anónimo que Web API transformará automáticamente en JSON
        Dim respuesta = New With {
            .status = "OK",
            .mensaje = "La API en VB.NET está funcionando correctamente",
            .fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }

        Return Ok(respuesta)
    End Function

End Class
