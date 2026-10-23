Imports System.Linq
Imports System.Web.Security
Imports DTOs.Usuarios
Imports Repositories.Interfaces
Imports Services.Interfaces
Imports Utils

Namespace Services
    Public Class UsuarioService
        Implements IUsuarioService

        Private Const LongitudPasswordGenerada As Integer = 8

        Private ReadOnly repo As IUsuarioRepository
        Private ReadOnly notificador As INotificadorUsuarios

        Public Sub New(repo As IUsuarioRepository, notificador As INotificadorUsuarios)
            Me.repo = repo
            Me.notificador = notificador
        End Sub

        Public Function ObtenerPorUserName(userName As String) As UsuarioDTO Implements IUsuarioService.ObtenerPorUserName
            Dim usuario = repo.ObtenerPorUserName(userName)
            Return If(usuario Is Nothing, Nothing, MapearDTO(usuario))
        End Function

        Public Function Listar() As List(Of UsuarioDTO) Implements IUsuarioService.Listar
            Return repo.ObtenerTodos().Select(Function(u) MapearDTO(u)).ToList()
        End Function

        Public Function ActualizarPerfilPropio(userName As String, input As ActualizarPerfilPropioInputDTO) As ResultadoUsuario Implements IUsuarioService.ActualizarPerfilPropio
            Dim usuario = repo.ObtenerPorUserName(userName)
            If usuario Is Nothing Then
                Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.NoEncontrado}
            End If

            usuario.Email = input.Email
            usuario.Comment = input.Comentario

            Try
                repo.ActualizarUsuario(usuario)
            Catch
                Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Fallo, .Mensaje = "No se pudieron guardar los datos."}
            End Try

            Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Ok, .Usuario = MapearDTO(usuario)}
        End Function

        Public Function ActualizarUsuario(userName As String, input As ActualizarUsuarioInputDTO, usuarioActual As String) As ResultadoUsuario Implements IUsuarioService.ActualizarUsuario
            Dim usuario = repo.ObtenerPorUserName(userName)
            If usuario Is Nothing Then
                Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.NoEncontrado}
            End If

            ' --- Validaciones: ninguna escribe nada, todas antes del primer cambio ---

            Dim rol As String = Nothing
            Dim errorValidacion = ValidarRolYHabilitado(input, rol)
            If errorValidacion IsNot Nothing Then Return errorValidacion

            ' La política se chequea acá y no recién al cambiar la clave: ForzarNuevaPassword hace ResetPassword
            ' antes de ChangePassword, así que una clave inválida dejaría la cuenta con una temporal desconocida.
            Dim cambiaPassword = Not String.IsNullOrEmpty(input.Password)
            If cambiaPassword AndAlso Not repo.CumplePoliticaPassword(input.Password) Then
                Return Invalido(MensajePasswordInvalida)
            End If

            Dim habilitado = input.Habilitado.Value
            Dim eraAdministradorActivo = EsAdministradorActivo(usuario)
            Dim pierdeAdministrador = eraAdministradorActivo AndAlso
                                      (Not rol.Equals(Aplicacion.Grupos.Administrador, StringComparison.OrdinalIgnoreCase) OrElse Not habilitado)

            If pierdeAdministrador Then
                If userName.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase) Then
                    Return Conflicto("Un administrador no puede quitarse su propio rol ni deshabilitarse a sí mismo.")
                End If
                If Not HayOtroAdministradorActivo(usuario.UserName) Then
                    Return Conflicto("No se puede dejar al sistema sin ningún administrador habilitado.")
                End If
            End If

            ' --- Escrituras, de la que más probablemente falla a la menos ---
            ' Membership, Profile y Roles no comparten transacción. UpdateUser va primero porque falla con
            ' facilidad (email duplicado, requiresUniqueEmail=true) y no deja nada a medias; el rol va al
            ' final, ya validado que existe, y AsignarRolUnico nunca deja al usuario sin rol. La clave (si viene)
            ' se cambia justo después de UpdateUser, antes del Profile y del rol.

            usuario.Email = input.Email
            usuario.Comment = input.Comentario
            usuario.IsApproved = habilitado

            Try
                repo.ActualizarUsuario(usuario)
            Catch
                Return Fallo("No se pudieron guardar los datos. Verificá que el email no esté en uso por otro usuario.")
            End Try

            If cambiaPassword Then
                Try
                    ' ResetPassword falla en cuentas bloqueadas. Al fijarle una clave nueva a un usuario bloqueado
                    ' se lo desbloquea; no hay otro endpoint para hacerlo.
                    If usuario.IsLockedOut Then repo.Desbloquear(userName)

                    If Not repo.ForzarNuevaPassword(userName, input.Password) Then
                        Return Fallo(MensajePasswordFallida)
                    End If
                Catch
                    Return Fallo(MensajePasswordFallida)
                End Try
            End If

            Dim perfil = repo.ObtenerPerfil(userName)
            AplicarPerfil(perfil, input)

            Try
                repo.ActualizarPerfil(perfil)
            Catch
                Return Fallo("Se guardaron el email, el estado y la contraseña (si se envió), pero no se pudo guardar el perfil. El rol no se modificó; reintentá la operación.")
            End Try

            Try
                repo.AsignarRolUnico(userName, rol)
            Catch
                Return Fallo("Se guardaron los datos del usuario, pero no se pudo cambiar el rol. Reintentá la operación.")
            End Try

            Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Ok, .Usuario = MapearDTO(usuario)}
        End Function

        Public Function CrearUsuario(input As CrearUsuarioInputDTO) As ResultadoUsuario Implements IUsuarioService.CrearUsuario
            ' --- Validaciones: ninguna escribe nada ---

            Dim userName = If(input.UserName, "")
            If String.IsNullOrWhiteSpace(userName) OrElse userName <> userName.Trim() Then
                Return Invalido("El nombre de usuario es obligatorio y no puede tener espacios al inicio ni al final.")
            End If
            If String.IsNullOrEmpty(input.Password) Then
                Return Invalido("La contraseña es obligatoria.")
            End If

            Dim rol As String = Nothing
            Dim errorValidacion = ValidarRolYHabilitado(input, rol)
            If errorValidacion IsNot Nothing Then Return errorValidacion

            If repo.ObtenerPorUserName(userName) IsNot Nothing Then
                Return Conflicto("Ya existe un usuario con ese nombre.")
            End If

            Dim habilitado = input.Habilitado.Value

            ' --- Escrituras: CreateUser primero (es la que rechaza clave, usuario o email inválidos o repetidos
            ' sin dejar nada creado). Si algo falla después, se intenta borrar el usuario recién creado. ---

            Dim estado As MembershipCreateStatus
            Dim usuario As MembershipUser

            Try
                usuario = repo.CrearUsuario(userName, input.Password, input.Email, habilitado, estado)
            Catch
                Return Fallo("No se pudo crear el usuario.")
            End Try

            If usuario Is Nothing Then Return ResultadoPorEstadoCreacion(estado)

            Dim paso = "guardar los datos del usuario"
            Try
                usuario.Comment = input.Comentario
                usuario.IsApproved = habilitado
                repo.ActualizarUsuario(usuario)

                paso = "guardar el perfil"
                Dim perfil = repo.ObtenerPerfil(userName)
                AplicarPerfil(perfil, input)
                repo.ActualizarPerfil(perfil)

                paso = "asignar el rol"
                repo.AsignarRolUnico(userName, rol)
            Catch
                Return RevertirAlta(userName, paso)
            End Try

            Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Ok, .Usuario = MapearDTO(usuario)}
        End Function

        Public Function CambiarPassword(userName As String, input As CambiarPasswordInputDTO) As ResultadoCambioPassword Implements IUsuarioService.CambiarPassword
            Try
                Dim exito = repo.CambiarPassword(userName, input.PasswordActual, input.PasswordNueva)
                Return If(exito, ResultadoCambioPassword.Ok, ResultadoCambioPassword.PasswordActualIncorrecta)
            Catch ex As MembershipPasswordException
                Return ResultadoCambioPassword.PasswordNoValida
            Catch
                Return ResultadoCambioPassword.Fallo
            End Try
        End Function

        Public Function RecuperarPassword(email As String) As ResultadoRecuperarPassword Implements IUsuarioService.RecuperarPassword
            Dim usuario = repo.ObtenerPorEmail(email)

            If usuario Is Nothing Then
                Return ResultadoRecuperarPassword.EmailNoEncontrado
            End If

            If Not usuario.IsApproved Then
                Return ResultadoRecuperarPassword.UsuarioInactivo
            End If

            If usuario.IsLockedOut Then
                Return ResultadoRecuperarPassword.CuentaBloqueada
            End If

            Dim nuevaPassword = GeneradorPassword.Generar(LongitudPasswordGenerada)

            Try
                If Not repo.ForzarNuevaPassword(usuario.UserName, nuevaPassword) Then
                    Return ResultadoRecuperarPassword.Fallo
                End If

                notificador.EnviarNuevaPassword(usuario.Email, nuevaPassword)
            Catch
                Return ResultadoRecuperarPassword.Fallo
            End Try

            Return ResultadoRecuperarPassword.Ok
        End Function

        Private Shared ReadOnly RolesPermitidos As String() = {
            Aplicacion.Grupos.Basico,
            Aplicacion.Grupos.Avanzado,
            Aplicacion.Grupos.Administrador
        }

        ' Devuelve el nombre canónico del rol (ignora mayúsculas) o Nothing si no es uno de los permitidos.
        Private Shared Function RolValido(rol As String) As String
            If String.IsNullOrWhiteSpace(rol) Then Return Nothing
            Return RolesPermitidos.FirstOrDefault(Function(r) r.Equals(rol.Trim(), StringComparison.OrdinalIgnoreCase))
        End Function

        Private Function EsAdministradorActivo(usuario As MembershipUser) As Boolean
            Return usuario.IsApproved AndAlso
                   repo.ObtenerRoles(usuario.UserName).Contains(Aplicacion.Grupos.Administrador, StringComparer.OrdinalIgnoreCase)
        End Function

        ' Administradores habilitados (aprobados y no bloqueados) distintos del usuario indicado.
        Private Function HayOtroAdministradorActivo(userName As String) As Boolean
            Return repo.ObtenerUsuariosEnRol(Aplicacion.Grupos.Administrador).
                Where(Function(n) Not n.Equals(userName, StringComparison.OrdinalIgnoreCase)).
                Select(Function(n) repo.ObtenerPorUserName(n)).
                Any(Function(u) u IsNot Nothing AndAlso u.IsApproved AndAlso Not u.IsLockedOut)
        End Function

        Private Const MensajePasswordInvalida As String = "La contraseña no cumple con la política requerida."
        Private Const MensajePasswordFallida As String = "Se guardaron el email y el estado, pero no se pudo cambiar la contraseña (puede haber quedado reemplazada por una temporal desconocida). Volvé a asignarla."

        ' Valida Rol y Habilitado (obligatorios en alta y edición) y que el rol exista en el sistema.
        ' Devuelve el resultado de error o Nothing; el nombre canónico del rol sale por rol.
        Private Function ValidarRolYHabilitado(input As ActualizarUsuarioInputDTO, ByRef rol As String) As ResultadoUsuario
            rol = RolValido(input.Rol)
            If rol Is Nothing Then
                Return Invalido("El rol debe ser uno de: " & String.Join(", ", RolesPermitidos) & ".")
            End If
            If Not input.Habilitado.HasValue Then
                Return Invalido("Debe indicar si el usuario está habilitado.")
            End If
            If Not repo.ExisteRol(rol) Then
                Return Fallo("El rol '" & rol & "' no está configurado en el sistema.")
            End If

            Return Nothing
        End Function

        Private Shared Sub AplicarPerfil(perfil As ProfileCommon, input As ActualizarUsuarioInputDTO)
            perfil.Legajo = input.Legajo
            perfil.Modalidad = input.Modalidad
            perfil.Centro = input.Centro
            perfil.CUIL = input.CUIL
            perfil.Dependencia = input.Dependencia
            perfil.PeriodoAbierto = input.PeriodoAbierto
        End Sub

        Private Shared Function ResultadoPorEstadoCreacion(estado As MembershipCreateStatus) As ResultadoUsuario
            Select Case estado
                Case MembershipCreateStatus.InvalidPassword
                    Return Invalido(MensajePasswordInvalida)
                Case MembershipCreateStatus.InvalidUserName
                    Return Invalido("El nombre de usuario no es válido.")
                Case MembershipCreateStatus.InvalidEmail
                    Return Invalido("El email no es válido.")
                Case MembershipCreateStatus.DuplicateUserName
                    Return Conflicto("Ya existe un usuario con ese nombre.")
                Case MembershipCreateStatus.DuplicateEmail
                    Return Conflicto("Ya existe un usuario con ese email.")
                Case Else
                    Return Fallo("No se pudo crear el usuario.")
            End Select
        End Function

        ' El alta falló después de CreateUser: se intenta borrar el usuario para no dejarlo a medias.
        Private Function RevertirAlta(userName As String, paso As String) As ResultadoUsuario
            Dim borrado As Boolean

            Try
                borrado = repo.EliminarUsuario(userName)
            Catch
                borrado = False
            End Try

            If borrado Then
                Return Fallo("No se pudo crear el usuario: falló al " & paso & ". No se creó nada.")
            End If

            Return Fallo("El usuario '" & userName & "' quedó creado a medias: falló al " & paso &
                         " y tampoco se pudo borrar. Revisalo o eliminalo manualmente.")
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoUsuario
            Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function Conflicto(mensaje As String) As ResultadoUsuario
            Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Conflicto, .Mensaje = mensaje}
        End Function

        Private Shared Function Fallo(mensaje As String) As ResultadoUsuario
            Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Fallo, .Mensaje = mensaje}
        End Function

        Private Function MapearDTO(usuario As MembershipUser) As UsuarioDTO
            Dim perfil = repo.ObtenerPerfil(usuario.UserName)
            Dim roles = repo.ObtenerRoles(usuario.UserName)

            Return New UsuarioDTO With {
                .UserName = usuario.UserName,
                .Email = usuario.Email,
                .IsApproved = usuario.IsApproved,
                .Comentario = usuario.Comment,
                .Legajo = perfil.Legajo,
                .Modalidad = perfil.Modalidad,
                .PeriodoAbierto = perfil.PeriodoAbierto,
                .Centro = perfil.Centro,
                .CUIL = perfil.CUIL,
                .Dependencia = perfil.Dependencia,
                .Roles = roles.ToList()
            }
        End Function
    End Class
End Namespace
