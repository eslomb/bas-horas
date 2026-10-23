Imports System.Linq
Imports System.Web.Security
Imports System.Text.RegularExpressions
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

        ' Devuelve Nothing si Membership rechaza el alta; el motivo queda en estado.
        Public Function CrearUsuario(userName As String, password As String, email As String, habilitado As Boolean, ByRef estado As MembershipCreateStatus) As MembershipUser Implements IUsuarioRepository.CrearUsuario
            Return System.Web.Security.Membership.CreateUser(userName, password, email, Nothing, Nothing, habilitado, estado)
        End Function

        ' Borra también el Profile y las membresías de rol del usuario.
        Public Function EliminarUsuario(userName As String) As Boolean Implements IUsuarioRepository.EliminarUsuario
            Return System.Web.Security.Membership.DeleteUser(userName, True)
        End Function

        Public Function Desbloquear(userName As String) As Boolean Implements IUsuarioRepository.Desbloquear
            Dim usuario = ObtenerPorUserName(userName)
            If usuario Is Nothing Then Return False

            Return usuario.UnlockUser()
        End Function

        ' Misma política que aplica el provider al cambiar la clave. Sirve para rechazar una clave inválida
        ' antes de escribir nada: ForzarNuevaPassword hace ResetPassword primero, y si el ChangePassword
        ' posterior fallara por política la cuenta quedaría con una clave temporal desconocida.
        Public Function CumplePoliticaPassword(password As String) As Boolean Implements IUsuarioRepository.CumplePoliticaPassword
            If String.IsNullOrEmpty(password) Then Return False
            If password.Length < System.Web.Security.Membership.MinRequiredPasswordLength Then Return False

            Dim noAlfanumericos = password.Count(Function(c) Not Char.IsLetterOrDigit(c))
            If noAlfanumericos < System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters Then Return False

            Dim patron = System.Web.Security.Membership.PasswordStrengthRegularExpression
            If Not String.IsNullOrEmpty(patron) AndAlso Not Regex.IsMatch(password, patron) Then Return False

            Return True
        End Function

        Public Function ExisteRol(rol As String) As Boolean Implements IUsuarioRepository.ExisteRol
            Return Roles.RoleExists(rol)
        End Function

        Public Function ObtenerUsuariosEnRol(rol As String) As String() Implements IUsuarioRepository.ObtenerUsuariosEnRol
            Return Roles.GetUsersInRole(rol)
        End Function

        ' Deja al usuario únicamente en el rol indicado. Primero lo agrega al nuevo y recién después lo saca
        ' de los otros: si algo falla a la mitad, el usuario conserva al menos un rol en vez de quedar sin ninguno.
        Public Sub AsignarRolUnico(userName As String, rol As String) Implements IUsuarioRepository.AsignarRolUnico
            Dim actuales = Roles.GetRolesForUser(userName)

            If Not actuales.Contains(rol, StringComparer.OrdinalIgnoreCase) Then
                Roles.AddUserToRole(userName, rol)
            End If

            For Each otro In actuales.Where(Function(r) Not r.Equals(rol, StringComparison.OrdinalIgnoreCase))
                Roles.RemoveUserFromRole(userName, otro)
            Next
        End Sub

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
