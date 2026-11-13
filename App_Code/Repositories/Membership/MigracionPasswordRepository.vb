Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Web.Security
Imports Repositories.Interfaces

Namespace Repositories.Membership
    ' Migración única de contraseñas Clear (PasswordFormat = 0) a Hashed (1), conservando la clave.
    ' Se borra junto con su endpoint cuando todos los ambientes estén migrados.
    '
    ' Ojo: no se puede hacer con la API pública de Membership. SqlMembershipProvider guarda el formato por usuario
    ' y ChangePassword/ResetPassword re-graban la clave con el formato y salt que YA tenía ese usuario (verificado:
    ' ChangePassword(x, x) deja PasswordFormat = 0). Por eso el hash se calcula con el EncodePassword privado del
    ' propio provider (mismo algoritmo que usa ValidateUser) y se escribe directo en aspnet_Membership.
    Public Class MigracionPasswordRepository
        Implements IMigracionPasswordRepository

        Private Const FormatoClear As Integer = 0
        Private Const FormatoHashed As Integer = 1
        Private Const BytesSalt As Integer = 16

        Public Function ObtenerPendientes() As List(Of PasswordPendienteMigracion) Implements IMigracionPasswordRepository.ObtenerPendientes
            Const sql As String =
                "SELECT u.UserName, m.Password, m.PasswordSalt " &
                "FROM dbo.aspnet_Membership m " &
                "JOIN dbo.aspnet_Users u ON u.UserId = m.UserId " &
                "JOIN dbo.aspnet_Applications a ON a.ApplicationId = m.ApplicationId " &
                "WHERE m.PasswordFormat = 0 AND a.LoweredApplicationName = @app " &
                "ORDER BY u.UserName"

            Dim resultado As New List(Of PasswordPendienteMigracion)

            Using cn = AbrirConexion()
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@app", NombreAplicacion())
                    Using dr = cmd.ExecuteReader()
                        While dr.Read()
                            resultado.Add(New PasswordPendienteMigracion With {
                                .UserName = dr.GetString(0),
                                .Password = dr.GetString(1),
                                .PasswordSalt = dr.GetString(2)
                            })
                        End While
                    End Using
                End Using
            End Using

            Return resultado
        End Function

        ' Escribe el hash, verifica con el provider que la misma clave valide y, si no valida, vuelve al texto
        ' plano original. False = no se migró (la clave quedó como estaba).
        Public Function Migrar(pendiente As PasswordPendienteMigracion) As Boolean Implements IMigracionPasswordRepository.Migrar
            Dim provider = ObtenerProvider()

            Dim salt = GenerarSalt()
            Dim hash = CStr(MetodoEncodePassword.Invoke(provider, New Object() {pendiente.Password, FormatoHashed, salt}))

            ' Condicional: solo si sigue en Clear con la misma clave (comparación binaria; la collation es CI).
            If Not Escribir(pendiente.UserName, hash, salt, FormatoHashed, pendiente.Password, FormatoClear) Then Return False

            Dim valida = CBool(MetodoCheckPassword.Invoke(provider, New Object() {pendiente.UserName, pendiente.Password, False, False}))
            If valida Then Return True

            Escribir(pendiente.UserName, pendiente.Password, pendiente.PasswordSalt, FormatoClear, hash, FormatoHashed)
            Return False
        End Function

        Private Function Escribir(userName As String, password As String, salt As String, formato As Integer,
                                  passwordEsperada As String, formatoEsperado As Integer) As Boolean
            Const sql As String =
                "UPDATE m SET m.Password = @password, m.PasswordSalt = @salt, m.PasswordFormat = @formato " &
                "FROM dbo.aspnet_Membership m " &
                "JOIN dbo.aspnet_Users u ON u.UserId = m.UserId " &
                "JOIN dbo.aspnet_Applications a ON a.ApplicationId = m.ApplicationId " &
                "WHERE u.LoweredUserName = @user AND a.LoweredApplicationName = @app " &
                "AND m.PasswordFormat = @formatoEsperado " &
                "AND CAST(m.Password AS varbinary(256)) = CAST(@passwordEsperada AS varbinary(256))"

            Using cn = AbrirConexion()
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar, 128).Value = password
                    cmd.Parameters.Add("@salt", SqlDbType.NVarChar, 128).Value = salt
                    cmd.Parameters.Add("@formato", SqlDbType.Int).Value = formato
                    cmd.Parameters.Add("@user", SqlDbType.NVarChar, 256).Value = userName.ToLowerInvariant()
                    cmd.Parameters.Add("@app", SqlDbType.NVarChar, 256).Value = NombreAplicacion()
                    cmd.Parameters.Add("@formatoEsperado", SqlDbType.Int).Value = formatoEsperado
                    cmd.Parameters.Add("@passwordEsperada", SqlDbType.NVarChar, 128).Value = passwordEsperada
                    Return cmd.ExecuteNonQuery() = 1
                End Using
            End Using
        End Function

        ' Mismo salt que genera SqlMembershipProvider (16 bytes aleatorios en base64).
        Private Shared Function GenerarSalt() As String
            Dim bytes(BytesSalt - 1) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(bytes)
            End Using
            Return Convert.ToBase64String(bytes)
        End Function

        Private Shared Function ObtenerProvider() As SqlMembershipProvider
            Dim provider = TryCast(System.Web.Security.Membership.Provider, SqlMembershipProvider)
            If provider Is Nothing Then
                Throw New InvalidOperationException("El provider de Membership no es SqlMembershipProvider.")
            End If
            Return provider
        End Function

        ' Métodos privados de SqlMembershipProvider (System.Web 4.x, congelado). Si no aparecen, la migración
        ' se corta antes de escribir nada.
        Private Shared ReadOnly MetodoEncodePassword As MethodInfo = BuscarMetodo("EncodePassword", GetType(String), GetType(Integer), GetType(String))
        Private Shared ReadOnly MetodoCheckPassword As MethodInfo = BuscarMetodo("CheckPassword", GetType(String), GetType(String), GetType(Boolean), GetType(Boolean))

        Private Shared Function BuscarMetodo(nombre As String, ParamArray tipos As Type()) As MethodInfo
            Dim metodo = GetType(SqlMembershipProvider).GetMethod(nombre, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public, Nothing, tipos, Nothing)
            If metodo Is Nothing Then
                Throw New InvalidOperationException("No se encontró SqlMembershipProvider." & nombre & " con la firma esperada.")
            End If
            Return metodo
        End Function

        Private Shared Function NombreAplicacion() As String
            Return System.Web.Security.Membership.ApplicationName.ToLowerInvariant()
        End Function

        Private Shared Function AbrirConexion() As SqlConnection
            Dim cn As New SqlConnection(ConfigurationManager.ConnectionStrings("LocalSqlServer").ConnectionString)
            cn.Open()
            Return cn
        End Function
    End Class
End Namespace
