Imports System.Linq

Namespace Utils
    ' Único lugar donde se define quién tiene acceso a la información de remuneraciones: los usuarios cuyo nombre
    ' empieza (sin distinguir mayúsculas) con alguno de PrefijosNivelFull. Mismo criterio que habilitarRemuneraciones
    ' de app/, reimplementado para no depender de Aplicacion.vb.
    '  - Reportes: esos usuarios ven el nivel Full (Dias.habil, Remuneracion, Costo Tarea).
    '  - Remuneraciones: gestionarlas además exige el rol Administrador (ver PuedeGestionarRemuneraciones).
    Public NotInheritable Class AccesoRemuneraciones
        Private Sub New()
        End Sub

        Public Shared ReadOnly PrefijosNivelFull As String() = {"ROCCHI", "CARRACEDO"}

        Public Shared Function EsNivelFull(usuario As String) As Boolean
            If String.IsNullOrWhiteSpace(usuario) Then Return False
            Return PrefijosNivelFull.Any(Function(p) usuario.StartsWith(p, StringComparison.OrdinalIgnoreCase))
        End Function

        Public Shared Function PuedeGestionarRemuneraciones(usuario As String, esAdministrador As Boolean) As Boolean
            Return esAdministrador AndAlso EsNivelFull(usuario)
        End Function
    End Class
End Namespace
