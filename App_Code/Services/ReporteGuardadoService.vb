Imports System.Linq
Imports DTOs.Reportes
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ReporteGuardadoService
        Implements IReporteGuardadoService

        Private ReadOnly repo As IReporteGuardadoRepository

        Public Sub New(repo As IReporteGuardadoRepository)
            Me.repo = repo
        End Sub

        Public Function Listar(usuario As String) As List(Of ReporteGuardadoDTO) Implements IReporteGuardadoService.Listar
            Return repo.Listar(usuario).Select(Function(r) MapearDTO(r)).ToList()
        End Function

        Public Function ObtenerPorId(id As Guid, usuario As String) As ResultadoReporte Implements IReporteGuardadoService.ObtenerPorId
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Prohibido}
            End If

            Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Ok, .Reporte = MapearDTO(entidad)}
        End Function

        Public Function Crear(usuario As String, input As ReporteGuardadoInputDTO) As ResultadoReporte Implements IReporteGuardadoService.Crear
            Dim entidad As New REPORTES With {
                .IDRep = Guid.NewGuid(),
                .Titulo = NormalizarVacio(input.Titulo),
                .Usuario = usuario,
                .Desde = input.Desde,
                .Hasta = input.Hasta,
                .Actividad_id = input.ActividadId,
                .Producto_id = input.ProductoId,
                .CodPais = NormalizarVacio(input.CodPais),
                .DeUsuario = NormalizarVacio(input.DeUsuario),
                .DeCentro = NormalizarVacio(input.DeCentro)
            }

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Fallo, .Mensaje = "No se pudo guardar el reporte."}
            End Try

            Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Ok, .Reporte = MapearDTO(entidad)}
        End Function

        Public Function Actualizar(id As Guid, usuario As String, input As ReporteGuardadoInputDTO) As ResultadoReporte Implements IReporteGuardadoService.Actualizar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Prohibido}
            End If

            entidad.Titulo = NormalizarVacio(input.Titulo)
            entidad.Desde = input.Desde
            entidad.Hasta = input.Hasta
            entidad.Actividad_id = input.ActividadId
            entidad.Producto_id = input.ProductoId
            entidad.CodPais = NormalizarVacio(input.CodPais)
            entidad.DeUsuario = NormalizarVacio(input.DeUsuario)
            entidad.DeCentro = NormalizarVacio(input.DeCentro)

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Fallo, .Mensaje = "No se pudo actualizar el reporte."}
            End Try

            Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Ok, .Reporte = MapearDTO(entidad)}
        End Function

        Public Function Eliminar(id As Guid, usuario As String) As ResultadoReporte Implements IReporteGuardadoService.Eliminar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Prohibido}
            End If

            Try
                repo.Eliminar(id)
            Catch
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Fallo, .Mensaje = "No se pudo eliminar el reporte."}
            End Try

            Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Ok}
        End Function

        Private Shared Function NormalizarVacio(valor As String) As String
            Return If(String.IsNullOrEmpty(valor), Nothing, valor)
        End Function

        Private Shared Function MapearDTO(r As REPORTES) As ReporteGuardadoDTO
            Return New ReporteGuardadoDTO With {
                .IDRep = r.IDRep,
                .Titulo = r.Titulo,
                .Usuario = r.Usuario,
                .Desde = r.Desde,
                .Hasta = r.Hasta,
                .ActividadId = r.Actividad_id,
                .ProductoId = r.Producto_id,
                .CodPais = r.CodPais,
                .DeUsuario = r.DeUsuario,
                .DeCentro = r.DeCentro
            }
        End Function
    End Class
End Namespace
