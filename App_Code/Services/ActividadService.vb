Imports System.Linq
Imports DTOs.Actividades
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ActividadService
        Implements IActividadService

        Private Const LongitudMaximaCodigo As Integer = 10
        Private Const LongitudMaximaNombre As Integer = 400

        Private ReadOnly repo As IActividadRepository

        Public Sub New(repo As IActividadRepository)
            Me.repo = repo
        End Sub

        Public Function Listar(soloHabilitadas As Boolean) As List(Of ActividadDTO) Implements IActividadService.Listar
            Dim actividades = If(soloHabilitadas, repo.ObtenerHabilitadas(), repo.ObtenerTodas()).ToList()

            Return actividades.
                OrderBy(Function(a) a.Actividad).
                Select(Function(a) MapearDTO(a)).
                ToList()
        End Function

        Public Function ObtenerPorId(id As Integer) As ResultadoActividad Implements IActividadService.ObtenerPorId
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.NoEncontrado}
            End If

            Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Ok, .Actividad = MapearDTO(entidad)}
        End Function

        Public Function Crear(input As ActividadInputDTO) As ResultadoActividad Implements IActividadService.Crear
            Dim mensaje = Validar(input)
            If mensaje IsNot Nothing Then Return Invalido(mensaje)

            Dim codigo = input.Codigo.Trim()
            If repo.ExisteCodigo(codigo, 0) Then Return CodigoDuplicado()

            Dim entidad As New ACTIVIDADES With {
                .Codigo = codigo,
                .Actividad = input.Actividad.Trim(),
                .Habilitada = If(input.Habilitada, True)
            }

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Fallo, .Mensaje = "No se pudo guardar la actividad."}
            End Try

            Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Ok, .Actividad = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(id As Integer, input As ActividadInputDTO) As ResultadoActividad Implements IActividadService.Actualizar
            Dim mensaje = Validar(input)
            If mensaje IsNot Nothing Then Return Invalido(mensaje)

            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.NoEncontrado}
            End If

            Dim codigo = input.Codigo.Trim()
            If repo.ExisteCodigo(codigo, id) Then Return CodigoDuplicado()

            entidad.Codigo = codigo
            entidad.Actividad = input.Actividad.Trim()
            entidad.Habilitada = If(input.Habilitada, True)

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Fallo, .Mensaje = "No se pudo actualizar la actividad."}
            End Try

            Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Ok, .Actividad = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(id As Integer) As ResultadoActividad Implements IActividadService.Eliminar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.NoEncontrado}
            End If

            If repo.TieneRegistros(id) Then
                Return New ResultadoActividad With {
                    .Estado = ResultadoOperacionActividad.EnUso,
                    .Mensaje = "No se puede eliminar: hay registros de horas cargados con esa actividad. Deshabilitala en su lugar."
                }
            End If

            If repo.TieneTareas(id) Then
                Return New ResultadoActividad With {
                    .Estado = ResultadoOperacionActividad.EnUso,
                    .Mensaje = "No se puede eliminar: hay tareas asociadas a esa actividad. Deshabilitala en su lugar."
                }
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Fallo, .Mensaje = "No se pudo eliminar la actividad."}
            End Try

            Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Ok}
        End Function

        ' Devuelve el mensaje de error o Nothing si el input es válido.
        Private Shared Function Validar(input As ActividadInputDTO) As String
            Dim codigo = If(input.Codigo, "").Trim()
            Dim nombre = If(input.Actividad, "").Trim()

            If codigo.Length = 0 OrElse codigo.Length > LongitudMaximaCodigo Then
                Return "El código es obligatorio y puede tener hasta " & LongitudMaximaCodigo & " caracteres."
            End If
            If nombre.Length = 0 OrElse nombre.Length > LongitudMaximaNombre Then
                Return "El nombre de la actividad es obligatorio y puede tener hasta " & LongitudMaximaNombre & " caracteres."
            End If

            Return Nothing
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoActividad
            Return New ResultadoActividad With {.Estado = ResultadoOperacionActividad.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function CodigoDuplicado() As ResultadoActividad
            Return New ResultadoActividad With {
                .Estado = ResultadoOperacionActividad.Duplicado,
                .Mensaje = "Ya existe una actividad con ese código."
            }
        End Function

        Private Shared Function MapearDTO(a As ACTIVIDADES) As ActividadDTO
            Return New ActividadDTO With {
                .IDAct = a.IDAct,
                .Codigo = a.Codigo,
                .Actividad = a.Actividad,
                .Habilitada = a.Habilitada
            }
        End Function
    End Class
End Namespace
