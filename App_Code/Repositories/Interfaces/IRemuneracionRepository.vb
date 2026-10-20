Namespace Repositories.Interfaces
    Public Interface IRemuneracionRepository
        Function ListarRemuneraciones() As IQueryable(Of REMUNERACIONES)
        Function ListarDiasHabiles() As IQueryable(Of DIAS_HABILES)
    End Interface
End Namespace
