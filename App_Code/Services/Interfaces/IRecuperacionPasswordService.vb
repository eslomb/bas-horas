Imports DTOs.Usuarios

Namespace Services.Interfaces
    Public Interface IRecuperacionPasswordService
        Sub Solicitar(email As String)
        Function Validar(token As String) As Boolean
        Function Restablecer(token As String, password As String) As ResultadoRestablecerPassword
        Function RequiereCambioObligatorio(userName As String, password As String) As Boolean
        Function EmitirTokenCambioObligatorio(userName As String) As String
        Function ArmarLinkRestablecer(token As String) As String
    End Interface
End Namespace
