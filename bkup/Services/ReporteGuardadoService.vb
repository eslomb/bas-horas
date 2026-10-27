Imports System.Linq
Imports DTOs.Reportes
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ReporteGuardadoService
        Implements IReporteGuardadoService

        Private ReadOnly repo As IReporteGuardadoRepository
        Private ReadOnly reporteGeneral As IReporteGeneralService
        Private ReadOnly exportacion As IReporteExportacionService

        Public Sub New(repo As IReporteGuardadoRepository, reporteGeneral As IReporteGeneralService, exportacion As IReporteExportacionService)
            Me.repo = repo
            Me.reporteGeneral = reporteGeneral
            Me.exportacion = exportacion
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

        Public Function EjecutarPorId(id As Guid, usuario As String) As ResultadoReporte Implements IReporteGuardadoService.EjecutarPorId
            Dim entidad As REPORTES = Nothing
            Dim rechazo = ValidarAcceso(id, usuario, entidad)
            If rechazo IsNot Nothing Then Return rechazo

            Return New ResultadoReporte With {
                .Estado = ResultadoOperacionReporte.Ok,
                .Reporte = MapearDTO(entidad),
                .Filas = reporteGeneral.GenerarReporteGeneral(ArmarFiltros(entidad, usuario))
            }
        End Function

        ' PAGINADO DESACTIVADO
        'Public Function EjecutarPorIdPaginado(id As Guid, usuario As String, pagina As Integer, tamanioPagina As Integer) As ResultadoReporte Implements IReporteGuardadoService.EjecutarPorIdPaginado
        '    Dim entidad As REPORTES = Nothing
        '    Dim rechazo = ValidarAcceso(id, usuario, entidad)
        '    If rechazo IsNot Nothing Then Return rechazo
        '
        '    Return New ResultadoReporte With {
        '        .Estado = ResultadoOperacionReporte.Ok,
        '        .Reporte = MapearDTO(entidad),
        '        .FilasPaginadas = reporteGeneral.GenerarReporteGeneralPaginado(ArmarFiltros(entidad, usuario), pagina, tamanioPagina)
        '    }
        'End Function

        Public Function ExportarPorId(id As Guid, usuario As String, esAdministrador As Boolean) As ResultadoReporte Implements IReporteGuardadoService.ExportarPorId
            Dim entidad As REPORTES = Nothing
            Dim rechazo = ValidarAcceso(id, usuario, entidad)
            If rechazo IsNot Nothing Then Return rechazo

            Try
                Return New ResultadoReporte With {
                    .Estado = ResultadoOperacionReporte.Ok,
                    .Reporte = MapearDTO(entidad),
                    .Archivo = exportacion.GenerarExcel(ArmarFiltros(entidad, usuario), entidad.Titulo, usuario, esAdministrador)
                }
            Catch
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Fallo, .Mensaje = "No se pudo generar el archivo Excel."}
            End Try
        End Function

        ' Devuelve Nothing si el reporte existe y es del usuario (y lo deja en entidad); si no, el resultado de rechazo.
        Private Function ValidarAcceso(id As Guid, usuario As String, ByRef entidad As REPORTES) As ResultadoReporte
            entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Prohibido}
            End If

            Return Nothing
        End Function

        Private Shared Function ArmarFiltros(entidad As REPORTES, usuario As String) As FiltroReporte
            ' El SP trata 0 como "todos" en producto/actividad.
            ' El Usuario del SP es siempre el logueado (no entidad.DeUsuario): el reporte solo trae sus propias tareas.
            Return New FiltroReporte With {
                .Desde = entidad.Desde,
                .Hasta = entidad.Hasta,
                .Usuario = usuario,
                .Idprod = If(entidad.Producto_id, 0),
                .IdAct = If(entidad.Actividad_id, 0),
                .CodPais = entidad.CodPais,
                .Centro = entidad.DeCentro
            }
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
