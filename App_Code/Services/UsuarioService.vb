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

        Public Function ActualizarUsuario(userName As String, input As ActualizarUsuarioInputDTO) As ResultadoUsuario Implements IUsuarioService.ActualizarUsuario
            Dim usuario = repo.ObtenerPorUserName(userName)
            If usuario Is Nothing Then
                Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.NoEncontrado}
            End If

            usuario.Email = input.Email
            usuario.Comment = input.Comentario

            Dim perfil = repo.ObtenerPerfil(userName)
            perfil.Legajo = input.Legajo
            perfil.Modalidad = input.Modalidad
            perfil.Centro = input.Centro
            perfil.CUIL = input.CUIL
            perfil.Dependencia = input.Dependencia
            perfil.PeriodoAbierto = input.PeriodoAbierto

            Try
                repo.ActualizarUsuario(usuario)
                repo.ActualizarPerfil(perfil)
            Catch
                Return New ResultadoUsuario With {.Estado = ResultadoOperacionUsuario.Fallo, .Mensaje = "No se pudieron guardar los datos."}
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
