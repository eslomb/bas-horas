Imports System.Linq
Imports DTOs.Feedback
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class FeedbackService
        Implements IFeedbackService

        Private Const LongitudMaximaComentario As Integer = 2000
        Private Const LongitudMaximaEmail As Integer = 256

        ' Mismos valores que el CHECK CK_FEEDBACK_Tipo de la tabla.
        Private Shared ReadOnly TiposValidos As String() = {"Sugerencia", "Error", "Otro"}

        Private ReadOnly repo As IFeedbackRepository
        Private ReadOnly usuarios As IUsuarioRepository

        Public Sub New(repo As IFeedbackRepository, usuarios As IUsuarioRepository)
            Me.repo = repo
            Me.usuarios = usuarios
        End Sub

        ' Administrador ve el de todos (o el de deUsuario si se indica); el resto, solo el propio.
        Public Function Listar(usuario As String, esAdministrador As Boolean, tipo As String, deUsuario As String) As List(Of FeedbackDTO) Implements IFeedbackService.Listar
            Dim consulta = repo.ObtenerTodos()

            If Not esAdministrador Then
                consulta = consulta.Where(Function(f) f.Usuario = usuario)
            ElseIf Not String.IsNullOrWhiteSpace(deUsuario) Then
                Dim filtroUsuario = deUsuario.Trim()
                consulta = consulta.Where(Function(f) f.Usuario = filtroUsuario)
            End If

            If Not String.IsNullOrWhiteSpace(tipo) Then
                Dim filtroTipo = tipo.Trim()
                consulta = consulta.Where(Function(f) f.Tipo = filtroTipo)
            End If

            Return consulta.
                OrderByDescending(Function(f) f.Fecha).
                ThenByDescending(Function(f) f.IDFeedback).
                ToList().
                Select(Function(f) MapearDTO(f)).
                ToList()
        End Function

        Public Function ObtenerPorId(id As Integer, usuario As String, esAdministrador As Boolean) As ResultadoFeedback Implements IFeedbackService.ObtenerPorId
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.NoEncontrado}
            End If

            If Not esAdministrador AndAlso Not EsDueno(entidad, usuario) Then
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Prohibido}
            End If

            Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Ok, .Feedback = MapearDTO(entidad)}
        End Function

        Public Function Crear(usuario As String, input As FeedbackInputDTO) As ResultadoFeedback Implements IFeedbackService.Crear
            Dim entidad As New FEEDBACK With {.Usuario = usuario}

            Dim invalido = AplicarInput(entidad, usuario, input)
            If invalido IsNot Nothing Then Return invalido

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Fallo, .Mensaje = "No se pudo guardar el comentario."}
            End Try

            Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Ok, .Feedback = MapearDTO(entidad)}
        End Function

        ' Solo el autor edita su comentario (ni siquiera un Administrador puede cambiar lo que escribió otro).
        Public Function Actualizar(id As Integer, usuario As String, input As FeedbackInputDTO) As ResultadoFeedback Implements IFeedbackService.Actualizar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.NoEncontrado}
            End If

            If Not EsDueno(entidad, usuario) Then
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Prohibido}
            End If

            Dim invalido = AplicarInput(entidad, usuario, input)
            If invalido IsNot Nothing Then Return invalido

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Fallo, .Mensaje = "No se pudo actualizar el comentario."}
            End Try

            Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Ok, .Feedback = MapearDTO(entidad)}
        End Function

        ' Borra el autor o un Administrador (moderación).
        Public Function Eliminar(id As Integer, usuario As String, esAdministrador As Boolean) As ResultadoFeedback Implements IFeedbackService.Eliminar
            Dim entidad = repo.ObtenerPorId(id)

            If entidad Is Nothing Then
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.NoEncontrado}
            End If

            If Not esAdministrador AndAlso Not EsDueno(entidad, usuario) Then
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Prohibido}
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Fallo, .Mensaje = "No se pudo eliminar el comentario."}
            End Try

            Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Ok}
        End Function

        ' Valida el input y, si es correcto, lo copia a la entidad. Devuelve Nothing si todo está bien.
        Private Function AplicarInput(entidad As FEEDBACK, usuario As String, input As FeedbackInputDTO) As ResultadoFeedback
            Dim comentario = If(input.Comentario, "").Trim()
            If comentario.Length = 0 Then Return Invalido("El comentario es obligatorio.")
            If comentario.Length > LongitudMaximaComentario Then
                Return Invalido("El comentario puede tener hasta " & LongitudMaximaComentario & " caracteres.")
            End If

            Dim tipo = TiposValidos.FirstOrDefault(Function(t) t.Equals(If(input.Tipo, "").Trim(), StringComparison.OrdinalIgnoreCase))
            If tipo Is Nothing Then Return Invalido("El tipo debe ser uno de: " & String.Join(", ", TiposValidos) & ".")

            Dim email = If(input.Email, "").Trim()
            If email.Length = 0 Then email = ObtenerEmailUsuario(usuario)
            If email IsNot Nothing Then
                If email.Length > LongitudMaximaEmail Then
                    Return Invalido("El email puede tener hasta " & LongitudMaximaEmail & " caracteres.")
                End If
                If Not EsEmailValido(email) Then Return Invalido("El email no es válido.")
            End If

            entidad.Comentario = comentario
            entidad.Tipo = tipo
            entidad.Email = email
            Return Nothing
        End Function

        ' Email de Membership; Nothing si el usuario no tiene (la columna es nullable).
        Private Function ObtenerEmailUsuario(usuario As String) As String
            Dim miembro = usuarios.ObtenerPorUserName(usuario)
            If miembro Is Nothing OrElse String.IsNullOrWhiteSpace(miembro.Email) Then Return Nothing
            Return miembro.Email.Trim()
        End Function

        Private Shared Function EsEmailValido(email As String) As Boolean
            Try
                Return New System.Net.Mail.MailAddress(email).Address = email
            Catch
                Return False
            End Try
        End Function

        Private Shared Function EsDueno(entidad As FEEDBACK, usuario As String) As Boolean
            Return String.Equals(entidad.Usuario, usuario, StringComparison.OrdinalIgnoreCase)
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoFeedback
            Return New ResultadoFeedback With {.Estado = ResultadoOperacionFeedback.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(f As FEEDBACK) As FeedbackDTO
            Return New FeedbackDTO With {
                .IDFeedback = f.IDFeedback,
                .Usuario = f.Usuario,
                .Email = f.Email,
                .Comentario = f.Comentario,
                .Tipo = f.Tipo,
                .Fecha = f.Fecha
            }
        End Function
    End Class
End Namespace
