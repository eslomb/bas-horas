Imports DTOs.Centros

Namespace Services.Interfaces
    Public Interface ICentroService
        Function Listar() As List(Of CentroDTO)
        Function ObtenerPorCodigo(codigo As String) As ResultadoCentro
        Function Crear(input As CentroInputDTO) As ResultadoCentro
        Function Actualizar(codigo As String, input As CentroInputDTO) As ResultadoCentro
        Function Eliminar(codigo As String) As ResultadoCentro
    End Interface
End Namespace
