Imports System.Linq
Imports DTOs
Imports DTOs.Registros
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class RegistroService
        Implements IRegistroService

        Private Shared ReadOnly ActivEnVigencia As String() = {"DFIX", "TFIX", "DFFX", "TFFX"}
        Private Shared ReadOnly ActivFueraProyecto As String() = {"DVER", "TVER"}

        Private ReadOnly repo As IRegistroRepository
        Private ReadOnly configuraciones As IConfiguracionRepository
        Private ReadOnly feriados As IFeriadoRepository
        Private ReadOnly productos As IProductoRepository
        Private ReadOnly actividades As IActividadRepository
        Private ReadOnly usuarios As IUsuarioRepository

        Public Sub New(repo As IRegistroRepository, configuraciones As IConfiguracionRepository,
                       feriados As IFeriadoRepository, productos As IProductoRepository,
                       actividades As IActividadRepository, usuarios As IUsuarioRepository)
            Me.repo = repo
            Me.configuraciones = configuraciones
            Me.feriados = feriados
            Me.productos = productos
            Me.actividades = actividades
            Me.usuarios = usuarios
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

        Public Function Crear(usuario As String, esAdministrador As Boolean, input As RegistroInputDTO) As ResultadoRegistro Implements IRegistroService.Crear
            Dim entidad As New REGISTROS With {
                .Usuario = usuario,
                .idProducto = input.IdProducto,
                .idActividad = input.IdActividad,
                .Fecha = input.Fecha.Date,
                .Horas = input.Horas,
                .Minutos = input.Minutos,
                .CodPais = input.CodPais,
                .Observaciones = input.Observaciones
            }

            Dim resultadoValidacion = Validar(entidad, esAdministrador)
            If resultadoValidacion IsNot Nothing Then Return resultadoValidacion

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

        Public Function Actualizar(idRegistro As Integer, usuario As String, esAdministrador As Boolean, input As RegistroInputDTO) As ResultadoRegistro Implements IRegistroService.Actualizar
            Dim entidad = repo.ObtenerEntidadPorId(idRegistro)

            If entidad Is Nothing Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.NoEncontrado}
            End If

            If Not entidad.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) Then
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Prohibido}
            End If

            ' Se valida sobre una copia sin trackear (con el mismo IDReg para que Existe lo excluya),
            ' antes de tocar la entidad que EF está siguiendo.
            Dim candidato As New REGISTROS With {
                .IDReg = idRegistro,
                .Usuario = entidad.Usuario,
                .idProducto = input.IdProducto,
                .idActividad = input.IdActividad,
                .Fecha = input.Fecha.Date,
                .Horas = input.Horas,
                .Minutos = input.Minutos,
                .CodPais = input.CodPais,
                .Observaciones = input.Observaciones
            }

            Dim resultadoValidacion = Validar(candidato, esAdministrador)
            If resultadoValidacion IsNot Nothing Then Return resultadoValidacion

            entidad.idProducto = input.IdProducto
            entidad.idActividad = input.IdActividad
            entidad.Fecha = input.Fecha.Date
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

            ' Como el legacy, eliminar solo se bloquea con la regla 1 (carga habilitada).
            If Not CargaHabilitada() Then Return Invalido(MensajeCargaDeshabilitada)

            Try
                repo.Eliminar(idRegistro)
            Catch
                Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Fallo, .Mensaje = "No se pudo eliminar el registro."}
            End Try

            Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Ok}
        End Function

        Private Const MensajeCargaDeshabilitada As String = "Disculpas, en este momento no está habilitada la carga de tareas."

        Private Shared Function Invalido(mensaje As String) As ResultadoRegistro
            Return New ResultadoRegistro With {.Estado = ResultadoOperacion.Invalido, .Mensaje = mensaje}
        End Function

        Private Function ValorConfig(codigo As String) As String
            Dim c = configuraciones.ObtenerPorCodigo(codigo)
            Return If(c Is Nothing OrElse c.Valor Is Nothing, "", c.Valor.Trim())
        End Function

        ' Solo un "False" explícito bloquea; si la configuración no existe se permite cargar.
        Private Function CargaHabilitada() As Boolean
            Return Not ValorConfig("PERMITIR_CARGAS").Equals("False", StringComparison.OrdinalIgnoreCase)
        End Function

        ' Devuelve Nothing si el registro es válido; si no, el resultado de error a devolver.
        ' Orden del legacy: 1, 8, 7, 2, 3, 4, 5, 6.
        Private Function Validar(r As REGISTROS, esAdministrador As Boolean) As ResultadoRegistro
            Dim hoy = Date.Today
            Dim fecha = r.Fecha.Date

            ' 1. Carga habilitada
            If Not CargaHabilitada() Then Return Invalido(MensajeCargaDeshabilitada)

            ' 8. Duración, producto y actividad
            Dim producto = productos.ObtenerPorId(r.idProducto)
            Dim actividad = actividades.ObtenerPorId(r.idActividad)
            If r.Horas < 0 OrElse r.Horas > 12 OrElse r.Minutos < 0 OrElse r.Minutos > 59 OrElse
               (r.Horas = 0 AndAlso r.Minutos = 0) OrElse producto Is Nothing OrElse actividad Is Nothing Then
                Return Invalido("No se puede ingresar una tarea sin definir su duración.")
            End If

            ' 7. Duplicado
            If repo.Existe(r) Then
                Return New ResultadoRegistro With {
                    .Estado = ResultadoOperacion.Duplicado,
                    .Mensaje = "Esta tarea ya se encuentra guardada en el sistema."
                }
            End If

            If Not esAdministrador Then
                ' 2. Rango de fechas
                Dim desde = CalcularFechaDesde(r.Usuario, hoy)
                If fecha < desde OrElse fecha > hoy Then
                    Return Invalido("No se permite guardar esta tarea en la fecha especificada. Fecha desde permitida: " &
                                    desde.ToString("dd/MM/yyyy") & " Fecha hasta permitida: " & hoy.ToString("dd/MM/yyyy"))
                End If

                ' 3. Fecha laborable
                If fecha.DayOfWeek = DayOfWeek.Saturday OrElse fecha.DayOfWeek = DayOfWeek.Sunday OrElse
                   feriados.ObtenerPorFecha(fecha) IsNot Nothing Then
                    Return Invalido("No se permite cargar tareas en fecha no laborable.")
                End If
            End If

            Dim codActividad = actividad.Codigo

            If producto.FProyIni.HasValue AndAlso producto.FProyFin.HasValue Then
                Dim ini = producto.FProyIni.Value.Date
                Dim fin = producto.FProyFin.Value.Date

                ' 4. Proyecto en curso
                If ActivEnVigencia.Contains(codActividad) Then
                    If fecha >= ini AndAlso fecha <= fin Then
                        Return Invalido("Proyecto en curso; no se admite la actividad " & codActividad)
                    End If
                    If fecha < ini Then
                        Return Invalido("La fecha debe ser igual o posterior a " & ini.ToString("dd/MM/yyyy"))
                    End If
                End If

                ' 5. Proyecto cerrado
                If ActivFueraProyecto.Contains(codActividad) AndAlso (fecha < ini OrElse fecha > fin) Then
                    Return Invalido("Proyecto cerrado; no admite DVER / TVER")
                End If
            End If

            ' 6. Observaciones obligatorias
            Dim conObservaciones = ValorConfig("ACTIV_CON_OBSERV").Replace(" ", "").Split(","c)
            If conObservaciones.Contains(codActividad) AndAlso String.IsNullOrWhiteSpace(r.Observaciones) Then
                Return Invalido("La actividad requiere que se complete el campo de Observaciones")
            End If

            Return Nothing
        End Function

        Private Function CalcularFechaDesde(usuario As String, hoy As Date) As Date
            Dim gracia = If(hoy.Day < 8, 7, 0)

            Dim periodoAbierto As Integer = 0
            Dim perfil = usuarios.ObtenerPerfil(usuario)
            If perfil IsNot Nothing Then periodoAbierto = perfil.PeriodoAbierto

            Dim baseDias = If(periodoAbierto > 0, periodoAbierto, gracia)

            Dim periodoGlobal As Integer
            If Not Integer.TryParse(ValorConfig("PERIODO_CARGAS"), periodoGlobal) Then periodoGlobal = 0

            Dim dias = Math.Max(baseDias, periodoGlobal)
            Return New Date(hoy.Year, hoy.Month, 1).AddDays(-dias)
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
