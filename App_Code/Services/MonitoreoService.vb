Imports System.Data.Entity
Imports System.Linq
Imports DTOs.Monitoreo
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class MonitoreoService
        Implements IMonitoreoService

        ' Mismo largo que MONITOREO.Url.
        Private Const LongitudMaximaUrl As Integer = 2000

        Private ReadOnly repo As IMonitoreoRepository

        Public Sub New(repo As IMonitoreoRepository)
            Me.repo = repo
        End Sub

        Public Function Registrar(usuario As String, input As MonitoreoInputDTO) As ResultadoMonitoreo Implements IMonitoreoService.Registrar
            Dim url = If(input.Url, "").Trim()
            If url.Length = 0 Then Return Invalido("La URL es obligatoria.")
            If url.Length > LongitudMaximaUrl Then Return Invalido("La URL puede tener hasta " & LongitudMaximaUrl & " caracteres.")

            Try
                repo.Agregar(New MONITOREO With {.Usuario = usuario, .Url = url})
            Catch
                Return New ResultadoMonitoreo With {.Estado = ResultadoOperacionMonitoreo.Fallo, .Mensaje = "No se pudo registrar la visita."}
            End Try

            Return New ResultadoMonitoreo With {.Estado = ResultadoOperacionMonitoreo.Ok}
        End Function

        ' Visitas por usuario, URL y día. Desde/hasta son fechas inclusive y opcionales.
        Public Function ObtenerVisitas(desde As Date?, hasta As Date?) As ResultadoMonitoreo Implements IMonitoreoService.ObtenerVisitas
            If desde.HasValue AndAlso hasta.HasValue AndAlso desde.Value.Date > hasta.Value.Date Then
                Return Invalido("La fecha 'desde' no puede ser posterior a 'hasta'.")
            End If

            Dim consulta = repo.ObtenerTodos()
            If desde.HasValue Then
                Dim d = desde.Value.Date
                consulta = consulta.Where(Function(m) m.Fecha >= d)
            End If
            If hasta.HasValue Then
                Dim limite = hasta.Value.Date.AddDays(1)
                consulta = consulta.Where(Function(m) m.Fecha < limite)
            End If

            ' TruncateTime (EF6) se resuelve en SQL; en EF Core sería m.Fecha.Date.
            Dim filas = consulta.
                GroupBy(Function(m) New With {Key m.Usuario, Key m.Url, Key .Dia = DbFunctions.TruncateTime(m.Fecha)}).
                Select(Function(g) New With {g.Key.Usuario, g.Key.Url, g.Key.Dia, .Cantidad = g.Count()}).
                OrderByDescending(Function(f) f.Dia).
                ThenBy(Function(f) f.Usuario).
                ThenBy(Function(f) f.Url).
                ToList()

            Dim items = filas.
                Select(Function(f) New VisitaAgrupadaDTO With {
                    .Usuario = f.Usuario, .Url = f.Url, .Fecha = f.Dia.Value, .Cantidad = f.Cantidad}).
                ToList()

            Return New ResultadoMonitoreo With {.Estado = ResultadoOperacionMonitoreo.Ok, .Items = items}
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoMonitoreo
            Return New ResultadoMonitoreo With {.Estado = ResultadoOperacionMonitoreo.Invalido, .Mensaje = mensaje}
        End Function
    End Class
End Namespace
