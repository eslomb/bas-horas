Imports System.Linq
Imports DTOs.Reportes
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ReporteGeneralService
        Implements IReporteGeneralService

        Private ReadOnly repo As IReporteGeneralRepository
        Private ReadOnly usuarios As IUsuarioRepository

        Public Sub New(repo As IReporteGeneralRepository, usuarios As IUsuarioRepository)
            Me.repo = repo
            Me.usuarios = usuarios
        End Sub

        Public Function GenerarReporteGeneral(filtros As FiltroReporte) As List(Of ReporteFilaDTO) Implements IReporteGeneralService.GenerarReporteGeneral
            Dim perfiles As New Dictionary(Of String, ProfileCommon)(StringComparer.OrdinalIgnoreCase)
            Dim filas = ObtenerFilas(filtros, perfiles)

            CargarPerfiles(perfiles, filas)

            Return filas.Select(Function(r) MapearDTO(r, perfiles)).ToList()
        End Function

        ' PAGINADO DESACTIVADO
        'Public Function GenerarReporteGeneralPaginado(filtros As FiltroReporte, pagina As Integer, tamanioPagina As Integer) As ResultadoPaginadoDTO(Of ReporteFilaDTO) Implements IReporteGeneralService.GenerarReporteGeneralPaginado
        '    Dim perfiles As New Dictionary(Of String, ProfileCommon)(StringComparer.OrdinalIgnoreCase)
        '    Dim filas = ObtenerFilas(filtros, perfiles)
        '
        '    ' Orden estable (el IDReg desempata) para que Skip/Take no repita ni saltee filas entre páginas.
        '    Dim ordenadas = filas.
        '        OrderByDescending(Function(r) r.FECHA_ACTIVIDAD).
        '        ThenBy(Function(r) r.USUARIO).
        '        ThenBy(Function(r) r.IDReg).
        '        ToList()
        '
        '    Dim total = ordenadas.Count
        '    Dim filasPagina = ordenadas.Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina).ToList()
        '
        '    ' Solo se consultan los Profile de los usuarios de la página pedida.
        '    CargarPerfiles(perfiles, filasPagina)
        '
        '    Return New ResultadoPaginadoDTO(Of ReporteFilaDTO) With {
        '        .Items = filasPagina.Select(Function(r) MapearDTO(r, perfiles)).ToList(),
        '        .Pagina = pagina,
        '        .TamanioPagina = tamanioPagina,
        '        .TotalRegistros = total,
        '        .TotalPaginas = CInt(Math.Ceiling(total / CDbl(tamanioPagina)))
        '    }
        'End Function

        ' Ejecuta el SP y aplica el filtro por Centro (el único que requiere el Profile de todos los usuarios).
        Private Function ObtenerFilas(filtros As FiltroReporte, perfiles As Dictionary(Of String, ProfileCommon)) As List(Of spReporteGeneral_Result)
            Dim filas = repo.EjecutarReporteGeneral(filtros.Desde, filtros.Hasta, filtros.Usuario, filtros.Idprod, filtros.IdAct, filtros.CodPais)

            If String.IsNullOrWhiteSpace(filtros.Centro) Then Return filas

            Dim centros As New HashSet(Of String)(filtros.Centro.Replace(" ", "").Split(","c), StringComparer.OrdinalIgnoreCase)

            CargarPerfiles(perfiles, filas)

            Return filas.Where(Function(r) perfiles.ContainsKey(r.USUARIO) AndAlso
                                           Not String.IsNullOrEmpty(perfiles(r.USUARIO).Centro) AndAlso
                                           centros.Contains(perfiles(r.USUARIO).Centro)).ToList()
        End Function

        ' Cache de perfiles: una consulta de Profile por usuario único que todavía no esté en el diccionario.
        Private Sub CargarPerfiles(perfiles As Dictionary(Of String, ProfileCommon), filas As List(Of spReporteGeneral_Result))
            Dim pendientes = filas.
                Select(Function(r) r.USUARIO).
                Where(Function(u) Not String.IsNullOrWhiteSpace(u) AndAlso Not perfiles.ContainsKey(u)).
                Distinct(StringComparer.OrdinalIgnoreCase).
                ToList()

            For Each usuario In pendientes
                perfiles(usuario) = usuarios.ObtenerPerfil(usuario)
            Next
        End Sub

        Private Shared Function MapearDTO(r As spReporteGeneral_Result, perfiles As Dictionary(Of String, ProfileCommon)) As ReporteFilaDTO
            Dim dto As New ReporteFilaDTO With {
                .IDReg = r.IDReg,
                .FechaActividad = r.FECHA_ACTIVIDAD,
                .Usuario = r.USUARIO,
                .CodActividad = r.COD_ACTIVIDAD,
                .Actividad = r.ACTIVIDAD,
                .Pais = r.PAIS,
                .IdProducto = r.ID_PRODUCTO,
                .Producto = r.PRODUCTO,
                .Horas = r.HORAS,
                .Minutos = r.MINUTOS,
                .Observaciones = r.OBSERVACIONES
            }

            Dim perfil As ProfileCommon = Nothing
            If Not String.IsNullOrWhiteSpace(r.USUARIO) AndAlso perfiles.TryGetValue(r.USUARIO, perfil) Then
                dto.Legajo = perfil.Legajo
                dto.Jornada = perfil.Modalidad
                dto.Centro = perfil.Centro
                dto.CUIL = perfil.CUIL
            End If

            Return dto
        End Function
    End Class
End Namespace
