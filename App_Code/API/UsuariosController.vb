Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Usuarios
Imports Services.Interfaces

<JwtAuthorize>
Public Class UsuariosController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IUsuarioService

    Public Sub New(servicio As IUsuarioService)
        Me.servicio = servicio
    End Sub

    ' --- Self-service: cualquier usuario autenticado sobre su propia cuenta ---

    <HttpGet>
    <Route("api/usuarios/me")>
    Public Function ObtenerPropio() As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim datos = servicio.ObtenerPorUserName(usuario)
        If datos Is Nothing Then Return NotFound()
        Return Ok(datos)
    End Function

    <HttpPut>
    <Route("api/usuarios/me")>
    Public Function ActualizarPropio(<FromBody> input As ActualizarPerfilPropioInputDTO) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        If input Is Nothing Then Return BadRequest("Debe enviar los datos a actualizar.")

        Dim resultado = servicio.ActualizarPerfilPropio(usuario, input)

        Select Case resultado.Estado
            Case ResultadoOperacionUsuario.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionUsuario.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Usuario)
        End Select
    End Function

    <HttpPost>
    <Route("api/usuarios/me/password")>
    Public Function CambiarPassword(<FromBody> input As CambiarPasswordInputDTO) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        If input Is Nothing Then Return BadRequest("Debe enviar la contraseña actual y la nueva.")

        Dim resultado = servicio.CambiarPassword(usuario, input)

        Select Case resultado
            Case ResultadoCambioPassword.Ok
                Return Ok()
            Case ResultadoCambioPassword.PasswordActualIncorrecta
                Return Content(HttpStatusCode.BadRequest, "La contraseña actual no es correcta.")
            Case ResultadoCambioPassword.PasswordNoValida
                Return Content(HttpStatusCode.BadRequest, "La nueva contraseña no cumple con la política requerida.")
            Case Else
                Return Content(HttpStatusCode.InternalServerError, "No se pudo cambiar la contraseña.")
        End Select
    End Function

    ' --- Equipo: usuarios cuyo Dependencia es el Legajo de quien llama ---

    <HttpGet>
    <JwtAuthorize(Roles:="Avanzado,Administrador")>
    <Route("api/usuarios/equipo")>
    Public Function ListarEquipo() As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()
        Return Ok(servicio.ListarEquipo(usuario))
    End Function

    <HttpPut>
    <JwtAuthorize(Roles:="Avanzado,Administrador")>
    <Route("api/usuarios/equipo/periodos")>
    Public Function ActualizarPeriodosEquipo(<FromBody> periodos As List(Of PeriodoEquipoInputDTO)) As IHttpActionResult
        Dim usuario = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(usuario) Then Return Unauthorized()

        Dim resultado = servicio.ActualizarPeriodosEquipo(usuario, periodos)

        Select Case resultado.Estado
            Case ResultadoOperacionUsuario.Invalido
                Return Content(HttpStatusCode.BadRequest, New With {.mensaje = resultado.Mensaje})
            Case ResultadoOperacionUsuario.Fallo
                Return Content(HttpStatusCode.InternalServerError, New With {.mensaje = resultado.Mensaje})
            Case Else
                Return Ok(servicio.ListarEquipo(usuario))
        End Select
    End Function

    ' --- Administración: requiere rol Administrador ---

    <HttpGet>
    <JwtAuthorize(Roles:="Administrador")>
    <Route("api/usuarios")>
    Public Function Listar() As IHttpActionResult
        Return Ok(servicio.Listar())
    End Function

    <HttpGet>
    <JwtAuthorize(Roles:="Administrador")>
    <Route("api/usuarios/{userName}")>
    Public Function ObtenerPorUserName(userName As String) As IHttpActionResult
        Dim datos = servicio.ObtenerPorUserName(userName)
        If datos Is Nothing Then Return NotFound()
        Return Ok(datos)
    End Function

    <HttpPost>
    <JwtAuthorize(Roles:="Administrador")>
    <Route("api/usuarios")>
    Public Function Crear(<FromBody> input As CrearUsuarioInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del usuario.")

        Dim resultado = servicio.CrearUsuario(input)

        Select Case resultado.Estado
            Case ResultadoOperacionUsuario.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionUsuario.Conflicto
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionUsuario.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Content(HttpStatusCode.Created, resultado.Usuario)
        End Select
    End Function

    <HttpPut>
    <JwtAuthorize(Roles:="Administrador")>
    <Route("api/usuarios/{userName}")>
    Public Function ActualizarUsuario(userName As String, <FromBody> input As ActualizarUsuarioInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos a actualizar.")

        Dim actual = ObtenerUsuarioActual()
        If String.IsNullOrEmpty(actual) Then Return Unauthorized()

        Dim resultado = servicio.ActualizarUsuario(userName, input, actual)

        Select Case resultado.Estado
            Case ResultadoOperacionUsuario.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionUsuario.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionUsuario.Conflicto
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionUsuario.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Usuario)
        End Select
    End Function

End Class
