Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class ConfiguracionRepository
        Implements IConfiguracionRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodas() As IQueryable(Of CONFIGURACIONES) Implements IConfiguracionRepository.ObtenerTodas
            Return cntx.CONFIGURACIONES.OrderBy(Function(c) c.CodConfig)
        End Function

        Public Function ObtenerPorCodigo(codigo As String) As CONFIGURACIONES Implements IConfiguracionRepository.ObtenerPorCodigo
            Return cntx.CONFIGURACIONES.FirstOrDefault(Function(c) c.CodConfig = codigo)
        End Function

        Public Sub Actualizar(configuracion As CONFIGURACIONES) Implements IConfiguracionRepository.Actualizar
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
