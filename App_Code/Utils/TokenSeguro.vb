Imports System.Security.Cryptography
Imports System.Text

Namespace Utils
    ' Tokens de un solo uso (ej. link de recuperacion de contrasena).
    ' El token en claro solo viaja en el link; en la base se guarda Hash(token).
    Public Module TokenSeguro

        Private Const BytesToken As Integer = 32

        ' 256 bits de RandomNumberGenerator en base64url (sin '+', '/' ni '='), apto para ir en una URL.
        Public Function Generar() As String
            Dim bytes(BytesToken - 1) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(bytes)
            End Using

            Return Convert.ToBase64String(bytes).TrimEnd("="c).Replace("+"c, "-"c).Replace("/"c, "_"c)
        End Function

        ' SHA-256 en hex minuscula (64 caracteres). Sin salt: el token ya es aleatorio de 256 bits.
        Public Function Hash(token As String) As String
            Using sha = SHA256.Create()
                Dim digest = sha.ComputeHash(Encoding.UTF8.GetBytes(token))
                Dim sb As New StringBuilder(digest.Length * 2)
                For Each b In digest
                    sb.Append(b.ToString("x2"))
                Next
                Return sb.ToString()
            End Using
        End Function

    End Module
End Namespace
