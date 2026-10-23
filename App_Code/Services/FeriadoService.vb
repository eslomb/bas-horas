Imports System.Linq
Imports DTOs.Feriados
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class FeriadoService
        Implements IFeriadoService

        Private Const LongitudMaximaMotivo As Integer = 50

        Private ReadOnly repo As IFeriadoRepository

        Public Sub New(repo As IFeriadoRepository)
            Me.repo = repo
        End Sub

        Public Function Listar(anio As Integer?) As List(Of FeriadoDTO) Implements IFeriadoService.Listar
            Dim feriados = repo.ObtenerTodos().ToList()

            If anio.HasValue Then
                feriados = feriados.Where(Function(f) f.Feriado.Year = anio.Value).ToList()
            End If

            Return feriados.
                OrderBy(Function(f) f.Feriado).
                Select(Function(f) MapearDTO(f)).
                ToList()
        End Function

        Public Function ObtenerPorFecha(fecha As Date) As ResultadoFeriado Implements IFeriadoService.ObtenerPorFecha
            Dim entidad = repo.ObtenerPorFecha(fecha.Date)

            If entidad Is Nothing Then
                Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.NoEncontrado}
            End If

            Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Ok, .Feriado = MapearDTO(entidad)}
        End Function

        Public Function Crear(input As FeriadoInputDTO) As ResultadoFeriado Implements IFeriadoService.Crear
            If Not input.Feriado.HasValue Then Return Invalido("La fecha del feriado es obligatoria.")

            Dim motivo = NormalizarMotivo(input.Motivo)
            If motivo IsNot Nothing AndAlso motivo.Length > LongitudMaximaMotivo Then
                Return Invalido("El motivo puede tener hasta " & LongitudMaximaMotivo & " caracteres.")
            End If

            Dim fecha = input.Feriado.Value.Date

            If repo.ObtenerPorFecha(fecha) IsNot Nothing Then
                Return New ResultadoFeriado With {
                    .Estado = ResultadoOperacionFeriado.Duplicado,
                    .Mensaje = "Ya existe un feriado en esa fecha."
                }
            End If

            Dim entidad As New FERIADOS With {.Feriado = fecha, .Motivo = motivo}

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Fallo, .Mensaje = "No se pudo guardar el feriado."}
            End Try

            Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Ok, .Feriado = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(fecha As Date, input As FeriadoInputDTO) As ResultadoFeriado Implements IFeriadoService.Actualizar
            Dim motivo = NormalizarMotivo(input.Motivo)
            If motivo IsNot Nothing AndAlso motivo.Length > LongitudMaximaMotivo Then
                Return Invalido("El motivo puede tener hasta " & LongitudMaximaMotivo & " caracteres.")
            End If

            Dim entidad = repo.ObtenerPorFecha(fecha.Date)

            If entidad Is Nothing Then
                Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.NoEncontrado}
            End If

            ' La fecha es la clave: no se modifica (para moverlo, se da de baja y de alta).
            entidad.Motivo = motivo

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Fallo, .Mensaje = "No se pudo actualizar el feriado."}
            End Try

            Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Ok, .Feriado = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(fecha As Date) As ResultadoFeriado Implements IFeriadoService.Eliminar
            Dim entidad = repo.ObtenerPorFecha(fecha.Date)

            If entidad Is Nothing Then
                Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.NoEncontrado}
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Fallo, .Mensaje = "No se pudo eliminar el feriado."}
            End Try

            Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Ok}
        End Function

        ' El motivo es opcional (columna nullable): vacío se guarda como NULL.
        Private Shared Function NormalizarMotivo(motivo As String) As String
            Dim limpio = If(motivo, "").Trim()
            Return If(limpio.Length = 0, Nothing, limpio)
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoFeriado
            Return New ResultadoFeriado With {.Estado = ResultadoOperacionFeriado.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(f As FERIADOS) As FeriadoDTO
            Return New FeriadoDTO With {
                .Feriado = f.Feriado,
                .Motivo = f.Motivo
            }
        End Function
    End Class
End Namespace
