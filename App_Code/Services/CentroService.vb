Imports System.Linq
Imports DTOs.Centros
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class CentroService
        Implements ICentroService

        Private Const LongitudMaximaCodigo As Integer = 50
        Private Const LongitudMaximaNombre As Integer = 50

        Private ReadOnly repo As ICentroRepository
        Private ReadOnly usuarioRepo As IUsuarioRepository

        Public Sub New(repo As ICentroRepository, usuarioRepo As IUsuarioRepository)
            Me.repo = repo
            Me.usuarioRepo = usuarioRepo
        End Sub

        Public Function Listar() As List(Of CentroDTO) Implements ICentroService.Listar
            Return repo.ObtenerTodos().ToList().
                OrderBy(Function(c) c.Nombre).
                Select(Function(c) MapearDTO(c)).
                ToList()
        End Function

        Public Function ObtenerPorCodigo(codigo As String) As ResultadoCentro Implements ICentroService.ObtenerPorCodigo
            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.NoEncontrado}
            End If

            Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Ok, .Centro = MapearDTO(entidad)}
        End Function

        Public Function Crear(input As CentroInputDTO) As ResultadoCentro Implements ICentroService.Crear
            Dim codigo = If(input.CodCen, "").Trim()
            Dim nombre = If(input.Nombre, "").Trim()

            If codigo.Length = 0 OrElse codigo.Length > LongitudMaximaCodigo Then
                Return Invalido("El código es obligatorio y puede tener hasta " & LongitudMaximaCodigo & " caracteres.")
            End If
            If nombre.Length = 0 OrElse nombre.Length > LongitudMaximaNombre Then
                Return Invalido("El nombre del centro es obligatorio y puede tener hasta " & LongitudMaximaNombre & " caracteres.")
            End If

            If repo.ObtenerPorCodigo(codigo) IsNot Nothing Then
                Return New ResultadoCentro With {
                    .Estado = ResultadoOperacionCentro.Duplicado,
                    .Mensaje = "Ya existe un centro con ese código."
                }
            End If

            Dim entidad As New CENTROS With {.CodCen = codigo, .Nombre = nombre}

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Fallo, .Mensaje = "No se pudo guardar el centro."}
            End Try

            Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Ok, .Centro = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(codigo As String, input As CentroInputDTO) As ResultadoCentro Implements ICentroService.Actualizar
            Dim nombre = If(input.Nombre, "").Trim()

            If nombre.Length = 0 OrElse nombre.Length > LongitudMaximaNombre Then
                Return Invalido("El nombre del centro es obligatorio y puede tener hasta " & LongitudMaximaNombre & " caracteres.")
            End If

            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.NoEncontrado}
            End If

            ' El código es la clave y lo referencian los perfiles de usuario: no se modifica.
            entidad.Nombre = nombre

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Fallo, .Mensaje = "No se pudo actualizar el centro."}
            End Try

            Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Ok, .Centro = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(codigo As String) As ResultadoCentro Implements ICentroService.Eliminar
            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.NoEncontrado}
            End If

            If TieneUsuarios(entidad.CodCen) Then
                Return New ResultadoCentro With {
                    .Estado = ResultadoOperacionCentro.EnUso,
                    .Mensaje = "No se puede eliminar: hay usuarios asignados a ese centro."
                }
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Fallo, .Mensaje = "No se pudo eliminar el centro."}
            End Try

            Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Ok}
        End Function

        ' El centro de un usuario vive en su Profile de Membership (sin FK en la base),
        ' así que hay que revisar el perfil de cada usuario.
        Private Function TieneUsuarios(codigo As String) As Boolean
            Return usuarioRepo.ObtenerTodos().Any(
                Function(u)
                    Dim centro = usuarioRepo.ObtenerPerfil(u.UserName).Centro
                    Return Not String.IsNullOrEmpty(centro) AndAlso centro.Trim().Equals(codigo.Trim(), StringComparison.OrdinalIgnoreCase)
                End Function)
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoCentro
            Return New ResultadoCentro With {.Estado = ResultadoOperacionCentro.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(c As CENTROS) As CentroDTO
            Return New CentroDTO With {
                .CodCen = c.CodCen.Trim(),
                .Nombre = c.Nombre
            }
        End Function
    End Class
End Namespace
