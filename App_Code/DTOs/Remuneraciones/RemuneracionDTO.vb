Imports System.ComponentModel

Namespace DTOs.Remuneraciones
    ' Legajo sale del Profile del usuario, no de la columna REMUNERACIONES.Legajo.
    ' IdRem va en el JSON pero no en el Excel de exportación.
    Public Class RemuneracionDTO
        <Browsable(False)>
        Public Property IdRem As Integer
        Public Property Anio As Integer
        Public Property Mes As Integer
        Public Property Usuario As String
        Public Property Legajo As String
        Public Property Remuneracion As Decimal
    End Class

    ' Fila de la plantilla de importación. Las columnas (y su orden) son las que espera Importar.
    Public Class RemuneracionPlantillaFilaDTO
        Public Property Anio As Integer
        Public Property Mes As Integer
        Public Property Usuario As String
        Public Property Remuneracion As Decimal
        Public Property Legajo As String
    End Class
End Namespace
