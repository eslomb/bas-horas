Imports System.Linq
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class PaisRepository
        Implements IPaisRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerTodos() As IQueryable(Of PAISES) Implements IPaisRepository.ObtenerTodos
            Return cntx.PAISES.OrderBy(Function(p) p.Pais)
        End Function

        Public Function ObtenerPreferidos() As IQueryable(Of PAISES) Implements IPaisRepository.ObtenerPreferidos
            Return cntx.PAISES.Where(Function(p) p.Preferido = True)
        End Function

        Public Function ObtenerPorCodigo(codigo As String) As PAISES Implements IPaisRepository.ObtenerPorCodigo
            ' Codigo es nchar(4): SQL Server ignora los espacios de relleno al comparar.
            Return cntx.PAISES.FirstOrDefault(Function(p) p.Codigo = codigo)
        End Function

        Public Function TieneRegistros(codigo As String) As Boolean Implements IPaisRepository.TieneRegistros
            Return cntx.REGISTROS.Any(Function(r) r.CodPais = codigo)
        End Function

        Public Sub Agregar(pais As PAISES) Implements IPaisRepository.Agregar
            cntx.PAISES.Add(pais)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(pais As PAISES) Implements IPaisRepository.Actualizar
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(pais As PAISES) Implements IPaisRepository.Eliminar
            cntx.PAISES.Remove(pais)
            cntx.SaveChanges()
        End Sub
    End Class
End Namespace
