Namespace DTOs.Reportes
    Public Class ResultadoPaginadoDTO(Of T)
        Public Property Items As List(Of T)
        Public Property Pagina As Integer
        Public Property TamanioPagina As Integer
        Public Property TotalRegistros As Integer
        Public Property TotalPaginas As Integer
    End Class
End Namespace
