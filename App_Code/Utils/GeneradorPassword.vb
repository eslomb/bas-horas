Imports System.Security.Cryptography

Namespace Utils
    ' Genera passwords aleatorias usando un generador criptográficamente seguro
    ' (a diferencia de System.Random, que no es seguro y puede repetir secuencia
    ' si se crean varias instancias casi al mismo tiempo en threads distintos).
    Public Module GeneradorPassword

        Private Const Minusculas As String = "abcdefghijkmnpqrstuvwxyz"
        Private Const Mayusculas As String = "ABCDEFGHJKLMNPQRSTUVWXYZ"
        Private Const Digitos As String = "23456789"
        Private Const TodosLosCaracteres As String = Minusculas & Mayusculas & Digitos
        ' Nota: se excluyen 0/O, 1/I/l por ser visualmente ambiguos al leerlos en un mail.

        Public Function Generar(Optional longitud As Integer = 10) As String
            If longitud < 4 Then longitud = 4

            Dim buffer(longitud - 1) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(buffer)
            End Using

            ' Garantiza al menos una minúscula, una mayúscula y un dígito.
            Dim resultado(longitud - 1) As Char
            resultado(0) = Minusculas(buffer(0) Mod Minusculas.Length)
            resultado(1) = Mayusculas(buffer(1) Mod Mayusculas.Length)
            resultado(2) = Digitos(buffer(2) Mod Digitos.Length)

            For i As Integer = 3 To longitud - 1
                resultado(i) = TodosLosCaracteres(buffer(i) Mod TodosLosCaracteres.Length)
            Next

            Mezclar(resultado, buffer)
            Return New String(resultado)
        End Function

        Private Sub Mezclar(valores As Char(), semillaAleatoria As Byte())
            ' Fisher-Yates, para que las posiciones 0/1/2 (minúscula/mayúscula/dígito garantizados)
            ' no queden siempre en el mismo lugar.
            For i As Integer = valores.Length - 1 To 1 Step -1
                Dim j = semillaAleatoria(i) Mod (i + 1)
                Dim temp = valores(i)
                valores(i) = valores(j)
                valores(j) = temp
            Next
        End Sub

    End Module
End Namespace
