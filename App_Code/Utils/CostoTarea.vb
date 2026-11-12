Namespace Utils
    ' Costo de un registro de horas: Remuneracion / DiasHabiles / Jornada x duración (en horas), redondeado a 2 decimales.
    ' Mismo cálculo que app/reporte.aspx. Nothing si falta algún dato (sin remuneración del mes, sin días hábiles o sin jornada).
    ' Lo usan la columna "Costo Tarea" de los reportes (ReporteRegistrosService) y los gráficos de costos (CostoService).
    Public NotInheritable Class CostoTarea
        Private Sub New()
        End Sub

        Public Shared Function Duracion(horas As Integer, minutos As Integer) As Decimal
            Return horas + (minutos / 60D)
        End Function

        Public Shared Function Calcular(remuneracion As Decimal?, diasHabiles As Integer?, jornada As Decimal?, duracion As Decimal) As Decimal?
            If Not remuneracion.HasValue OrElse diasHabiles.GetValueOrDefault() <= 0 OrElse jornada.GetValueOrDefault() <= 0 Then Return Nothing
            Return Decimal.Round(remuneracion.Value / diasHabiles.Value / jornada.Value * duracion, 2)
        End Function
    End Class
End Namespace
