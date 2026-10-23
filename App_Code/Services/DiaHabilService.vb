Imports System.Linq
Imports DTOs.DiasHabiles
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class DiaHabilService
        Implements IDiaHabilService

        Private Const AnioMinimo As Integer = 2000
        Private Const AnioMaximo As Integer = 2100

        Private ReadOnly repo As IDiaHabilRepository

        Public Sub New(repo As IDiaHabilRepository)
            Me.repo = repo
        End Sub

        Public Function Listar(anio As Integer?) As List(Of DiaHabilDTO) Implements IDiaHabilService.Listar
            Dim dias = repo.ObtenerTodos().ToList()

            If anio.HasValue Then
                dias = dias.Where(Function(d) d.Anio = anio.Value).ToList()
            End If

            Return dias.
                OrderBy(Function(d) d.Anio).
                ThenBy(Function(d) d.Mes).
                Select(Function(d) MapearDTO(d)).
                ToList()
        End Function

        Public Function ObtenerPorId(id As Integer) As ResultadoDiaHabil Implements IDiaHabilService.ObtenerPorId
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.NoEncontrado}
            End If

            Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Ok, .DiaHabil = MapearDTO(entidad)}
        End Function

        Public Function Crear(input As DiaHabilInputDTO) As ResultadoDiaHabil Implements IDiaHabilService.Crear
            Dim mensaje = Validar(input)
            If mensaje IsNot Nothing Then Return Invalido(mensaje)

            If repo.ExisteAnioMes(input.Anio.Value, input.Mes.Value, 0) Then Return PeriodoDuplicado()

            Dim entidad As New DIAS_HABILES With {
                .Anio = input.Anio.Value,
                .Mes = input.Mes.Value,
                .CantDias = input.CantDias.Value
            }

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Fallo, .Mensaje = "No se pudo guardar el registro."}
            End Try

            Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Ok, .DiaHabil = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(id As Integer, input As DiaHabilInputDTO) As ResultadoDiaHabil Implements IDiaHabilService.Actualizar
            Dim mensaje = Validar(input)
            If mensaje IsNot Nothing Then Return Invalido(mensaje)

            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.NoEncontrado}
            End If

            If repo.ExisteAnioMes(input.Anio.Value, input.Mes.Value, id) Then Return PeriodoDuplicado()

            entidad.Anio = input.Anio.Value
            entidad.Mes = input.Mes.Value
            entidad.CantDias = input.CantDias.Value

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Fallo, .Mensaje = "No se pudo actualizar el registro."}
            End Try

            Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Ok, .DiaHabil = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(id As Integer) As ResultadoDiaHabil Implements IDiaHabilService.Eliminar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.NoEncontrado}
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Fallo, .Mensaje = "No se pudo eliminar el registro."}
            End Try

            Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Ok}
        End Function

        ' Devuelve el mensaje de error o Nothing si el input es válido.
        Private Shared Function Validar(input As DiaHabilInputDTO) As String
            If Not input.Anio.HasValue OrElse input.Anio.Value < AnioMinimo OrElse input.Anio.Value > AnioMaximo Then
                Return "El año es obligatorio y debe estar entre " & AnioMinimo & " y " & AnioMaximo & "."
            End If
            If Not input.Mes.HasValue OrElse input.Mes.Value < 1 OrElse input.Mes.Value > 12 Then
                Return "El mes es obligatorio y debe estar entre 1 y 12."
            End If
            If Not input.CantDias.HasValue Then
                Return "La cantidad de días es obligatoria."
            End If

            Dim diasDelMes = Date.DaysInMonth(input.Anio.Value, input.Mes.Value)
            If input.CantDias.Value < 0 OrElse input.CantDias.Value > diasDelMes Then
                Return "La cantidad de días debe estar entre 0 y " & diasDelMes & " para ese mes."
            End If

            Return Nothing
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoDiaHabil
            Return New ResultadoDiaHabil With {.Estado = ResultadoOperacionDiaHabil.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function PeriodoDuplicado() As ResultadoDiaHabil
            Return New ResultadoDiaHabil With {
                .Estado = ResultadoOperacionDiaHabil.Duplicado,
                .Mensaje = "Ya existe un registro de días hábiles para ese año y mes."
            }
        End Function

        Private Shared Function MapearDTO(d As DIAS_HABILES) As DiaHabilDTO
            Return New DiaHabilDTO With {
                .Id = d.Id,
                .Anio = d.Anio,
                .Mes = d.Mes,
                .CantDias = d.CantDias
            }
        End Function
    End Class
End Namespace
