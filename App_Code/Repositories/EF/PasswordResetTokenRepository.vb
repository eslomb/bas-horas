Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class PasswordResetTokenRepository
        Implements IPasswordResetTokenRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        ' FechaCreacion es StoreGeneratedPattern=Computed: la pone el default de la base.
        Public Sub Crear(token As PASSWORD_RESET_TOKENS) Implements IPasswordResetTokenRepository.Crear
            cntx.PASSWORD_RESET_TOKENS.Add(token)
            cntx.SaveChanges()
        End Sub

        Public Function ObtenerPorHash(tokenHash As String) As PASSWORD_RESET_TOKENS Implements IPasswordResetTokenRepository.ObtenerPorHash
            Return cntx.PASSWORD_RESET_TOKENS.AsNoTracking().FirstOrDefault(Function(t) t.TokenHash = tokenHash)
        End Function

        ' UPDATE condicional: si dos pedidos usan el mismo token a la vez, solo uno afecta la fila.
        ' (En EF Core: ExecuteUpdate con el mismo Where.)
        Public Function MarcarUsado(idToken As Integer) As Boolean Implements IPasswordResetTokenRepository.MarcarUsado
            Dim filas = cntx.Database.ExecuteSqlCommand(
                "UPDATE dbo.PASSWORD_RESET_TOKENS SET FechaUso = GETDATE() WHERE IdToken = @p0 AND FechaUso IS NULL",
                idToken)
            Return filas = 1
        End Function

        ' Da por usados los tokens pendientes del usuario: solo vale el ultimo link pedido.
        Public Sub InvalidarDelUsuario(userName As String) Implements IPasswordResetTokenRepository.InvalidarDelUsuario
            cntx.Database.ExecuteSqlCommand(
                "UPDATE dbo.PASSWORD_RESET_TOKENS SET FechaUso = GETDATE() WHERE UserName = @p0 AND FechaUso IS NULL",
                userName)
        End Sub

        Public Function ContarRecientes(userName As String, origen As String, desde As Date) As Integer Implements IPasswordResetTokenRepository.ContarRecientes
            Return cntx.PASSWORD_RESET_TOKENS.Count(Function(t) t.UserName = userName AndAlso t.Origen = origen AndAlso t.FechaCreacion >= desde)
        End Function
    End Class
End Namespace
