Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Configuraciones
Imports Services.Interfaces

<JwtAuthorize>
Public Class ConfiguracionesController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IConfiguracionService

    Public Sub New(servicio As IConfiguracionService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado (el frontend necesita los valores, ej. PERMITIR_CARGAS).
    <HttpGet>
    <Route("api/configuraciones")>
    Public Function Listar() As IHttpActionResult
        Return Ok(servicio.Listar())
    End Function

    <HttpGet>
    <Route("api/configuraciones/{codigo}")>
    Public Function ObtenerPorCodigo(codigo As String) As IHttpActionResult
        Dim resultado = servicio.ObtenerPorCodigo(codigo)

        If resultado.Estado = ResultadoOperacionConfiguracion.NoEncontrado Then Return NotFound()
        Return Ok(resultado.Configuracion)
    End Function

    ' Escritura: configuración global, solo Administrador. Solo se modifica el Valor; no hay alta ni baja.
    <HttpPut>
    <Route("api/configuraciones/{codigo}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(codigo As String, <FromBody> input As ConfiguracionInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar el valor de la configuración.")

        Dim resultado = servicio.ActualizarValor(codigo, input)

        Select Case resultado.Estado
            Case ResultadoOperacionConfiguracion.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionConfiguracion.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionConfiguracion.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Configuracion)
        End Select
    End Function

End Class
