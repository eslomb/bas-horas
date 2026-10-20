Imports DTOs.Paises

Namespace Services.Interfaces
    Public Interface IPaisService
        Function Listar() As List(Of PaisDTO)
        Function ObtenerPorCodigo(codigo As String) As ResultadoPais
        Function Crear(input As PaisInputDTO) As ResultadoPais
        Function Actualizar(codigo As String, input As PaisInputDTO) As ResultadoPais
        Function Eliminar(codigo As String) As ResultadoPais
    End Interface
End Namespace
