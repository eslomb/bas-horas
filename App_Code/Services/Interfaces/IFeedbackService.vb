Imports DTOs.Feedback

Namespace Services.Interfaces
    Public Interface IFeedbackService
        Function Listar(usuario As String, esAdministrador As Boolean, tipo As String, deUsuario As String) As List(Of FeedbackDTO)
        Function ObtenerPorId(id As Integer, usuario As String, esAdministrador As Boolean) As ResultadoFeedback
        Function Crear(usuario As String, input As FeedbackInputDTO) As ResultadoFeedback
        Function Actualizar(id As Integer, usuario As String, input As FeedbackInputDTO) As ResultadoFeedback
        Function Eliminar(id As Integer, usuario As String, esAdministrador As Boolean) As ResultadoFeedback
    End Interface
End Namespace
