Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Feedback
Imports Services.Interfaces

<JwtAuthorize>
Public Class FeedbackController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IFeedbackService

    Public Sub New(servicio As IFeedbackService)
        Me.servicio = servicio
    End Sub

    ' Cada usuario ve su propio feedback; Administrador ve el de todos y puede filtrar por usuario.
    ' Filtro opcional por tipo (Sugerencia, Error, Otro).
    <HttpGet>
    <Route("api/feedback")>
    Public Function Listar(Optional tipo As String = Nothing, Optional usuario As String = Nothing) As IHttpActionResult
        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()

        Return Ok(servicio.Listar(actual, EsAdministrador(), tipo, usuario))
    End Function

    <HttpGet>
    <Route("api/feedback/{id:int}")>
    Public Function ObtenerPorId(id As Integer) As IHttpActionResult
        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()

        Dim resultado = servicio.ObtenerPorId(id, actual, EsAdministrador())

        Select Case resultado.Estado
            Case ResultadoOperacionFeedback.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionFeedback.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case Else
                Return Ok(resultado.Feedback)
        End Select
    End Function

    <HttpPost>
    <Route("api/feedback")>
    Public Function Crear(<FromBody> input As FeedbackInputDTO) As IHttpActionResult
        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del comentario.")

        Dim resultado = servicio.Crear(actual, input)

        Select Case resultado.Estado
            Case ResultadoOperacionFeedback.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionFeedback.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Feedback)
        End Select
    End Function

    <HttpPut>
    <Route("api/feedback/{id:int}")>
    Public Function Actualizar(id As Integer, <FromBody> input As FeedbackInputDTO) As IHttpActionResult
        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del comentario.")

        Dim resultado = servicio.Actualizar(id, actual, input)

        Select Case resultado.Estado
            Case ResultadoOperacionFeedback.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionFeedback.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacionFeedback.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionFeedback.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Feedback)
        End Select
    End Function

    <HttpDelete>
    <Route("api/feedback/{id:int}")>
    Public Function Eliminar(id As Integer) As IHttpActionResult
        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()

        Dim resultado = servicio.Eliminar(id, actual, EsAdministrador())

        Select Case resultado.Estado
            Case ResultadoOperacionFeedback.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionFeedback.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacionFeedback.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

    Private Function EsAdministrador() As Boolean
        Return User.IsInRole(Aplicacion.Grupos.Administrador)
    End Function

End Class
