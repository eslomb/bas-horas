Imports DTOs.Feriados

Namespace Services.Interfaces
    Public Interface IFeriadoService
        Function Listar(anio As Integer?) As List(Of FeriadoDTO)
        Function ObtenerPorFecha(fecha As Date) As ResultadoFeriado
        Function Crear(input As FeriadoInputDTO) As ResultadoFeriado
        Function Actualizar(fecha As Date, input As FeriadoInputDTO) As ResultadoFeriado
        Function Eliminar(fecha As Date) As ResultadoFeriado
    End Interface
End Namespace
