Imports System.Linq
Imports DTOs.Reportes
Imports Repositories.Interfaces
Imports Services.Interfaces
Imports Utils

Namespace Services
    ' Registros de un reporte guardado (spReporteGeneral con los filtros del reporte), según quién los pide:
    '  - Qué registros ve (rol):
    '      Basico        -> solo los propios (DeUsuario del reporte se ignora).
    '      Avanzado      -> los propios y los de usuarios Basicos (403 si DeUsuario es otro usuario).
    '      Administrador -> todos (o solo DeUsuario si el reporte lo define).
    '  - Qué campos ve (nivel): Basico < Avanzado (Avanzado/Administrador) < Full (usuarios de UsuariosNivelFull).
    Public Class ReporteRegistrosService
        Implements IReporteRegistrosService

        ' Prefijos de usuario (sin distinguir mayúsculas) con nivel Full. Mismo criterio que habilitarRemuneraciones de app/.
        Private Shared ReadOnly UsuariosNivelFull As String() = {"ROCCHI", "CARRACEDO"}

        ' Orden de las primeras columnas del Excel (el resto sigue el orden de los DTOs). Basico no tiene Usuario/Legajo/CUIL y se saltean.
        Private Shared ReadOnly ColumnasExcelPrimero As String() = {"FechaActividad", "Usuario", "Legajo", "CUIL", "Jornada"}

        Private ReadOnly reportes As IReporteGuardadoRepository
        Private ReadOnly reporteGeneral As IReporteGeneralRepository
        Private ReadOnly usuarios As IUsuarioRepository
        Private ReadOnly remuneraciones As IRemuneracionRepository
        Private ReadOnly diasHabiles As IDiaHabilRepository

        Public Sub New(reportes As IReporteGuardadoRepository,
                       reporteGeneral As IReporteGeneralRepository,
                       usuarios As IUsuarioRepository,
                       remuneraciones As IRemuneracionRepository,
                       diasHabiles As IDiaHabilRepository)
            Me.reportes = reportes
            Me.reporteGeneral = reporteGeneral
            Me.usuarios = usuarios
            Me.remuneraciones = remuneraciones
            Me.diasHabiles = diasHabiles
        End Sub

        ' Resultado de Preparar: filas ya filtradas por rol y centro, listas para paginar/mapear.
        Private Class Consulta
            Public Filas As List(Of spReporteGeneral_Result)
            Public Perfiles As Dictionary(Of String, ProfileCommon)
            Public Nivel As NivelDetalleReporte
            Public Titulo As String
        End Class

        Public Function ObtenerRegistros(id As Guid, usuario As String, pagina As Integer, tamanioPagina As Integer) As ResultadoReporte Implements IReporteRegistrosService.ObtenerRegistros
            Dim consulta As Consulta = Nothing
            Dim rechazo = Preparar(id, usuario, consulta)
            If rechazo IsNot Nothing Then Return rechazo

            ' Orden determinista antes de paginar (mismo que el dashboard), para no repetir ni saltear filas entre páginas.
            Dim filasPagina = consulta.Filas.
                OrderByDescending(Function(f) f.FECHA_ACTIVIDAD).
                ThenByDescending(Function(f) f.IDReg).
                Skip((pagina - 1) * tamanioPagina).
                Take(tamanioPagina).
                ToList()

            ' Perfiles y mapeo solo para la página pedida.
            CargarPerfiles(consulta.Perfiles, filasPagina)

            Return New ResultadoReporte With {
                .Estado = ResultadoOperacionReporte.Ok,
                .Registros = New DTOs.ResultadoPaginado(Of RegistroReporteBasicoDTO) With {
                    .Items = Mapear(filasPagina, consulta.Perfiles, consulta.Nivel),
                    .TotalRegistros = consulta.Filas.Count,
                    .Pagina = pagina,
                    .TamanioPagina = tamanioPagina
                }
            }
        End Function

        ' Todas las filas, sin paginar, en el orden del SP.
        Public Function Exportar(id As Guid, usuario As String) As ResultadoReporte Implements IReporteRegistrosService.Exportar
            Dim consulta As Consulta = Nothing
            Dim rechazo = Preparar(id, usuario, consulta)
            If rechazo IsNot Nothing Then Return rechazo

            CargarPerfiles(consulta.Perfiles, consulta.Filas)

            Try
                Dim registros = Mapear(consulta.Filas, consulta.Perfiles, consulta.Nivel)
                Return New ResultadoReporte With {
                    .Estado = ResultadoOperacionReporte.Ok,
                    .Archivo = ExportadorExcel.Generar(registros, TipoDTO(consulta.Nivel), "Registros", ColumnasExcelPrimero),
                    .NombreArchivo = NombreArchivo(consulta.Titulo)
                }
            Catch
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Fallo, .Mensaje = "No se pudo generar el archivo Excel."}
            End Try
        End Function

        ' Valida el reporte (existe, es propio, DeUsuario permitido), ejecuta el SP y filtra por rol y centro.
        ' Devuelve Nothing y deja la consulta armada; si no, el resultado de rechazo.
        Private Function Preparar(id As Guid, usuario As String, ByRef consulta As Consulta) As ResultadoReporte
            Dim reporte = reportes.ObtenerPorId(id)

            If reporte Is Nothing Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.NoEncontrado}
            End If

            If Not reporte.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoReporte With {.Estado = ResultadoOperacionReporte.Prohibido}
            End If

            Dim roles = usuarios.ObtenerRoles(usuario)
            Dim esAdministrador = roles.Contains(Aplicacion.Grupos.Administrador, StringComparer.OrdinalIgnoreCase)
            Dim esAvanzado = roles.Contains(Aplicacion.Grupos.Avanzado, StringComparer.OrdinalIgnoreCase)

            ' Usuario que se le pasa al SP (Nothing = todos) y, para Avanzado, a quiénes puede ver.
            Dim usuarioSP As String
            Dim visibles As HashSet(Of String) = Nothing

            If esAdministrador Then
                usuarioSP = reporte.DeUsuario
            ElseIf esAvanzado Then
                visibles = New HashSet(Of String)(usuarios.ObtenerUsuariosEnRol(Aplicacion.Grupos.Basico), StringComparer.OrdinalIgnoreCase)
                visibles.Add(usuario)

                If reporte.DeUsuario IsNot Nothing AndAlso Not visibles.Contains(reporte.DeUsuario) Then
                    Return New ResultadoReporte With {
                        .Estado = ResultadoOperacionReporte.Prohibido,
                        .Mensaje = "Solo puede consultar sus propios registros y los de usuarios Basicos."
                    }
                End If
                usuarioSP = reporte.DeUsuario
            Else
                usuarioSP = usuario
            End If

            ' El SP trata 0 como "todos" en producto/actividad.
            Dim filas = reporteGeneral.EjecutarReporteGeneral(reporte.Desde, reporte.Hasta, usuarioSP,
                                                              If(reporte.Producto_id, 0), If(reporte.Actividad_id, 0), reporte.CodPais)

            If visibles IsNot Nothing Then
                filas = filas.Where(Function(f) f.USUARIO IsNot Nothing AndAlso visibles.Contains(f.USUARIO)).ToList()
            End If

            Dim perfiles As New Dictionary(Of String, ProfileCommon)(StringComparer.OrdinalIgnoreCase)

            ' El filtro por Centro es el único que necesita el Profile de todos los usuarios, no solo los de la página.
            If Not String.IsNullOrWhiteSpace(reporte.DeCentro) Then
                CargarPerfiles(perfiles, filas)
                Dim centros As New HashSet(Of String)(reporte.DeCentro.Replace(" ", "").Split(","c), StringComparer.OrdinalIgnoreCase)
                filas = filas.Where(Function(f) centros.Contains(CentroDe(f.USUARIO, perfiles))).ToList()
            End If

            consulta = New Consulta With {
                .Filas = filas,
                .Perfiles = perfiles,
                .Nivel = DeterminarNivel(usuario, esAdministrador OrElse esAvanzado),
                .Titulo = reporte.Titulo
            }
            Return Nothing
        End Function

        Private Shared Function DeterminarNivel(usuario As String, esAvanzadoOAdmin As Boolean) As NivelDetalleReporte
            If UsuariosNivelFull.Any(Function(p) usuario.StartsWith(p, StringComparison.OrdinalIgnoreCase)) Then Return NivelDetalleReporte.Full
            If esAvanzadoOAdmin Then Return NivelDetalleReporte.Avanzado
            Return NivelDetalleReporte.Basico
        End Function

        Private Shared Function TipoDTO(nivel As NivelDetalleReporte) As Type
            Select Case nivel
                Case NivelDetalleReporte.Full : Return GetType(RegistroReporteFullDTO)
                Case NivelDetalleReporte.Avanzado : Return GetType(RegistroReporteAvanzadoDTO)
                Case Else : Return GetType(RegistroReporteBasicoDTO)
            End Select
        End Function

        ' Un Profile por usuario único, no por fila; solo los que todavía no están en el diccionario.
        Private Sub CargarPerfiles(perfiles As Dictionary(Of String, ProfileCommon), filas As List(Of spReporteGeneral_Result))
            Dim pendientes = filas.
                Select(Function(f) f.USUARIO).
                Where(Function(x) Not String.IsNullOrWhiteSpace(x) AndAlso Not perfiles.ContainsKey(x)).
                Distinct(StringComparer.OrdinalIgnoreCase).
                ToList()

            For Each u In pendientes
                perfiles(u) = usuarios.ObtenerPerfil(u)
            Next
        End Sub

        Private Shared Function PerfilDe(usuario As String, perfiles As Dictionary(Of String, ProfileCommon)) As ProfileCommon
            Dim perfil As ProfileCommon = Nothing
            If usuario IsNot Nothing Then perfiles.TryGetValue(usuario, perfil)
            Return perfil
        End Function

        Private Shared Function CentroDe(usuario As String, perfiles As Dictionary(Of String, ProfileCommon)) As String
            Dim perfil = PerfilDe(usuario, perfiles)
            Return If(perfil IsNot Nothing, perfil.Centro, Nothing)
        End Function

        Private Function Mapear(filas As List(Of spReporteGeneral_Result),
                                perfiles As Dictionary(Of String, ProfileCommon),
                                nivel As NivelDetalleReporte) As List(Of RegistroReporteBasicoDTO)
            ' Remuneraciones y días hábiles solo hacen falta para el nivel Full.
            Dim diasPorMes As Dictionary(Of Integer, Integer) = Nothing
            Dim remuneracionesPorMes As Dictionary(Of String, Decimal) = Nothing
            If nivel = NivelDetalleReporte.Full AndAlso filas.Count > 0 Then
                diasPorMes = CargarDiasHabiles()
                remuneracionesPorMes = CargarRemuneraciones(filas)
            End If

            Dim lista As New List(Of RegistroReporteBasicoDTO)(filas.Count)

            For Each f In filas
                Dim perfil = PerfilDe(f.USUARIO, perfiles)
                Dim dto = CType(Activator.CreateInstance(TipoDTO(nivel)), RegistroReporteBasicoDTO)

                dto.IDReg = f.IDReg
                dto.FechaActividad = f.FECHA_ACTIVIDAD
                dto.Centro = If(perfil IsNot Nothing, perfil.Centro, Nothing)
                dto.CodActividad = f.COD_ACTIVIDAD
                dto.Actividad = f.ACTIVIDAD
                dto.Pais = f.PAIS
                dto.IdProducto = f.ID_PRODUCTO
                dto.Producto = f.PRODUCTO
                dto.Horas = f.HORAS
                dto.Minutos = f.MINUTOS
                dto.Observaciones = f.OBSERVACIONES

                Dim avanzado = TryCast(dto, RegistroReporteAvanzadoDTO)
                If avanzado IsNot Nothing Then
                    avanzado.Usuario = f.USUARIO
                    If perfil IsNot Nothing Then
                        avanzado.Legajo = perfil.Legajo
                        avanzado.CUIL = perfil.CUIL
                        avanzado.Jornada = perfil.Modalidad
                    End If
                End If

                Dim full = TryCast(dto, RegistroReporteFullDTO)
                If full IsNot Nothing Then CompletarCostos(full, diasPorMes, remuneracionesPorMes)

                lista.Add(dto)
            Next

            Return lista
        End Function

        ' Costo = Remuneracion / DiasHabiles / Jornada x duración (mismo cálculo que app/reporte.aspx); vacío si falta algún dato.
        Private Shared Sub CompletarCostos(dto As RegistroReporteFullDTO,
                                           diasPorMes As Dictionary(Of Integer, Integer),
                                           remuneracionesPorMes As Dictionary(Of String, Decimal))
            Dim duracion = dto.Horas + (dto.Minutos / 60D)
            dto.DuracionTarea = Math.Round(duracion, 2)

            Dim fecha = dto.FechaActividad
            Dim dias As Integer
            If diasPorMes.TryGetValue(ClaveMes(fecha.Year, fecha.Month), dias) Then dto.DiasHabiles = dias

            Dim remuneracion As Decimal
            If dto.Usuario IsNot Nothing AndAlso
               remuneracionesPorMes.TryGetValue(ClaveRemuneracion(dto.Usuario, fecha.Year, fecha.Month), remuneracion) Then
                dto.Remuneracion = remuneracion
            End If

            If dto.Remuneracion.HasValue AndAlso dto.DiasHabiles.GetValueOrDefault() > 0 AndAlso dto.Jornada.GetValueOrDefault() > 0 Then
                dto.CostoTarea = Decimal.Round(dto.Remuneracion.Value / dto.DiasHabiles.Value / dto.Jornada.Value * duracion, 2)
            End If
        End Sub

        Private Function CargarDiasHabiles() As Dictionary(Of Integer, Integer)
            Dim resultado As New Dictionary(Of Integer, Integer)
            For Each d In diasHabiles.ObtenerTodos().ToList()
                resultado(ClaveMes(d.Anio, d.Mes)) = d.CantDias
            Next
            Return resultado
        End Function

        ' Se acota por el rango de años de las filas (la tabla es chica: usuarios x meses).
        Private Function CargarRemuneraciones(filas As List(Of spReporteGeneral_Result)) As Dictionary(Of String, Decimal)
            Dim desde = filas.Min(Function(f) f.FECHA_ACTIVIDAD).Year
            Dim hasta = filas.Max(Function(f) f.FECHA_ACTIVIDAD).Year

            Dim resultado As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
            For Each r In remuneraciones.ListarRemuneraciones().Where(Function(x) x.Anio >= desde AndAlso x.Anio <= hasta).ToList()
                resultado(ClaveRemuneracion(r.Usuario, r.Anio, r.Mes)) = r.Remuneracion
            Next
            Return resultado
        End Function

        Private Shared Function ClaveMes(anio As Integer, mes As Integer) As Integer
            Return anio * 100 + mes
        End Function

        Private Shared Function ClaveRemuneracion(usuario As String, anio As Integer, mes As Integer) As String
            Return usuario.Trim() & "|" & anio & "|" & mes
        End Function

        Private Shared Function NombreArchivo(titulo As String) As String
            Dim nombre = If(String.IsNullOrWhiteSpace(titulo), "Reporte", titulo.Trim())
            For Each c In System.IO.Path.GetInvalidFileNameChars()
                nombre = nombre.Replace(c, "_"c)
            Next
            Return nombre & ".xlsx"
        End Function
    End Class
End Namespace
