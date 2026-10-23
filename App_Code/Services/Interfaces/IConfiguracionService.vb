Imports DTOs.Configuraciones

Namespace Services.Interfaces
    Public Interface IConfiguracionService
        Function Listar() As List(Of ConfiguracionDTO)
        Function ObtenerPorCodigo(codigo As String) As ResultadoConfiguracion
        Function ActualizarValor(codigo As String, input As ConfiguracionInputDTO) As ResultadoConfiguracion
    End Interface
End Namespace
