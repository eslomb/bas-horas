Namespace DTOs
    ' Envelope genérico reusable para cualquier listado paginado de la API.
    Public Class ResultadoPaginado(Of T)
        Public Property Items As List(Of T)
        Public Property TotalRegistros As Integer
        Public Property Pagina As Integer
        Public Property TamanioPagina As Integer

        Public ReadOnly Property TotalPaginas As Integer
            Get
                If TamanioPagina <= 0 Then Return 0
                Return CInt(Math.Ceiling(TotalRegistros / CDbl(TamanioPagina)))
            End Get
        End Property
    End Class
End Namespace
