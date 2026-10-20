Imports System.Linq
Imports System.Web.Security
Imports System.Web.Profile
Imports Repositories.Interfaces

Namespace Repositories.Membership
    Public Class UsuarioRepository
        Implements IUsuarioRepository

        Public Function ObtenerTodos() As List(Of MembershipUser) Implements IUsuarioRepository.ObtenerTodos
            Return System.Web.Security.Membership.GetAllUsers().Cast(Of MembershipUser)().ToList()
        End Function

        Public Function ObtenerPorUserName(userName As String) As MembershipUser Implements IUsuarioRepository.ObtenerPorUserName
            Return System.Web.Security.Membership.GetUser(userName)
        End Function

        Public Function ObtenerPorEmail(email As String) As MembershipUser Implements IUsuarioRepository.ObtenerPorEmail
            Dim userName = System.Web.Security.Membership.GetUserNameByEmail(email)
            If String.IsNullOrEmpty(userName) Then Return Nothing

            Return ObtenerPorUserName(userName)
        End Function

        Public Function ObtenerRoles(userName As String) As String() Implements IUsuarioRepository.ObtenerRoles
            Return Roles.GetRolesForUser(userName)
        End Function

        Public Function ObtenerPerfil(userName As String) As ProfileCommon Implements IUsuarioRepository.ObtenerPerfil
            Return CType(ProfileBase.Create(userName, True), ProfileCommon)
        End Function

        Public Sub ActualizarUsuario(usuario As MembershipUser) Implements IUsuarioRepository.ActualizarUsuario
            System.Web.Security.Membership.UpdateUser(usuario)
        End Sub

        Public Sub ActualizarPerfil(perfil As ProfileCommon) Implements IUsuarioRepository.ActualizarPerfil
            perfil.Save()
        End Sub

        Public Function CambiarPassword(userName As String, passwordActual As String, passwordNueva As String) As Boolean Implements IUsuarioRepository.CambiarPassword
            Dim usuario = ObtenerPorUserName(userName)
            If usuario Is Nothing Then Return False

            Return usuario.ChangePassword(passwordActual, passwordNueva)
        End Function

        Public Function ForzarNuevaPassword(userName As String, nuevaPassword As String) As Boolean Implements IUsuarioRepository.ForzarNuevaPassword
            Dim usuario = ObtenerPorUserName(userName)
            If usuario Is Nothing Then Return False

            ' Membership no permite setear una password arbitraria sin conocer la actual.
            ' ResetPassword() genera una temporal (requiere EnablePasswordReset, true por default);
            ' con esa temporal recién conocida, ChangePassword() la reemplaza por la nuestra.
            Dim passwordTemporal = usuario.ResetPassword()
            Return usuario.ChangePassword(passwordTemporal, nuevaPassword)
        End Function
    End Class
End Namespace
