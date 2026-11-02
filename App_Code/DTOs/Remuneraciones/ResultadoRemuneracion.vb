Namespace DTOs.Remuneraciones
    Public Enum ResultadoOperacionRemuneracion
        Ok
        NoEncontrado
        Duplicado
        Invalido
        Fallo
    End Enum

    Public Class ResultadoRemuneracion
        Public Property Estado As ResultadoOperacionRemuneracion
        Public Property Remuneracion As RemuneracionDTO
        Public Property Mensaje As String
    End Class

    Public Class ResultadoListadoRemuneraciones
        Public Property Estado As ResultadoOperacionRemuneracion
        Public Property Items As List(Of RemuneracionDTO)
        Public Property Mensaje As String
    End Class

    ' Descarga de .xlsx (plantilla o exportación).
    Public Class ResultadoArchivoRemuneraciones
        Public Property Estado As ResultadoOperacionRemuneracion
        Public Property Contenido As Byte()
        Public Property NombreArchivo As String
        Public Property Mensaje As String
    End Class

    ' Fila = número de fila del Excel (1 = encabezado); 0 = error del archivo en sí (hoja o formato).
    Public Class ErrorImportacionRemuneracionDTO
        Public Property Fila As Integer
        Public Property Mensaje As String
    End Class

    ' Cuerpo de la respuesta de /importar.
    Public Class ImportacionRemuneracionesDTO
        Public Property Creados As Integer
        Public Property Actualizados As Integer
        Public Property Omitidos As Integer
        Public Property Errores As List(Of ErrorImportacionRemuneracionDTO) = New List(Of ErrorImportacionRemuneracionDTO)
    End Class

    ' Estado Invalido = el archivo tiene errores y no se escribió nada (detalle en Resumen.Errores).
    Public Class ResultadoImportacionRemuneraciones
        Public Property Estado As ResultadoOperacionRemuneracion
        Public Property Resumen As ImportacionRemuneracionesDTO
        Public Property Mensaje As String
    End Class
End Namespace
