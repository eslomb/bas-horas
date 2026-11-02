Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports ClosedXML.Excel
Imports DTOs.Remuneraciones
Imports Repositories.Interfaces
Imports Services.Interfaces
Imports Utils

Namespace Services
    Public Class RemuneracionService
        Implements IRemuneracionService

        Private Const AnioMinimo As Integer = 2000
        Private Const AnioMaximo As Integer = 2100

        ' REMUNERACIONES.Remuneracion es decimal(18,2): 16 dígitos enteros y 2 decimales.
        Private Const MontoMaximo As Decimal = 9999999999999999.99D

        Private Const HojaImportacion As String = "Remuneraciones"
        Private Const MaximoBytesImportacion As Integer = 5 * 1024 * 1024
        Private Const MaximoFilasImportacion As Integer = 10000

        Private Shared ReadOnly CulturaAr As New CultureInfo("es-AR")

        Private ReadOnly repo As IRemuneracionRepository
        Private ReadOnly usuarios As IUsuarioRepository

        Public Sub New(repo As IRemuneracionRepository, usuarios As IUsuarioRepository)
            Me.repo = repo
            Me.usuarios = usuarios
        End Sub

        ' ---------------------------------------------------------------- CRUD

        Public Function Listar(mes As Integer?, anio As Integer?) As ResultadoListadoRemuneraciones Implements IRemuneracionService.Listar
            Dim mensaje = ValidarFiltro(mes, anio)
            If mensaje IsNot Nothing Then
                Return New ResultadoListadoRemuneraciones With {.Estado = ResultadoOperacionRemuneracion.Invalido, .Mensaje = mensaje}
            End If

            Return New ResultadoListadoRemuneraciones With {.Estado = ResultadoOperacionRemuneracion.Ok, .Items = ListarDTOs(mes, anio)}
        End Function

        Public Function Crear(input As RemuneracionInputDTO) As ResultadoRemuneracion Implements IRemuneracionService.Crear
            Dim nombre = If(input.Usuario, "").Trim()
            If nombre.Length = 0 Then Return Invalido("El usuario es obligatorio.")

            If Not input.Anio.HasValue OrElse input.Anio.Value < AnioMinimo OrElse input.Anio.Value > AnioMaximo Then
                Return Invalido("El año es obligatorio y debe estar entre " & AnioMinimo & " y " & AnioMaximo & ".")
            End If
            If Not input.Mes.HasValue OrElse input.Mes.Value < 1 OrElse input.Mes.Value > 12 Then
                Return Invalido("El mes es obligatorio y debe estar entre 1 y 12.")
            End If
            If Not input.Remuneracion.HasValue Then Return Invalido("La remuneración es obligatoria.")

            Dim monto As Decimal
            Dim errorMonto = ValidarMonto(input.Remuneracion.Value, monto)
            If errorMonto IsNot Nothing Then Return Invalido(errorMonto)

            Dim usuario = usuarios.ObtenerPorUserName(nombre)
            If usuario Is Nothing Then Return Invalido("El usuario '" & nombre & "' no existe.")

            If repo.Existe(usuario.UserName, input.Anio.Value, input.Mes.Value, 0) Then
                Return New ResultadoRemuneracion With {
                    .Estado = ResultadoOperacionRemuneracion.Duplicado,
                    .Mensaje = "Ya existe una remuneración de ese usuario para ese año y mes."
                }
            End If

            Dim entidad As New REMUNERACIONES With {
                .Usuario = usuario.UserName,
                .Anio = CShort(input.Anio.Value),
                .Mes = CShort(input.Mes.Value),
                .Remuneracion = monto
            }

            Try
                repo.Agregar(entidad)
            Catch
                Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.Fallo, .Mensaje = "No se pudo guardar la remuneración."}
            End Try

            Return New ResultadoRemuneracion With {
                .Estado = ResultadoOperacionRemuneracion.Ok,
                .Remuneracion = MapearDTO(entidad, LegajoDePerfil(entidad.Usuario))
            }
        End Function

        Public Function Actualizar(id As Integer, input As ActualizarRemuneracionInputDTO) As ResultadoRemuneracion Implements IRemuneracionService.Actualizar
            If Not input.Remuneracion.HasValue Then Return Invalido("La remuneración es obligatoria.")

            Dim monto As Decimal
            Dim errorMonto = ValidarMonto(input.Remuneracion.Value, monto)
            If errorMonto IsNot Nothing Then Return Invalido(errorMonto)

            Dim entidad = repo.ObtenerPorId(id)
            If entidad Is Nothing Then
                Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.NoEncontrado}
            End If

            ' Usuario, año y mes no se modifican: son la identidad de la fila.
            entidad.Remuneracion = monto

            Try
                repo.Actualizar(entidad)
            Catch
                Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.Fallo, .Mensaje = "No se pudo actualizar la remuneración."}
            End Try

            Return New ResultadoRemuneracion With {
                .Estado = ResultadoOperacionRemuneracion.Ok,
                .Remuneracion = MapearDTO(entidad, LegajoDePerfil(entidad.Usuario))
            }
        End Function

        Public Function Eliminar(id As Integer) As ResultadoRemuneracion Implements IRemuneracionService.Eliminar
            Dim entidad = repo.ObtenerPorId(id)
            If entidad Is Nothing Then
                Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.NoEncontrado}
            End If

            Try
                repo.Eliminar(entidad)
            Catch
                Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.Fallo, .Mensaje = "No se pudo eliminar la remuneración."}
            End Try

            Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.Ok}
        End Function

        ' ---------------------------------------------------------------- Excel: plantilla y exportación

        ' Usuarios activos (aprobados) con su última remuneración conocida, para el mes y año pedidos (por defecto, los actuales).
        ' Equivale al "Exportar" del sistema viejo; el archivo se puede completar y volver a subir con Importar.
        Public Function GenerarPlantilla(mes As Integer?, anio As Integer?) As ResultadoArchivoRemuneraciones Implements IRemuneracionService.GenerarPlantilla
            Dim hoy = Date.Today
            Dim mesEfectivo = mes.GetValueOrDefault(hoy.Month)
            Dim anioEfectivo = anio.GetValueOrDefault(hoy.Year)

            Dim mensaje = ValidarFiltro(mesEfectivo, anioEfectivo)
            If mensaje IsNot Nothing Then
                Return New ResultadoArchivoRemuneraciones With {.Estado = ResultadoOperacionRemuneracion.Invalido, .Mensaje = mensaje}
            End If

            Dim ultimas = UltimasRemuneraciones()

            Dim filas = usuarios.ObtenerTodos().
                Where(Function(u) u.IsApproved).
                OrderBy(Function(u) u.UserName).
                ToList().
                Select(Function(u) New RemuneracionPlantillaFilaDTO With {
                    .Anio = anioEfectivo,
                    .Mes = mesEfectivo,
                    .Usuario = u.UserName,
                    .Remuneracion = ValorODefecto(ultimas, u.UserName),
                    .Legajo = LegajoDePerfil(u.UserName)
                }).
                ToList()

            Return New ResultadoArchivoRemuneraciones With {
                .Estado = ResultadoOperacionRemuneracion.Ok,
                .Contenido = ExportadorExcel.Generar(filas, GetType(RemuneracionPlantillaFilaDTO), HojaImportacion),
                .NombreArchivo = "Plantilla_Remuneraciones_" & anioEfectivo & "-" & mesEfectivo.ToString("00") & ".xlsx"
            }
        End Function

        ' El listado (con los mismos filtros que Listar) como .xlsx.
        Public Function Exportar(mes As Integer?, anio As Integer?) As ResultadoArchivoRemuneraciones Implements IRemuneracionService.Exportar
            Dim mensaje = ValidarFiltro(mes, anio)
            If mensaje IsNot Nothing Then
                Return New ResultadoArchivoRemuneraciones With {.Estado = ResultadoOperacionRemuneracion.Invalido, .Mensaje = mensaje}
            End If

            Dim nombre = "Remuneraciones" &
                         If(anio.HasValue, "_" & anio.Value, "") &
                         If(mes.HasValue, "_" & mes.Value.ToString("00"), "") & ".xlsx"

            Return New ResultadoArchivoRemuneraciones With {
                .Estado = ResultadoOperacionRemuneracion.Ok,
                .Contenido = ExportadorExcel.Generar(ListarDTOs(mes, anio), GetType(RemuneracionDTO), HojaImportacion),
                .NombreArchivo = nombre
            }
        End Function

        ' ---------------------------------------------------------------- Excel: importación

        Private Class FilaImportacion
            Public Fila As Integer
            Public Usuario As String
            Public Anio As Integer
            Public Mes As Integer
            Public Monto As Decimal
        End Class

        ' Hoja "Remuneraciones" con las columnas Anio, Mes, Usuario, Remuneracion (Legajo se ignora). Upsert por
        ' usuario + año + mes. Primero se lee y valida TODO el archivo; si hay un solo error no se escribe nada.
        ' Sin errores, todos los cambios se persisten en un único SaveChanges (una transacción).
        Public Function Importar(archivo As Byte()) As ResultadoImportacionRemuneraciones Implements IRemuneracionService.Importar
            Dim resumen As New ImportacionRemuneracionesDTO()

            If archivo Is Nothing OrElse archivo.Length = 0 Then
                Return ConErrores(resumen, 0, "El archivo está vacío.")
            End If
            If archivo.Length > MaximoBytesImportacion Then
                Return ConErrores(resumen, 0, "El archivo supera el tamaño máximo de " & (MaximoBytesImportacion \ 1024 \ 1024) & " MB.")
            End If

            Dim validas As New List(Of FilaImportacion)

            Try
                LeerArchivo(archivo, resumen, validas)
            Catch
                resumen.Errores.Clear()
                Return ConErrores(resumen, 0, "No se pudo leer el archivo. Verificá que sea un .xlsx válido.")
            End Try

            If resumen.Errores.Count > 0 Then
                Return New ResultadoImportacionRemuneraciones With {
                    .Estado = ResultadoOperacionRemuneracion.Invalido,
                    .Resumen = resumen,
                    .Mensaje = "El archivo tiene errores. No se importó ninguna remuneración."
                }
            End If

            Try
                Aplicar(validas, resumen)
            Catch
                resumen.Creados = 0
                resumen.Actualizados = 0
                Return New ResultadoImportacionRemuneraciones With {
                    .Estado = ResultadoOperacionRemuneracion.Fallo,
                    .Resumen = resumen,
                    .Mensaje = "No se pudo guardar la importación. No se modificó ninguna remuneración."
                }
            End Try

            Return New ResultadoImportacionRemuneraciones With {.Estado = ResultadoOperacionRemuneracion.Ok, .Resumen = resumen}
        End Function

        ' Lee y valida el archivo. Los errores van a resumen.Errores y las filas correctas a validas.
        Private Sub LeerArchivo(archivo As Byte(), resumen As ImportacionRemuneracionesDTO, validas As List(Of FilaImportacion))
            Using ms As New MemoryStream(archivo)
                Using wb As New XLWorkbook(ms)
                    Dim hoja = wb.Worksheets.FirstOrDefault(Function(w) w.Name.Equals(HojaImportacion, StringComparison.OrdinalIgnoreCase))
                    If hoja Is Nothing Then
                        AgregarError(resumen, 0, "No se encontró la hoja '" & HojaImportacion & "'.")
                        Return
                    End If

                    Dim ultimaFila = hoja.LastRowUsed()
                    Dim ultimaColumna = hoja.LastColumnUsed()
                    If ultimaFila Is Nothing OrElse ultimaColumna Is Nothing Then
                        AgregarError(resumen, 0, "La hoja '" & HojaImportacion & "' está vacía.")
                        Return
                    End If

                    ' Columnas por nombre de encabezado (fila 1), sin importar el orden ni las mayúsculas.
                    Dim columnas As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
                    For c = 1 To ultimaColumna.ColumnNumber()
                        Dim nombre = hoja.Cell(1, c).GetString().Trim()
                        If nombre.Length > 0 AndAlso Not columnas.ContainsKey(nombre) Then columnas(nombre) = c
                    Next

                    Dim colAnio As Integer, colMes As Integer, colUsuario As Integer, colMonto As Integer
                    If Not (columnas.TryGetValue("Anio", colAnio) OrElse columnas.TryGetValue("Año", colAnio)) Then AgregarError(resumen, 1, "Falta la columna 'Anio'.")
                    If Not columnas.TryGetValue("Mes", colMes) Then AgregarError(resumen, 1, "Falta la columna 'Mes'.")
                    If Not columnas.TryGetValue("Usuario", colUsuario) Then AgregarError(resumen, 1, "Falta la columna 'Usuario'.")
                    If Not columnas.TryGetValue("Remuneracion", colMonto) Then AgregarError(resumen, 1, "Falta la columna 'Remuneracion'.")
                    If resumen.Errores.Count > 0 Then Return

                    If ultimaFila.RowNumber() - 1 > MaximoFilasImportacion Then
                        AgregarError(resumen, 0, "El archivo tiene más de " & MaximoFilasImportacion & " filas.")
                        Return
                    End If

                    Dim cacheUsuarios As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                    Dim filasPorClave As New Dictionary(Of String, Integer)

                    For fila = 2 To ultimaFila.RowNumber()
                        Dim cAnio = hoja.Cell(fila, colAnio)
                        Dim cMes = hoja.Cell(fila, colMes)
                        Dim cUsuario = hoja.Cell(fila, colUsuario)
                        Dim cMonto = hoja.Cell(fila, colMonto)

                        If EsVacia(cAnio) AndAlso EsVacia(cMes) AndAlso EsVacia(cUsuario) AndAlso EsVacia(cMonto) Then Continue For

                        ' Remuneración vacía o 0: la fila se ignora (no se valida el resto).
                        Dim monto As Decimal = 0
                        If Not EsVacia(cMonto) Then
                            If Not TryLeerDecimal(cMonto, monto) Then
                                AgregarError(resumen, fila, "La remuneración no es un número válido.")
                                Continue For
                            End If
                            If monto < 0 Then
                                AgregarError(resumen, fila, "La remuneración no puede ser negativa.")
                                Continue For
                            End If
                        End If

                        Dim redondeado As Decimal = Math.Round(monto, 2, MidpointRounding.AwayFromZero)
                        If redondeado = 0 Then
                            resumen.Omitidos += 1
                            Continue For
                        End If

                        Dim erroresFila = 0
                        Dim errorMonto = ValidarMonto(monto, redondeado)
                        If errorMonto IsNot Nothing Then
                            AgregarError(resumen, fila, errorMonto)
                            erroresFila += 1
                        End If

                        Dim anio As Integer, mes As Integer
                        If Not TryLeerEntero(cAnio, anio) OrElse anio < AnioMinimo OrElse anio > AnioMaximo Then
                            AgregarError(resumen, fila, "El año debe ser un número entre " & AnioMinimo & " y " & AnioMaximo & ".")
                            erroresFila += 1
                        End If
                        If Not TryLeerEntero(cMes, mes) OrElse mes < 1 OrElse mes > 12 Then
                            AgregarError(resumen, fila, "El mes debe ser un número entre 1 y 12.")
                            erroresFila += 1
                        End If

                        Dim nombre = hoja.Cell(fila, colUsuario).GetString().Trim()
                        Dim canonico As String = Nothing
                        If nombre.Length = 0 Then
                            AgregarError(resumen, fila, "El usuario es obligatorio.")
                            erroresFila += 1
                        Else
                            If Not cacheUsuarios.TryGetValue(nombre, canonico) Then
                                Dim u = usuarios.ObtenerPorUserName(nombre)
                                canonico = If(u Is Nothing, Nothing, u.UserName)
                                cacheUsuarios(nombre) = canonico
                            End If
                            If canonico Is Nothing Then
                                AgregarError(resumen, fila, "El usuario '" & nombre & "' no existe.")
                                erroresFila += 1
                            End If
                        End If

                        If erroresFila > 0 Then Continue For

                        Dim clave = ClaveDe(canonico, anio, mes)
                        Dim filaPrevia As Integer
                        If filasPorClave.TryGetValue(clave, filaPrevia) Then
                            AgregarError(resumen, fila, "Repite el usuario, año y mes de la fila " & filaPrevia & ".")
                            Continue For
                        End If
                        filasPorClave(clave) = fila

                        validas.Add(New FilaImportacion With {.Fila = fila, .Usuario = canonico, .Anio = anio, .Mes = mes, .Monto = redondeado})
                    Next
                End Using
            End Using
        End Sub

        ' Upsert de las filas ya validadas. Todo se acumula en el contexto y se guarda de una vez.
        Private Sub Aplicar(validas As List(Of FilaImportacion), resumen As ImportacionRemuneracionesDTO)
            If validas.Count = 0 Then Return

            Dim anios = validas.Select(Function(f) f.Anio).Distinct().ToList()

            ' Con el índice UX_REMUNERACIONES_Usuario_Anio_Mes (App_Data/Scripts/REMUNERACIONES_UNIQUE.sql) no puede haber
            ' repetidas. Mientras ese script no se haya corrido en el ambiente, se actualiza la de IDRem más alto.
            Dim existentes = repo.ObtenerPorAnios(anios).ToList().
                GroupBy(Function(r) ClaveDe(r.Usuario, r.Anio, r.Mes)).
                ToDictionary(Function(g) g.Key, Function(g) g.OrderByDescending(Function(r) r.IDRem).First())

            Dim creados = 0
            Dim actualizados = 0

            For Each f In validas
                Dim existente As REMUNERACIONES = Nothing

                If existentes.TryGetValue(ClaveDe(f.Usuario, f.Anio, f.Mes), existente) Then
                    existente.Remuneracion = f.Monto
                    actualizados += 1
                Else
                    repo.AgregarSinGuardar(New REMUNERACIONES With {
                        .Usuario = f.Usuario,
                        .Anio = CShort(f.Anio),
                        .Mes = CShort(f.Mes),
                        .Remuneracion = f.Monto
                    })
                    creados += 1
                End If
            Next

            repo.Guardar()

            resumen.Creados = creados
            resumen.Actualizados = actualizados
        End Sub

        ' ---------------------------------------------------------------- Helpers

        Private Function ListarDTOs(mes As Integer?, anio As Integer?) As List(Of RemuneracionDTO)
            Dim consulta = repo.ListarRemuneraciones()

            If anio.HasValue Then
                Dim a = CShort(anio.Value)
                consulta = consulta.Where(Function(r) r.Anio = a)
            End If
            If mes.HasValue Then
                Dim m = CShort(mes.Value)
                consulta = consulta.Where(Function(r) r.Mes = m)
            End If

            Dim filas = consulta.
                OrderByDescending(Function(r) r.Anio).
                ThenByDescending(Function(r) r.Mes).
                ThenBy(Function(r) r.Usuario).
                ToList()

            ' Un Profile por usuario único, no por fila.
            Dim legajos As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each u In filas.Select(Function(r) r.Usuario).Where(Function(x) Not String.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase)
                legajos(u) = LegajoDePerfil(u)
            Next

            Return filas.Select(Function(r) MapearDTO(r, ValorODefecto(legajos, r.Usuario))).ToList()
        End Function

        ' Última remuneración conocida (año y mes más recientes) de cada usuario.
        Private Function UltimasRemuneraciones() As Dictionary(Of String, Decimal)
            Dim todas = repo.ListarRemuneraciones().
                Select(Function(r) New With {.Usuario = r.Usuario, .Anio = r.Anio, .Mes = r.Mes, .Remuneracion = r.Remuneracion}).
                ToList()

            Dim ultimas As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
            For Each grupo In todas.GroupBy(Function(x) x.Usuario, StringComparer.OrdinalIgnoreCase)
                ultimas(grupo.Key) = grupo.OrderByDescending(Function(x) x.Anio).ThenByDescending(Function(x) x.Mes).First().Remuneracion
            Next
            Return ultimas
        End Function

        Private Function LegajoDePerfil(usuario As String) As String
            Try
                Return usuarios.ObtenerPerfil(usuario).Legajo
            Catch
                Return Nothing
            End Try
        End Function

        Private Shared Function ValorODefecto(Of T)(diccionario As Dictionary(Of String, T), clave As String) As T
            Dim valor As T = Nothing
            If clave IsNot Nothing Then diccionario.TryGetValue(clave, valor)
            Return valor
        End Function

        ' Mensaje de error o Nothing. Mes y año son opcionales; si vienen, deben estar en rango.
        Private Shared Function ValidarFiltro(mes As Integer?, anio As Integer?) As String
            If mes.HasValue AndAlso (mes.Value < 1 OrElse mes.Value > 12) Then Return "El mes debe estar entre 1 y 12."
            If anio.HasValue AndAlso (anio.Value < AnioMinimo OrElse anio.Value > AnioMaximo) Then
                Return "El año debe estar entre " & AnioMinimo & " y " & AnioMaximo & "."
            End If
            Return Nothing
        End Function

        ' No negativo y dentro de decimal(18,2). Los decimales de más se redondean a 2 (hacia arriba en el .5).
        Private Shared Function ValidarMonto(valor As Decimal, ByRef redondeado As Decimal) As String
            If valor < 0 Then Return "La remuneración no puede ser negativa."

            redondeado = Math.Round(valor, 2, MidpointRounding.AwayFromZero)
            If redondeado > MontoMaximo Then Return "La remuneración es demasiado grande: el máximo es 9999999999999999,99."

            Return Nothing
        End Function

        Private Shared Function ClaveDe(usuario As String, anio As Integer, mes As Integer) As String
            Return usuario.ToUpperInvariant() & "|" & anio & "|" & mes
        End Function

        Private Shared Sub AgregarError(resumen As ImportacionRemuneracionesDTO, fila As Integer, mensaje As String)
            resumen.Errores.Add(New ErrorImportacionRemuneracionDTO With {.Fila = fila, .Mensaje = mensaje})
        End Sub

        Private Shared Function ConErrores(resumen As ImportacionRemuneracionesDTO, fila As Integer, mensaje As String) As ResultadoImportacionRemuneraciones
            AgregarError(resumen, fila, mensaje)
            Return New ResultadoImportacionRemuneraciones With {
                .Estado = ResultadoOperacionRemuneracion.Invalido,
                .Resumen = resumen,
                .Mensaje = mensaje
            }
        End Function

        Private Shared Function EsVacia(celda As IXLCell) As Boolean
            Dim valor = celda.Value
            Return valor Is Nothing OrElse String.IsNullOrWhiteSpace(Convert.ToString(valor, CultureInfo.InvariantCulture))
        End Function

        Private Shared Function TryLeerEntero(celda As IXLCell, ByRef valor As Integer) As Boolean
            Dim numero As Decimal
            If Not TryLeerDecimal(celda, numero) Then Return False
            If numero <> Math.Floor(numero) OrElse numero < Integer.MinValue OrElse numero > Integer.MaxValue Then Return False

            valor = CInt(numero)
            Return True
        End Function

        ' Número de una celda: las numéricas se leen directo. Un texto con coma se interpreta como es-AR
        ' ("35844,67"); sin coma, como invariante ("35844.67"), para no confundir el punto decimal con el de miles.
        Private Shared Function TryLeerDecimal(celda As IXLCell, ByRef valor As Decimal) As Boolean
            If celda.DataType = XLCellValues.Number Then
                Dim d = celda.GetDouble()
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) OrElse Math.Abs(d) > 1.0E+20 Then Return False
                valor = CDec(d)
                Return True
            End If

            Dim texto = celda.GetString().Trim()
            Dim cultura = If(texto.Contains(","), CulturaAr, CultureInfo.InvariantCulture)
            Return Decimal.TryParse(texto, NumberStyles.Number, cultura, valor)
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoRemuneracion
            Return New ResultadoRemuneracion With {.Estado = ResultadoOperacionRemuneracion.Invalido, .Mensaje = mensaje}
        End Function

        Private Shared Function MapearDTO(r As REMUNERACIONES, legajo As String) As RemuneracionDTO
            Return New RemuneracionDTO With {
                .IdRem = r.IDRem,
                .Anio = r.Anio,
                .Mes = r.Mes,
                .Usuario = r.Usuario,
                .Legajo = legajo,
                .Remuneracion = r.Remuneracion
            }
        End Function
    End Class
End Namespace
