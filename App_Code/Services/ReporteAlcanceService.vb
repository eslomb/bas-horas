Imports System.Linq
Imports DTOs.Reportes
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    ' Alcance de un reporte para un usuario:
    '   Administrador -> sin restricción (UsuariosPermitidos = Nothing); DeUsuario y DeCentro (lista separada por coma) se respetan.
    '   Avanzado      -> él mismo + los usuarios Basico de su mismo Centro (Profile). DeCentro se ignora: siempre aplica su Centro.
    '   Basico        -> solo él mismo. DeUsuario y DeCentro se ignoran.
    Public Class AlcanceReporte
        Public Const MensajeFueraDeAlcance As String = "Solo puede consultar sus propios registros y los de usuarios Basicos de su Centro."

        Public Property EsAdministrador As Boolean
        Public Property EsAvanzado As Boolean
        Public Property UsuariosPermitidos As HashSet(Of String)

        Public Function Permite(userName As String) As Boolean
            If UsuariosPermitidos Is Nothing Then Return True
            Return userName IsNot Nothing AndAlso UsuariosPermitidos.Contains(userName.Trim())
        End Function
    End Class

    Public Class ReporteAlcanceService
        Implements IReporteAlcanceService

        Private ReadOnly usuarios As IUsuarioRepository

        Public Sub New(usuarios As IUsuarioRepository)
            Me.usuarios = usuarios
        End Sub

        Public Function ObtenerAlcance(usuario As String) As AlcanceReporte Implements IReporteAlcanceService.ObtenerAlcance
            Dim roles = usuarios.ObtenerRoles(usuario)

            If roles.Contains(Aplicacion.Grupos.Administrador, StringComparer.OrdinalIgnoreCase) Then
                Return New AlcanceReporte With {.EsAdministrador = True}
            End If

            Dim esAvanzado = roles.Contains(Aplicacion.Grupos.Avanzado, StringComparer.OrdinalIgnoreCase)
            Dim permitidos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {usuario}

            If esAvanzado Then
                ' Un Profile por cada Basico: no hay forma de leer Profile en bloque. Sin Centro propio, solo se ve a sí mismo.
                Dim centro = CentroDe(usuario)
                If centro <> "" Then
                    For Each basico In usuarios.ObtenerUsuariosEnRol(Aplicacion.Grupos.Basico)
                        If centro.Equals(CentroDe(basico), StringComparison.OrdinalIgnoreCase) Then permitidos.Add(basico)
                    Next
                End If
            End If

            Return New AlcanceReporte With {.EsAvanzado = esAvanzado, .UsuariosPermitidos = permitidos}
        End Function

        ' Administrador: todos los usuarios habilitados. Avanzado: su alcance. Basico: lista vacía.
        Public Function ListarUsuariosSeleccionables(usuario As String) As List(Of UsuarioSeleccionableDTO) Implements IReporteAlcanceService.ListarUsuariosSeleccionables
            Dim alcance = ObtenerAlcance(usuario)
            Dim nombres As IEnumerable(Of String)

            If alcance.EsAdministrador Then
                nombres = usuarios.ObtenerTodos().Where(Function(u) u.IsApproved).Select(Function(u) u.UserName)
            ElseIf alcance.EsAvanzado Then
                nombres = alcance.UsuariosPermitidos
            Else
                Return New List(Of UsuarioSeleccionableDTO)
            End If

            Return nombres.
                OrderBy(Function(n) n, StringComparer.OrdinalIgnoreCase).
                Select(Function(n) New UsuarioSeleccionableDTO With {.UserName = n}).
                ToList()
        End Function

        Private Function CentroDe(userName As String) As String
            Dim perfil = usuarios.ObtenerPerfil(userName)
            If perfil Is Nothing OrElse perfil.Centro Is Nothing Then Return ""
            Return perfil.Centro.Trim()
        End Function
    End Class
End Namespace
