Imports DTOs.Remuneraciones

Namespace Services.Interfaces
    Public Interface IRemuneracionService
        Function Listar(mes As Integer?, anio As Integer?) As ResultadoListadoRemuneraciones
        Function Crear(input As RemuneracionInputDTO) As ResultadoRemuneracion
        Function Actualizar(id As Integer, input As ActualizarRemuneracionInputDTO) As ResultadoRemuneracion
        Function Eliminar(id As Integer) As ResultadoRemuneracion
        Function GenerarPlantilla(mes As Integer?, anio As Integer?) As ResultadoArchivoRemuneraciones
        Function Exportar(mes As Integer?, anio As Integer?) As ResultadoArchivoRemuneraciones
        Function Importar(archivo As Byte()) As ResultadoImportacionRemuneraciones
    End Interface
End Namespace
