Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Registros
Imports Services.Interfaces

<JwtAuthorize>
Public Class RegistrosController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IRegistroService

    Public Sub New(servicio As IRegistroService)
        Me.servicio = servicio
    End Sub

    Private Const TamanioPaginaPorDefecto As Integer = 100
    Private Const TamanioPaginaMaximo As Integer = 1000

    <HttpGet>
    <Route("api/registros")>
    Public Function Listar(Optional desde As Date? = Nothing, Optional hasta As Date? = Nothing,
                            Optional pagina As Integer = 1, Optional tamanioPagina As Integer = TamanioPaginaPorDefecto) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim fechaDesde = If(desde.HasValue, desde.Value, Date.Today)
        Dim fechaHasta = If(hasta.HasValue, hasta.Value, Date.Today)

        If fechaDesde > fechaHasta Then Return BadRequest("'desde' no puede ser posterior a 'hasta'.")

        If pagina < 1 Then pagina = 1
        If tamanioPagina < 1 Then tamanioPagina = TamanioPaginaPorDefecto
        If tamanioPagina > TamanioPaginaMaximo Then tamanioPagina = TamanioPaginaMaximo

        Return Ok(servicio.Listar(usuario, fechaDesde, fechaHasta, pagina, tamanioPagina))
    End Function

    <HttpGet>
    <Route("api/registros/{id}")>
    Public Function ObtenerPorId(id As Integer) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicio.ObtenerPorId(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacion.NoEncontrado
                Return NotFound()
            Case ResultadoOperacion.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case Else
                Return Ok(resultado.Registro)
        End Select
    End Function

    <HttpPost>
    <Route("api/registros")>
    Public Function Crear(<FromBody> input As RegistroInputDTO) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        If input Is Nothing Then Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "Debe enviar los datos del registro."})

        Dim resultado = servicio.Crear(usuario, User.IsInRole(Aplicacion.Grupos.Administrador), input)

        Select Case resultado.Estado
            Case ResultadoOperacion.Invalido
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacion.Duplicado
                Return Content(HttpStatusCode.Conflict, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacion.Fallo
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = resultado.Mensaje})
            Case Else
                Return Ok(resultado.Registro)
        End Select
    End Function

    <HttpPut>
    <Route("api/registros/{id}")>
    Public Function Actualizar(id As Integer, <FromBody> input As RegistroInputDTO) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        If input Is Nothing Then Return Content(HttpStatusCode.BadRequest, New With {.mensaje = "Debe enviar los datos del registro."})

        Dim resultado = servicio.Actualizar(id, usuario, User.IsInRole(Aplicacion.Grupos.Administrador), input)

        Select Case resultado.Estado
            Case ResultadoOperacion.NoEncontrado
                Return NotFound()
            Case ResultadoOperacion.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacion.Invalido
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacion.Duplicado
                Return Content(HttpStatusCode.Conflict, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacion.Fallo
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = resultado.Mensaje})
            Case Else
                Return Ok(resultado.Registro)
        End Select
    End Function

    <HttpDelete>
    <Route("api/registros/{id}")>
    Public Function Eliminar(id As Integer) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicio.Eliminar(id, usuario)

        Select Case resultado.Estado
            Case ResultadoOperacion.NoEncontrado
                Return NotFound()
            Case ResultadoOperacion.Prohibido
                Return StatusCode(HttpStatusCode.Forbidden)
            Case ResultadoOperacion.Invalido
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacion.Fallo
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = resultado.Mensaje})
            Case Else
                Return Ok()
        End Select
    End Function

End Class
