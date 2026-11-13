Imports System.Web.Security
Imports DTOs.Usuarios
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    ' Migración única Clear -> Hashed conservando las claves actuales. Idempotente: solo toma las que siguen
    ' en texto plano. No marca PasswordMigrada: así quedan detectables las que nadie cambió.
    Public Class MigracionPasswordService
        Implements IMigracionPasswordService

        Private ReadOnly repo As IMigracionPasswordRepository
        Private ReadOnly usuarios As IUsuarioRepository

        Public Sub New(repo As IMigracionPasswordRepository, usuarios As IUsuarioRepository)
            Me.repo = repo
            Me.usuarios = usuarios
        End Sub

        ' Con el provider todavía en Clear, migrar re-escribiría las mismas claves en texto plano.
        Public Function ProviderEsHashed() As Boolean Implements IMigracionPasswordService.ProviderEsHashed
            Return System.Web.Security.Membership.Provider.PasswordFormat = MembershipPasswordFormat.Hashed
        End Function

        Public Function Migrar() As ResultadoMigracionPasswordsDTO Implements IMigracionPasswordService.Migrar
            Dim resultado As New ResultadoMigracionPasswordsDTO

            For Each pendiente In repo.ObtenerPendientes()
                Dim userName = pendiente.UserName
                Try
                    If repo.Migrar(pendiente) Then
                        resultado.Migrados += 1
                    Else
                        ' El hash no validó (o la fila cambió en el medio): la clave quedó en texto plano como estaba.
                        Dim usuario = usuarios.ObtenerPorUserName(userName)
                        Dim motivo = If(usuario IsNot Nothing AndAlso usuario.IsLockedOut,
                                        "La cuenta está bloqueada: desbloquearla y volver a correr la migración.",
                                        "La clave hasheada no validó; se dejó en texto plano como estaba.")
                        resultado.Fallidos.Add(New FalloMigracionPasswordDTO With {.UserName = userName, .Motivo = motivo})
                    End If
                Catch ex As Exception
                    resultado.Fallidos.Add(New FalloMigracionPasswordDTO With {.UserName = userName, .Motivo = ex.GetBaseException().Message})
                End Try
            Next

            Return resultado
        End Function
    End Class
End Namespace
