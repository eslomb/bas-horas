Imports DTOs
Imports DTOs.Registros

Namespace Services.Interfaces
    Public Interface IRegistroService
        Function Listar(usuario As String, desde As Date, hasta As Date, pagina As Integer, tamanioPagina As Integer) As ResultadoPaginado(Of RegistroDTO)
        Function ObtenerPorId(idRegistro As Integer, usuario As String) As ResultadoRegistro
        Function Crear(usuario As String, input As RegistroInputDTO) As ResultadoRegistro
        Function Actualizar(idRegistro As Integer, usuario As String, input As RegistroInputDTO) As ResultadoRegistro
        Function Eliminar(idRegistro As Integer, usuario As String) As ResultadoRegistro
    End Interface
End Namespace
