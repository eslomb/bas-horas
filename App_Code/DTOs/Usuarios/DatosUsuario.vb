Namespace DTOs.Usuarios
    <Serializable>
    Public Class DatosUsuario
        ' para coleccionar los datos de cada usuario que se guardan en Membership + los datos de Profile

        Public Property UserName As String

        Public Property Email As String

        Public Property IsApproved As Boolean

        Public Property Comentario As String

        Public Property Legajo As String

        Public Property Modalidad As Decimal ' alias Jornada 

        Public Property PeriodoAbierto As Int16

        Public Property Centro As String

        Public Property CUIL As String

        Public Property Dependencia As String


        Public ReadOnly Property Habilitado() As String
            Get
                Return IIf(IsApproved, "SI", "NO")
            End Get
        End Property


    End Class
End Namespace