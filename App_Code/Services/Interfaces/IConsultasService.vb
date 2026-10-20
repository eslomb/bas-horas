Imports DTOs.Consultas

Namespace Services.Interfaces
    Public Interface IConsultasService
        Function ListarProductos(soloActivos As Boolean) As List(Of ProductoDTO)
        Function ListarActividades(soloHabilitadas As Boolean) As List(Of ActividadDTO)
        Function ListarPaises() As List(Of PaisDTO)
        Function ListarCentros() As List(Of CentroDTO)
    End Interface
End Namespace
