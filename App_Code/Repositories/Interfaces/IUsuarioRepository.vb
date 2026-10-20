Namespace Repositories.Interfaces
    Public Interface IUsuarioRepository
        Function ObtenerTodos() As List(Of MembershipUser)
        Function ObtenerPorUserName(userName As String) As MembershipUser
        Function ObtenerPorEmail(email As String) As MembershipUser
        Function ObtenerRoles(userName As String) As String()
        Function ObtenerPerfil(userName As String) As ProfileCommon
        Sub ActualizarUsuario(usuario As MembershipUser)
        Sub ActualizarPerfil(perfil As ProfileCommon)
        Function CambiarPassword(userName As String, passwordActual As String, passwordNueva As String) As Boolean
        Function ForzarNuevaPassword(userName As String, nuevaPassword As String) As Boolean
    End Interface
End Namespace
