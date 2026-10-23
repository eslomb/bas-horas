Imports System.Linq
Imports DTOs.Configuraciones
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ConfiguracionService
        Implements IConfiguracionService

        Private Const LongitudMaximaValor As Integer = 50

        Private ReadOnly repo As IConfiguracionRepository

        Public Sub New(repo As IConfiguracionRepository)
            Me.repo = repo
        End Sub

        Public Function Listar() As List(Of ConfiguracionDTO) Implements IConfiguracionService.Listar
            Return repo.ObtenerTodas().ToList().
                Select(Function(c) MapearDTO(c)).
                ToList()
        End Function

        Public Function ObtenerPorCodigo(codigo As String) As ResultadoConfiguracion Implements IConfiguracionService.ObtenerPorCodigo
            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoConfiguracion With {.Estado = ResultadoOperacionConfiguracion.NoEncontrado}
            End If

            Return New ResultadoConfiguracion With {.Estado = ResultadoOperacionConfiguracion.Ok, .Configuracion = MapearDTO(entidad)}
        End Function

        Public Function ActualizarValor(codigo As String, input As ConfiguracionInputDTO) As ResultadoConfiguracion Implements IConfiguracionService.ActualizarValor
            If input.Valor Is Nothing Then
                Return Invalido("El valor es obligatorio.")
            End If

            Dim valor = input.Valor.Trim()

            If valor.Length > LongitudMaximaValor Then
                Return Invalido("El valor puede tener hasta " & LongitudMaximaValor & " caracteres.")
            End If

            Dim entidad = repo.ObtenerPorCodigo(If(codigo, "").Trim())

            If entidad Is Nothing Then
                Return New ResultadoConfiguracion With {.Estado = ResultadoOperacionConfiguracion.NoEncontrado}
            End If

            ' La descripción y el código no se tocan.
            entidad.Valor = valor

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoConfiguracion With {.Estado = ResultadoOperacionConfiguracion.Fallo, .Mensaje = "No se pudo actualizar la configuración."}
            End Try

            Return New ResultadoConfiguracion With {.Estado = ResultadoOperacionConfiguracion.Ok, .Configuracion = MapearDTO(entidad)}
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoConfiguracion
            Return New ResultadoConfiguracion With {.Estado = ResultadoOperacionConfiguracion.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(c As CONFIGURACIONES) As ConfiguracionDTO
            Return New ConfiguracionDTO With {
                .CodConfig = c.CodConfig,
                .Valor = c.Valor,
                .Descripcion = c.Descripcion
            }
        End Function
    End Class
End Namespace
