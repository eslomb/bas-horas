Imports System.Web.Http
Imports System.Net
Imports Auth
Imports DTOs.Productos
Imports Services.Interfaces

<JwtAuthorize>
Public Class ProductosController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IProductoService

    Public Sub New(servicio As IProductoService)
        Me.servicio = servicio
    End Sub

    ' Lectura: cualquier usuario autenticado (lo necesita el formulario de carga de horas).
    <HttpGet>
    <Route("api/productos")>
    Public Function Listar(Optional soloActivos As Boolean = True) As IHttpActionResult
        Return Ok(servicio.Listar(soloActivos))
    End Function

    <HttpGet>
    <Route("api/productos/{id:int}")>
    Public Function ObtenerPorId(id As Integer) As IHttpActionResult
        Dim resultado = servicio.ObtenerPorId(id)

        If resultado.Estado = ResultadoOperacionProducto.NoEncontrado Then Return NotFound()
        Return Ok(resultado.Producto)
    End Function

    ' Escritura: catálogo compartido por todos los usuarios, solo Administrador.
    <HttpPost>
    <Route("api/productos")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Crear(<FromBody> input As ProductoInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del producto.")

        Dim resultado = servicio.Crear(input)

        Select Case resultado.Estado
            Case ResultadoOperacionProducto.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionProducto.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Producto)
        End Select
    End Function

    <HttpPut>
    <Route("api/productos/{id:int}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Actualizar(id As Integer, <FromBody> input As ProductoInputDTO) As IHttpActionResult
        If input Is Nothing Then Return BadRequest("Debe enviar los datos del producto.")

        Dim resultado = servicio.Actualizar(id, input)

        Select Case resultado.Estado
            Case ResultadoOperacionProducto.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionProducto.Invalido
                Return BadRequest(resultado.Mensaje)
            Case ResultadoOperacionProducto.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok(resultado.Producto)
        End Select
    End Function

    <HttpDelete>
    <Route("api/productos/{id:int}")>
    <JwtAuthorize(Roles:="Administrador")>
    Public Function Eliminar(id As Integer) As IHttpActionResult
        Dim resultado = servicio.Eliminar(id)

        Select Case resultado.Estado
            Case ResultadoOperacionProducto.NoEncontrado
                Return NotFound()
            Case ResultadoOperacionProducto.EnUso
                Return Content(HttpStatusCode.Conflict, resultado.Mensaje)
            Case ResultadoOperacionProducto.Fallo
                Return Content(HttpStatusCode.InternalServerError, resultado.Mensaje)
            Case Else
                Return Ok()
        End Select
    End Function

End Class
