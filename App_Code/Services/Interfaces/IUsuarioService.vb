Imports DTOs.Usuarios

Namespace Services.Interfaces
    Public Interface IUsuarioService
        Function ObtenerPorUserName(userName As String) As UsuarioDTO
        Function Listar() As List(Of UsuarioDTO)
        Function ActualizarPerfilPropio(userName As String, input As ActualizarPerfilPropioInputDTO) As ResultadoUsuario
        Function CrearUsuario(input As CrearUsuarioInputDTO) As ResultadoUsuario
        Function ActualizarUsuario(userName As String, input As ActualizarUsuarioInputDTO, usuarioActual As String) As ResultadoUsuario
        Function CambiarPassword(userName As String, input As CambiarPasswordInputDTO) As ResultadoCambioPassword
        Function RecuperarPassword(email As String) As ResultadoRecuperarPassword
    End Interface
End Namespace
