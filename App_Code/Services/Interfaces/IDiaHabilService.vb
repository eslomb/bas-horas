Imports DTOs.DiasHabiles

Namespace Services.Interfaces
    Public Interface IDiaHabilService
        Function Listar(anio As Integer?) As List(Of DiaHabilDTO)
        Function ObtenerPorId(id As Integer) As ResultadoDiaHabil
        Function Crear(input As DiaHabilInputDTO) As ResultadoDiaHabil
        Function Actualizar(id As Integer, input As DiaHabilInputDTO) As ResultadoDiaHabil
        Function Eliminar(id As Integer) As ResultadoDiaHabil
    End Interface
End Namespace
