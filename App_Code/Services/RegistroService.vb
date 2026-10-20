Imports System.Linq
Imports DTOs
Imports DTOs.Registros
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class RegistroService
        Implements IRegistroService

        Private ReadOnly repo As IRegistroRepository

        Public Sub New(repo As IRegistroRepository)
            Me.repo = repo
        End Sub

        Public Function Listar(usuario As String, desde As Date, hasta As Date, pagina As Integer, tamanioPagina As Integer) As ResultadoPaginado(Of RegistroDTO) Implements IRegistroService.Listar
            Dim query = repo.ObtenerPorUsuarioYRango(usuario, desde, hasta)

            Dim total = query.Count()

            Dim paginaDeEntidades = query.
                OrderBy(Function(r) r.Fecha).ThenBy(Function(r) r.IDReg).
                Skip((pagina - 1) * tamanioPagina).
                Take(tamanioPagina).
                ToList()

            Dim items = (From r In paginaDeEntidades
                         Select MapearDTO(r)).ToList()

            Return New ResultadoPaginado(Of RegistroDTO) With {
                .Items = items,
                .TotalRegistros = total,
                .Pagina = pagina,
                .TamanioPagina = tamanioPagina
            }
        End Function

        Public Function ObtenerPorId(idRegistro As Integer, usuario As String) As ResultadoRegistro Implements IRegistroService.ObtenerPorId
            Dim vista = repo.ObtenerVistaPorId(idRegistro)

            If vista Is Nothing Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.NoEncontrado}
            End If

            If Not vista.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Prohibido}
            End If

            Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Ok, .Registro = MapearDTO(vista)}
        End Function

        Public Function Crear(usuario As String, input As RegistroInputDTO) As ResultadoRegistro Implements IRegistroService.Crear
            Dim entidad As New REGISTROS With {
                .Usuario = usuario,
                .idProducto = input.IdProducto,
                .idActividad = input.IdActividad,
                .Fecha = input.Fecha,
                .Horas = input.Horas,
                .Minutos = input.Minutos,
                .CodPais = input.CodPais,
                .Observaciones = input.Observaciones
            }

            If repo.Existe(entidad) Then
                Return New ResultadoRegistro With {
                    .Estado = ResultadoOperacion.Duplicado,
                    .Mensaje = "Ya existe un registro idéntico para ese usuario y fecha."
                }
            End If

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Fallo, .Mensaje = "No se pudo guardar el registro."}
            End Try

            Return New ResultadoRegistro With {
                .Estado = ResultadoOperacion.Ok,
                .Registro = MapearDTO(repo.ObtenerVistaPorId(entidad.IDReg))
            }
        End Function

        Public Function Actualizar(idRegistro As Integer, usuario As String, input As RegistroInputDTO) As ResultadoRegistro Implements IRegistroService.Actualizar
            Dim entidad = repo.ObtenerEntidadPorId(idRegistro)

            If entidad Is Nothing Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Prohibido}
            End If

            entidad.idProducto = input.IdProducto
            entidad.idActividad = input.IdActividad
            entidad.Fecha = input.Fecha
            entidad.Horas = input.Horas
            entidad.Minutos = input.Minutos
            entidad.CodPais = input.CodPais
            entidad.Observaciones = input.Observaciones

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Fallo, .Mensaje = "No se pudo actualizar el registro."}
            End Try

            Return New ResultadoRegistro With {
                .Estado = ResultadoOperacion.Ok,
                .Registro = MapearDTO(repo.ObtenerVistaPorId(idRegistro))
            }
        End Function

        Public Function Eliminar(idRegistro As Integer, usuario As String) As ResultadoRegistro Implements IRegistroService.Eliminar
            Dim entidad = repo.ObtenerEntidadPorId(idRegistro)

            If entidad Is Nothing Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Prohibido}
            End If

            Try
                repo.Eliminar(idRegistro)
            Catch
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Fallo, .Mensaje = "No se pudo eliminar el registro."}
            End Try

            Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Ok}
        End Function

        Private Shared Function MapearDTO(v As vRegistros) As RegistroDTO
            Return New RegistroDTO With {
                .IDReg = v.IDReg,
                .Fecha = v.Fecha,
                .Usuario = v.Usuario,
                .IdProducto = v.idProducto,
                .CodProducto = v.CodProducto,
                .Producto = v.Producto,
                .IdActividad = v.idActividad,
                .CodActividad = v.Codigo,
                .Actividad = v.Actividad,
                .CodPais = v.CodPais,
                .Pais = v.Pais,
                .Horas = v.Horas,
                .Minutos = v.Minutos,
                .Observaciones = v.Observaciones
            }
        End Function
    End Class
End Namespace
