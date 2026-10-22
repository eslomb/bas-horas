Imports System.Linq
Imports DTOs.Productos
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ProductoService
        Implements IProductoService

        Private Const LongitudMaximaCodigo As Integer = 10
        Private Const LongitudMaximaNombre As Integer = 400

        Private ReadOnly repo As IProductoRepository

        Public Sub New(repo As IProductoRepository)
            Me.repo = repo
        End Sub

        Public Function Listar(soloActivos As Boolean) As List(Of ProductoDTO) Implements IProductoService.Listar
            Dim productos = If(soloActivos, repo.ObtenerActivos(), repo.ObtenerTodos()).ToList()

            Return productos.
                OrderBy(Function(p) p.Producto).
                Select(Function(p) MapearDTO(p)).
                ToList()
        End Function

        Public Function ObtenerPorId(id As Integer) As ResultadoProducto Implements IProductoService.ObtenerPorId
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.NoEncontrado}
            End If

            Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Ok, .Producto = MapearDTO(entidad)}
        End Function

        Public Function Crear(input As ProductoInputDTO) As ResultadoProducto Implements IProductoService.Crear
            Dim mensaje = Validar(input)
            If mensaje IsNot Nothing Then Return Invalido(mensaje)

            Dim entidad As New PRODUCTOS With {
                .Codigo = input.Codigo.Trim(),
                .Producto = input.Producto.Trim(),
                .Activo = If(input.Activo, True),
                .FProyIni = input.FProyIni,
                .FProyFin = input.FProyFin
            }

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Fallo, .Mensaje = "No se pudo guardar el producto."}
            End Try

            Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Ok, .Producto = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(id As Integer, input As ProductoInputDTO) As ResultadoProducto Implements IProductoService.Actualizar
            Dim mensaje = Validar(input)
            If mensaje IsNot Nothing Then Return Invalido(mensaje)

            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.NoEncontrado}
            End If

            entidad.Codigo = input.Codigo.Trim()
            entidad.Producto = input.Producto.Trim()
            entidad.Activo = If(input.Activo, True)
            entidad.FProyIni = input.FProyIni
            entidad.FProyFin = input.FProyFin

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Fallo, .Mensaje = "No se pudo actualizar el producto."}
            End Try

            Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Ok, .Producto = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(id As Integer) As ResultadoProducto Implements IProductoService.Eliminar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.NoEncontrado}
            End If

            If repo.TieneRegistros(id) Then
                Return New ResultadoProducto With {
                    .Estado = ResultadoOperacionProducto.EnUso,
                    .Mensaje = "No se puede eliminar: hay registros de horas cargados con ese producto. Desactivalo en su lugar."
                }
            End If

            If repo.TieneProyectos(id) Then
                Return New ResultadoProducto With {
                    .Estado = ResultadoOperacionProducto.EnUso,
                    .Mensaje = "No se puede eliminar: hay proyectos asociados a ese producto. Desactivalo en su lugar."
                }
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Fallo, .Mensaje = "No se pudo eliminar el producto."}
            End Try

            Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Ok}
        End Function

        ' Devuelve el mensaje de error o Nothing si el input es válido.
        Private Shared Function Validar(input As ProductoInputDTO) As String
            Dim codigo = If(input.Codigo, "").Trim()
            Dim nombre = If(input.Producto, "").Trim()

            If codigo.Length = 0 OrElse codigo.Length > LongitudMaximaCodigo Then
                Return "El código es obligatorio y puede tener hasta " & LongitudMaximaCodigo & " caracteres."
            End If
            If nombre.Length = 0 OrElse nombre.Length > LongitudMaximaNombre Then
                Return "El nombre del producto es obligatorio y puede tener hasta " & LongitudMaximaNombre & " caracteres."
            End If
            If input.FProyIni.HasValue AndAlso input.FProyFin.HasValue AndAlso input.FProyIni.Value > input.FProyFin.Value Then
                Return "La fecha de inicio del proyecto no puede ser posterior a la de fin."
            End If

            Return Nothing
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoProducto
            Return New ResultadoProducto With {.Estado = ResultadoOperacionProducto.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(p As PRODUCTOS) As ProductoDTO
            Return New ProductoDTO With {
                .IDPro = p.IDPro,
                .Codigo = p.Codigo,
                .Producto = p.Producto,
                .Activo = p.Activo,
                .FProyIni = p.FProyIni,
                .FProyFin = p.FProyFin
            }
        End Function
    End Class
End Namespace
