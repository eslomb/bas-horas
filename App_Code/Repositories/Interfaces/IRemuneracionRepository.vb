Imports System.Linq

Namespace Repositories.Interfaces
    Public Interface IRemuneracionRepository
        ' Solo lectura (AsNoTracking). Lo usan los reportes y los listados.
        Function ListarRemuneraciones() As IQueryable(Of REMUNERACIONES)

        ' Entidades trackeadas: las modificaciones se persisten con Guardar().
        Function ObtenerPorId(id As Integer) As REMUNERACIONES
        Function ObtenerPorAnios(anios As IEnumerable(Of Integer)) As IQueryable(Of REMUNERACIONES)
        Function Existe(usuario As String, anio As Integer, mes As Integer, excluirId As Integer) As Boolean

        Sub Agregar(remuneracion As REMUNERACIONES)
        Sub Actualizar(remuneracion As REMUNERACIONES)
        Sub Eliminar(remuneracion As REMUNERACIONES)

        ' Para operaciones de varias filas: se acumulan los cambios y Guardar() los persiste todos
        ' en un único SaveChanges (una transacción: o se guarda todo o nada).
        Sub AgregarSinGuardar(remuneracion As REMUNERACIONES)
        Sub Guardar()
    End Interface
End Namespace
