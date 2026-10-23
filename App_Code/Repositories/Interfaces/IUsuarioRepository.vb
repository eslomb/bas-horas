Namespace Repositories.Interfaces
    Public Interface IUsuarioRepository
        Function ObtenerTodos() As List(Of MembershipUser)
        Function ObtenerPorUserName(userName As String) As MembershipUser
        Function ObtenerPorEmail(email As String) As MembershipUser
        Function ObtenerRoles(userName As String) As String()
        Function CrearUsuario(userName As String, password As String, email As String, habilitado As Boolean, ByRef estado As System.Web.Security.MembershipCreateStatus) As MembershipUser
        Function EliminarUsuario(userName As String) As Boolean
        Function Desbloquear(userName As String) As Boolean
        Function CumplePoliticaPassword(password As String) As Boolean
        Function ExisteRol(rol As String) As Boolean
        Function ObtenerUsuariosEnRol(rol As String) As String()
        Sub AsignarRolUnico(userName As String, rol As String)
        Function ObtenerPerfil(userName As String) As ProfileCommon
        Sub ActualizarUsuario(usuario As MembershipUser)
        Sub ActualizarPerfil(perfil As ProfileCommon)
        Function CambiarPassword(userName As String, passwordActual As String, passwordNueva As String) As Boolean
        Function ForzarNuevaPassword(userName As String, nuevaPassword As String) As Boolean
    End Interface
End Namespace
