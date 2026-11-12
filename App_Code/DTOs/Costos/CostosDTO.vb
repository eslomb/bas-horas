Namespace DTOs.Costos
    ' Qué agrupa cada ítem del gráfico de costos.
    Public Enum AgrupacionCostos
        ' Código de tarea (COD_ACTIVIDAD) del registro.
        Actividad
        ' Centro del perfil actual del usuario que cargó el registro (como la columna Centro de los reportes).
        Centro
    End Enum

    ' Costo total de un grupo (actividad o centro) en el período: suma de "Costo Tarea" de sus registros.
    Public Class CostoItemDTO
        ' Código de la actividad o del centro.
        Public Property Codigo As String
        ' Nombre de la actividad o del centro, si está en el catálogo.
        Public Property Nombre As String
        Public Property Costo As Decimal
        ' Duración de todos los registros del grupo, con o sin costo.
        Public Property Horas As Integer
        Public Property Minutos As Integer
        Public Property Registros As Integer
        ' Registros del grupo que no suman costo por falta de remuneración, días hábiles o jornada.
        Public Property RegistrosSinCosto As Integer
    End Class

    Public Class CostosDTO
        ' "actividad" o "centro".
        Public Property Agrupacion As String
        Public Property Desde As Date
        Public Property Hasta As Date
        ' Ordenados por costo, de mayor a menor.
        Public Property Items As List(Of CostoItemDTO)
        Public Property Total As Decimal
        Public Property RegistrosSinCosto As Integer
        ' Usuarios con algún registro sin costo, para saber qué dato falta cargar.
        Public Property UsuariosSinCosto As List(Of String)
    End Class

    Public Enum ResultadoOperacionCostos
        Ok
        Invalido
    End Enum

    Public Class ResultadoCostos
        Public Property Estado As ResultadoOperacionCostos
        Public Property Costos As CostosDTO
        Public Property Mensaje As String
    End Class
End Namespace
