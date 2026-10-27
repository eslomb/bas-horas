Imports System.ComponentModel

Namespace DTOs.Reportes
    ' Nivel de detalle con que se devuelven los registros de un reporte, según quién lo pide.
    ' Cada nivel agrega campos al anterior (ver ReporteRegistrosService.DeterminarNivel).
    Public Enum NivelDetalleReporte
        Basico
        Avanzado
        Full
    End Enum

    ' <DisplayName> es el encabezado de la columna al exportar a Excel (Utils.ExportadorExcel); <Browsable(False)> la omite.
    Public Class RegistroReporteBasicoDTO
        <Browsable(False)> Public Property IDReg As Integer
        <DisplayName("Fecha")> Public Property FechaActividad As Date
        <DisplayName("Centro")> Public Property Centro As String
        <DisplayName("Cod")> Public Property CodActividad As String
        <DisplayName("Actividad")> Public Property Actividad As String
        <DisplayName("Pais")> Public Property Pais As String
        <Browsable(False)> <DisplayName("Id Producto")> Public Property IdProducto As Integer?
        <DisplayName("Producto")> Public Property Producto As String
        <DisplayName("Horas")> Public Property Horas As Integer
        <DisplayName("Minutos")> Public Property Minutos As Integer
        <DisplayName("Observaciones")> Public Property Observaciones As String
    End Class

    Public Class RegistroReporteAvanzadoDTO
        Inherits RegistroReporteBasicoDTO

        <DisplayName("Usuario")> Public Property Usuario As String
        <DisplayName("Legajo")> Public Property Legajo As String
        <DisplayName("CUIL")> Public Property CUIL As String
        <DisplayName("Jornada")> Public Property Jornada As Decimal?
    End Class

    Public Class RegistroReporteFullDTO
        Inherits RegistroReporteAvanzadoDTO

        <DisplayName("Dias.habil")> Public Property DiasHabiles As Integer?
        <DisplayName("Durac.tarea")> Public Property DuracionTarea As Decimal
        <DisplayName("Remuneracion")> Public Property Remuneracion As Decimal?
        <DisplayName("Costo Tarea")> Public Property CostoTarea As Decimal?
    End Class
End Namespace
