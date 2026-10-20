Imports System.Linq
Imports DTOs.Paises
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class PaisService
        Implements IPaisService

        Private Const LongitudMaximaCodigo As Integer = 4
        Private Const LongitudMaximaNombre As Integer = 100

        Private ReadOnly repo As IPaisRepository

        Public Sub New(repo As IPaisRepository)
            Me.repo = repo
        End Sub

        Public Function Listar() As List(Of PaisDTO) Implements IPaisService.Listar
            Dim paises = repo.ObtenerTodos().ToList()

            Return paises.
                OrderByDescending(Function(p) If(p.Preferido, False)).
                ThenBy(Function(p) p.Pais).
                Select(Function(p) MapearDTO(p)).
                ToList()
        End Function

        Public Function ObtenerPorCodigo(codigo As String) As ResultadoPais Implements IPaisService.ObtenerPorCodigo
            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoPais With {.Estado = ResultadoOperacionPais.NoEncontrado}
            End If

            Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Ok, .Pais = MapearDTO(entidad)}
        End Function

        Public Function Crear(input As PaisInputDTO) As ResultadoPais Implements IPaisService.Crear
            Dim codigo = If(input.Codigo, "").Trim()
            Dim nombre = If(input.Pais, "").Trim()

            If codigo.Length = 0 OrElse codigo.Length > LongitudMaximaCodigo Then
                Return Invalido("El código es obligatorio y puede tener hasta " & LongitudMaximaCodigo & " caracteres.")
            End If
            If nombre.Length = 0 OrElse nombre.Length > LongitudMaximaNombre Then
                Return Invalido("El nombre del país es obligatorio y puede tener hasta " & LongitudMaximaNombre & " caracteres.")
            End If

            If repo.ObtenerPorCodigo(codigo) IsNot Nothing Then
                Return New ResultadoPais With {
                    .Estado = ResultadoOperacionPais.Duplicado,
                    .Mensaje = "Ya existe un país con ese código."
                }
            End If

            Dim entidad As New PAISES With {.Codigo = codigo, .Pais = nombre, .Preferido = input.Preferido}

            ' Los cambios de los otros países se guardan junto con el alta (mismo SaveChanges).
            If input.Preferido = True Then DesmarcarOtrosPreferidos(codigo)

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Fallo, .Mensaje = "No se pudo guardar el país."}
            End Try

            Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Ok, .Pais = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(codigo As String, input As PaisInputDTO) As ResultadoPais Implements IPaisService.Actualizar
            Dim nombre = If(input.Pais, "").Trim()

            If nombre.Length = 0 OrElse nombre.Length > LongitudMaximaNombre Then
                Return Invalido("El nombre del país es obligatorio y puede tener hasta " & LongitudMaximaNombre & " caracteres.")
            End If

            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoPais With {.Estado = ResultadoOperacionPais.NoEncontrado}
            End If

            ' El código es la clave y lo referencian los registros: no se modifica.
            entidad.Pais = nombre
            entidad.Preferido = input.Preferido

            If input.Preferido = True Then DesmarcarOtrosPreferidos(entidad.Codigo)

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Fallo, .Mensaje = "No se pudo actualizar el país."}
            End Try

            Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Ok, .Pais = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(codigo As String) As ResultadoPais Implements IPaisService.Eliminar
            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoPais With {.Estado = ResultadoOperacionPais.NoEncontrado}
            End If

            If entidad.Preferido = True Then
                Return New ResultadoPais With {
                    .Estado = ResultadoOperacionPais.EsPreferido,
                    .Mensaje = "No se puede eliminar el país preferido. Marcá otro país como preferido primero."
                }
            End If

            If repo.TieneRegistros(entidad.Codigo) Then
                Return New ResultadoPais With {
                    .Estado = ResultadoOperacionPais.EnUso,
                    .Mensaje = "No se puede eliminar: hay registros de horas cargados con ese país."
                }
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Fallo, .Mensaje = "No se pudo eliminar el país."}
            End Try

            Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Ok}
        End Function

        ' Regla de negocio: hay un único país preferido. Las entidades quedan modificadas en el
        ' contexto y se persisten con el SaveChanges de la operación que las invoca.
        Private Sub DesmarcarOtrosPreferidos(codigo As String)
            For Each otro In repo.ObtenerPreferidos().ToList()
                If Not otro.Codigo.Trim().Equals(codigo, StringComparison.OrdinalIgnoreCase) Then
                    otro.Preferido = False
                End If
            Next
        End Sub

        Private Shared Function Invalido(mensaje As String) As ResultadoPais
            Return New ResultadoPais With {.Estado = ResultadoOperacionPais.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(p As PAISES) As PaisDTO
            Return New PaisDTO With {
                .Codigo = p.Codigo.Trim(),
                .Pais = p.Pais,
                .Preferido = p.Preferido
            }
        End Function
    End Class
End Namespace
