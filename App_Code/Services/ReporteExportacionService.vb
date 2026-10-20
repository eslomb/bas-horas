Imports System.Data
Imports System.IO
Imports System.Linq
Imports ClosedXML.Excel
Imports DTOs.Reportes
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ReporteExportacionService
        Implements IReporteExportacionService

        ' Usuarios (por prefijo, sin distinguir mayúsculas) que además de los Administradores pueden ver Remuneración y Costo. 
        ' Mismo criterio que el hardcode de app/ (habilitarRemuneraciones).
        
        Private Shared ReadOnly UsuariosConRemuneraciones As String() = {"ROCCHI", "CARRACEDO"}

        Private ReadOnly reporteGeneral As IReporteGeneralService
        Private ReadOnly remuneraciones As IRemuneracionRepository

        Public Sub New(reporteGeneral As IReporteGeneralService, remuneraciones As IRemuneracionRepository)
            Me.reporteGeneral = reporteGeneral
            Me.remuneraciones = remuneraciones
        End Sub

        Public Function GenerarExcel(filtros As FiltroReporte, titulo As String, usuarioActual As String, esAdministrador As Boolean) As Byte() Implements IReporteExportacionService.GenerarExcel
            Dim filas = reporteGeneral.GenerarReporteGeneral(filtros)
            Dim incluirCostos = PuedeVerRemuneraciones(usuarioActual, esAdministrador)

            Dim diasHabiles As Dictionary(Of Integer, Integer) = Nothing
            Dim remuneracionesPorMes As Dictionary(Of String, Decimal) = Nothing

            If incluirCostos AndAlso filas.Count > 0 Then
                diasHabiles = CargarDiasHabiles()
                remuneracionesPorMes = CargarRemuneraciones(filas)
            End If

            Dim tabla = ArmarTabla(filas, incluirCostos, diasHabiles, remuneracionesPorMes)

            Using wb As New XLWorkbook()
                Dim ws = wb.Worksheets.Add(tabla)
                ws.Columns().AdjustToContents()

                Using ms As New MemoryStream()
                    wb.SaveAs(ms)
                    Return ms.ToArray()
                End Using
            End Using
        End Function

        Private Shared Function PuedeVerRemuneraciones(usuario As String, esAdministrador As Boolean) As Boolean
            If esAdministrador Then Return True
            If String.IsNullOrEmpty(usuario) Then Return False

            Return UsuariosConRemuneraciones.Any(Function(p) usuario.StartsWith(p, StringComparison.OrdinalIgnoreCase))
        End Function

        ' Clave = Anio * 100 + Mes
        Private Function CargarDiasHabiles() As Dictionary(Of Integer, Integer)
            Dim resultado As New Dictionary(Of Integer, Integer)
            For Each d In remuneraciones.ListarDiasHabiles().ToList()
                resultado(ClaveMes(d.Anio, d.Mes)) = d.CantDias
            Next
            Return resultado
        End Function

        ' Clave = USUARIO|Anio|Mes. Se acota por rango de años de las filas (tabla chica: usuarios x meses).
        Private Function CargarRemuneraciones(filas As List(Of ReporteFilaDTO)) As Dictionary(Of String, Decimal)
            Dim desde = filas.Min(Function(f) f.FechaActividad).Year
            Dim hasta = filas.Max(Function(f) f.FechaActividad).Year

            Dim resultado As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
            For Each r In remuneraciones.ListarRemuneraciones().
                Where(Function(x) x.Anio >= desde AndAlso x.Anio <= hasta).
                ToList()

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

        Private Shared Function ArmarTabla(filas As List(Of ReporteFilaDTO),
                                           incluirCostos As Boolean,
                                           diasHabiles As Dictionary(Of Integer, Integer),
                                           remuneracionesPorMes As Dictionary(Of String, Decimal)) As DataTable
            Dim tabla As New DataTable("Reporte")

            tabla.Columns.Add("Fecha", GetType(Date))
            tabla.Columns.Add("Usuario", GetType(String))
            tabla.Columns.Add("Legajo", GetType(String))
            tabla.Columns.Add("Jornada", GetType(Decimal))
            If incluirCostos Then tabla.Columns.Add("Dias.habil", GetType(Integer))
            tabla.Columns.Add("Centro", GetType(String))
            tabla.Columns.Add("Cod", GetType(String))
            tabla.Columns.Add("Actividad", GetType(String))
            tabla.Columns.Add("Pais", GetType(String))
            tabla.Columns.Add("Producto", GetType(String))
            tabla.Columns.Add("Horas", GetType(Integer))
            tabla.Columns.Add("Minutos", GetType(Integer))
            If incluirCostos Then
                tabla.Columns.Add("Durac.tarea", GetType(Decimal))
                tabla.Columns.Add("Remuneracion", GetType(Decimal))
                tabla.Columns.Add("Costo Tarea", GetType(Decimal))
            End If
            tabla.Columns.Add("Observaciones", GetType(String))
            tabla.Columns.Add("CUIL", GetType(String))

            For Each f In filas
                Dim fila = tabla.NewRow()

                fila("Fecha") = f.FechaActividad.Date
                fila("Usuario") = ValorODbNull(f.Usuario)
                fila("Legajo") = ValorODbNull(f.Legajo)
                fila("Jornada") = ValorODbNull(f.Jornada)
                fila("Centro") = ValorODbNull(f.Centro)
                fila("Cod") = ValorODbNull(f.CodActividad)
                fila("Actividad") = ValorODbNull(f.Actividad)
                fila("Pais") = ValorODbNull(f.Pais)
                fila("Producto") = ValorODbNull(f.Producto)
                fila("Horas") = f.Horas
                fila("Minutos") = f.Minutos
                fila("Observaciones") = ValorODbNull(f.Observaciones)
                fila("CUIL") = ValorODbNull(f.CUIL)

                If incluirCostos Then
                    Dim duracion = f.Horas + (f.Minutos / 60D)
                    Dim dias As Integer
                    Dim remuneracion As Decimal
                    Dim tieneDias = diasHabiles.TryGetValue(ClaveMes(f.FechaActividad.Year, f.FechaActividad.Month), dias)
                    Dim tieneRemuneracion = Not String.IsNullOrWhiteSpace(f.Usuario) AndAlso
                        remuneracionesPorMes.TryGetValue(ClaveRemuneracion(f.Usuario, f.FechaActividad.Year, f.FechaActividad.Month), remuneracion)

                    fila("Dias.habil") = If(tieneDias, CObj(dias), DBNull.Value)
                    fila("Durac.tarea") = Math.Round(duracion, 2)
                    fila("Remuneracion") = If(tieneRemuneracion, CObj(remuneracion), DBNull.Value)
                    fila("Costo Tarea") = CalcularCosto(tieneRemuneracion, remuneracion, tieneDias, dias, f.Jornada, duracion)
                End If

                tabla.Rows.Add(fila)
            Next

            Return tabla
        End Function

        ' ValorHora = Remuneracion / DiasHabiles / Jornada ; Costo = ValorHora x duración (mismo cálculo que app/reporte.aspx).
        Private Shared Function CalcularCosto(tieneRemuneracion As Boolean, remuneracion As Decimal,
                                              tieneDias As Boolean, dias As Integer,
                                              jornada As Decimal?, duracion As Decimal) As Object
            If Not tieneRemuneracion OrElse Not tieneDias OrElse dias <= 0 Then Return DBNull.Value
            If Not jornada.HasValue OrElse jornada.Value <= 0 Then Return DBNull.Value

            Return Decimal.Round(remuneracion / dias / jornada.Value * duracion, 2)
        End Function

        Private Shared Function ValorODbNull(valor As Object) As Object
            Return If(valor, DBNull.Value)
        End Function
    End Class
End Namespace
