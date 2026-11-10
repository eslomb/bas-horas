Imports DTOs.Reportes

Namespace Services.Interfaces
    ' Qué usuarios puede abarcar un reporte según el rol de quien lo usa. Única fuente de la regla:
    ' la usan el listado de usuarios seleccionables, el guardado (POST/PUT) y la ejecución/exportación.
    Public Interface IReporteAlcanceService
        Function ObtenerAlcance(usuario As String) As AlcanceReporte
        Function ListarUsuariosSeleccionables(usuario As String) As List(Of UsuarioSeleccionableDTO)
    End Interface
End Namespace
