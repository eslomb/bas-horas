Imports DTOs.Costos

Namespace Services.Interfaces
    Public Interface ICostoService
        ' agrupacion: "actividad" (o vacío) o "centro".
        Function Costos(desde As Date?, hasta As Date?, agrupacion As String) As ResultadoCostos
    End Interface
End Namespace
