Imports System.Linq
Imports System.Data.Entity
Imports Repositories.Interfaces

Namespace Repositories.EF
    Public Class RegistroRepository
        Implements IRegistroRepository

        Private ReadOnly cntx As BASHorasEntities

        Public Sub New(context As BASHorasEntities)
            cntx = context
        End Sub

        Public Function ObtenerPorUsuarioYRango(usuario As String, desde As Date, hasta As Date) As IQueryable(Of vRegistros) Implements IRegistroRepository.ObtenerPorUsuarioYRango
            ' Sin OrderBy acá a propósito: para que el orden se respete de verdad (y para que el
            ' Skip/Take de la paginación traduzca bien a SQL) tiene que quedar pegado al Service,
            ' justo antes de materializar. Ver ResultadoPaginado / RegistroService.Listar.
            Return cntx.vRegistros.
                Where(Function(r) r.Usuario = usuario AndAlso r.Fecha >= desde AndAlso r.Fecha <= hasta)
        End Function

        Public Function ObtenerVistaPorId(idRegistro As Integer) As vRegistros Implements IRegistroRepository.ObtenerVistaPorId
            Return cntx.vRegistros.FirstOrDefault(Function(r) r.IDReg = idRegistro)
        End Function

        Public Function ObtenerEntidadPorId(idRegistro As Integer) As REGISTROS Implements IRegistroRepository.ObtenerEntidadPorId
            Return cntx.REGISTROS.FirstOrDefault(Function(r) r.IDReg = idRegistro)
        End Function

        Public Function Existe(registro As REGISTROS) As Boolean Implements IRegistroRepository.Existe
            ' IDReg = 0 en un alta; en una edición excluye al propio registro para que no se detecte a sí mismo.
            Return cntx.REGISTROS.Any(Function(r) r.IDReg <> registro.IDReg AndAlso
                                                    r.Fecha = registro.Fecha AndAlso
                                                    r.idProducto = registro.idProducto AndAlso
                                                    r.idActividad = registro.idActividad AndAlso
                                                    r.Usuario = registro.Usuario AndAlso
                                                    r.CodPais = registro.CodPais AndAlso
                                                    r.Horas = registro.Horas AndAlso
                                                    r.Minutos = registro.Minutos AndAlso
                                                    r.Observaciones = registro.Observaciones)
        End Function

        Public Sub Agregar(registro As REGISTROS) Implements IRegistroRepository.Agregar
            cntx.REGISTROS.Add(registro)
            cntx.SaveChanges()
        End Sub

        Public Sub Actualizar(registro As REGISTROS) Implements IRegistroRepository.Actualizar
            cntx.REGISTROS.Attach(registro)
            cntx.Entry(registro).State = EntityState.Modified
            cntx.SaveChanges()
        End Sub

        Public Sub Eliminar(idRegistro As Integer) Implements IRegistroRepository.Eliminar
            Dim registro = ObtenerEntidadPorId(idRegistro)
            If registro IsNot Nothing Then
                cntx.REGISTROS.Remove(registro)
                cntx.SaveChanges()
            End If
        End Sub
    End Class
End Namespace
