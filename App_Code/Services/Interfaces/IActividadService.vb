Imports DTOs.Actividades

Namespace Services.Interfaces
    Public Interface IActividadService
        Function Listar(soloHabilitadas As Boolean) As List(Of ActividadDTO)
        Function ObtenerPorId(id As Integer) As ResultadoActividad
        Function Crear(input As ActividadInputDTO) As ResultadoActividad
        Function Actualizar(id As Integer, input As ActividadInputDTO) As ResultadoActividad
        Function Eliminar(id As Integer) As ResultadoActividad
    End Interface
End Namespace
