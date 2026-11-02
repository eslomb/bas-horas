Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Auth
Imports DTOs.Remuneraciones
Imports Services.Interfaces

' Administrador cuyo usuario figura en Utils.AccesoRemuneraciones; el resto recibe 403. Errores como { "mensaje": "..." }.
<RemuneracionesAuthorize>
Public Class RemuneracionesController
    Inherits ControladorApiBase

    Private Const TipoXlsx As String = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"

    Private ReadOnly servicio As IRemuneracionService

    Public Sub New(servicio As IRemuneracionService)
        Me.servicio = servicio
    End Sub

    <HttpGet>
    <Route("api/remuneraciones")>
    Public Function Listar(Optional mes As Integer? = Nothing, Optional anio As Integer? = Nothing) As IHttpActionResult
        Dim resultado = servicio.Listar(mes, anio)

        If resultado.Estado = ResultadoOperacionRemuneracion.Invalido Then Return Rechazar(HttpStatusCode.BadRequest, resultado.Mensaje)
        Return Ok(resultado.Items)
    End Function

    <HttpPost>
    <Route("api/remuneraciones")>
    Public Function Crear(<FromBody> input As RemuneracionInputDTO) As IHttpActionResult
        If input Is Nothing Then Return Rechazar(HttpStatusCode.BadRequest, "Debe enviar los datos de la remuneración.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionRemuneracion.Invalido
                Return Rechazar(HttpStatusCode.BadRequest, resultado.Mensaje)
            Case ResultadoOperacionRemuneracion.Duplicado
                Return Rechazar(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionRemuneracion.Fallo
                Return Rechazar(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Remuneracion)
        End Select
    End Function

    ' Solo cambia el monto: usuario, año y mes no se modifican.
    <HttpPut>
    <Route("api/remuneraciones/{id:int}")>
    Public Function Actualizar(id As Integer, <FromBody> input As ActualizarRemuneracionInputDTO) As IHttpActionResult
        If input Is Nothing Then Return Rechazar(HttpStatusCode.BadRequest, "Debe enviar la remuneración.")

        Dim resultado = servicio.Actualizar(id, input)

        Select Case resultado.Estado
            Case ResultadoOperacionRemuneracion.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionRemuneracion.Invalido
                Return Rechazar(HttpStatusCode.BadRequest, resultado.Mensaje)
            Case ResultadoOperacionRemuneracion.Fallo
                Return Rechazar(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Remuneracion)
        End Select
    End Function

    <HttpDelete>
    <Route("api/remuneraciones/{id:int}")>
    Public Function Eliminar(id As Integer) As IHttpActionResult
        Dim resultado = servicio.Eliminar(id)

        Select Case resultado.Estado
            Case ResultadoOperacionRemuneracion.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionRemuneracion.Fallo
                Return Rechazar(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

    ' Usuarios activos con su última remuneración conocida, listo para completar y volver a subir con /importar.
    <HttpGet>
    <Route("api/remuneraciones/plantilla")>
    Public Function Plantilla(Optional mes As Integer? = Nothing, Optional anio As Integer? = Nothing) As IHttpActionResult
        Return Descargar(servicio.GenerarPlantilla(mes, anio))
    End Function

    ' El listado filtrado como .xlsx.
    <HttpGet>
    <Route("api/remuneraciones/exportar")>
    Public Function Exportar(Optional mes As Integer? = Nothing, Optional anio As Integer? = Nothing) As IHttpActionResult
        Return Descargar(servicio.Exportar(mes, anio))
    End Function

    ' multipart/form-data con un .xlsx (hoja "Remuneraciones"). Todo o nada: si hay errores, 400 y no se escribe nada.
    <HttpPost>
    <Route("api/remuneraciones/importar")>
    Public Async Function Importar() As Task(Of IHttpActionResult)
        If Request.Content Is Nothing OrElse Not Request.Content.IsMimeMultipartContent() Then
            Return Rechazar(HttpStatusCode.UnsupportedMediaType, "Debe enviar el archivo como multipart/form-data.")
        End If

        Dim proveedor As New MultipartMemoryStreamProvider()
        Await Request.Content.ReadAsMultipartAsync(proveedor)

        Dim parte = proveedor.Contents.FirstOrDefault(
            Function(c) c.Headers.ContentDisposition IsNot Nothing AndAlso Not String.IsNullOrEmpty(c.Headers.ContentDisposition.FileName))
        If parte Is Nothing Then Return Rechazar(HttpStatusCode.BadRequest, "Debe adjuntar un archivo .xlsx.")

        Dim archivo = Await parte.ReadAsByteArrayAsync()
        Dim resultado = servicio.Importar(archivo)

        Select Case resultado.Estado
            Case ResultadoOperacionRemuneracion.Invalido
                ' Cuerpo con la misma forma que el éxito: { Creados, Actualizados, Omitidos, Errores }.
                Return Content(HttpStatusCode.BadRequest, resultado.Resumen)
            Case ResultadoOperacionRemuneracion.Fallo
                Return Rechazar(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Resumen)
        End Select
    End Function

    Private Function Descargar(resultado As ResultadoArchivoRemuneraciones) As IHttpActionResult
        If resultado.Estado = ResultadoOperacionRemuneracion.Invalido Then Return Rechazar(HttpStatusCode.BadRequest, resultado.Mensaje)

        Dim respuesta As New HttpResponseMessage(HttpStatusCode.OK) With {
            .Content = New ByteArrayContent(resultado.Contenido)
        }
        respuesta.Content.Headers.ContentType = New MediaTypeHeaderValue(TipoXlsx)
        respuesta.Content.Headers.ContentDisposition = New ContentDispositionHeaderValue("attachment") With {
            .FileName = resultado.NombreArchivo
        }
        Return ResponseMessage(respuesta)
    End Function

    Private Function Rechazar(estado As HttpStatusCode, mensaje As String) As IHttpActionResult
        Return Content(estado, New With {.mensaje = mensaje})
    End Function

End Class
