Imports System.Globalization
Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Feriados
Imports Services.Interfaces

<JwtAuthorize>
Public Class FeriadosController
    Inherits ControladorApiBase

    Private Const FormatoFecha As String = "yyyy-MM-dd"

    Private ReadOnly servicio As IFeriadoService

    Public Sub New(servicio As IFeriadoService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado. Filtro opcional por año.
    <HttpGet>
    <Route("api/feriados")>
    Public Function Listar(Optional anio As Integer? = Nothing) As IHttpActionResult
        Return Ok(servicio.Listar(anio))
    End Function

    ' La fecha va en la ruta como yyyy-MM-dd (es la clave del feriado).
    <HttpGet>
    <Route("api/feriados/{fecha}")>
    Public Function ObtenerPorFecha(fecha As String) As IHttpActionResult
        Dim f As Date
        If Not TryParsearFecha(fecha, f) Then Return BadRequest("La fecha debe tener el formato " & FormatoFecha & ".")

        Dim resultado = servicio.ObtenerPorFecha(f)

        If resultado.Estado = ResultadoOperacionFeriado.NoEncontrado Then Return NotFound()
        Return Ok(resultado.Feriado)
    End Function

    ' Escritura: catálogo compartido por todos los usuarios, solo Administrador.
    <HttpPost>
    <Route("api/feriados")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Crear(<FromBody> input As FeriadoInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del feriado.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionFeriado.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionFeriado.Duplicado
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionFeriado.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Feriado)
        End Select
    End Function

    <HttpPut>
    <Route("api/feriados/{fecha}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(fecha As String, <FromBody> input As FeriadoInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del feriado.")

        Dim f As Date
        If Not TryParsearFecha(fecha, f) Then Return BadRequest("La fecha debe tener el formato " & FormatoFecha & ".")

        Dim resultado = servicio.Actualizar(f, input)

        Select Case resultado.Estado
            Case ResultadoOperacionFeriado.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionFeriado.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionFeriado.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Feriado)
        End Select
    End Function

    <HttpDelete>
    <Route("api/feriados/{fecha}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Eliminar(fecha As String) As IHttpActionResult
        Dim f As Date
        If Not TryParsearFecha(fecha, f) Then Return BadRequest("La fecha debe tener el formato " & FormatoFecha & ".")

        Dim resultado = servicio.Eliminar(f)

        Select Case resultado.Estado
            Case ResultadoOperacionFeriado.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionFeriado.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

    Private Shared Function TryParsearFecha(texto As String, ByRef fecha As Date) As Boolean
        Return Date.TryParseExact(texto, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, fecha)
    End Function

End Class
