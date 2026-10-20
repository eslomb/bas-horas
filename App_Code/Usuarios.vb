Imports System.Linq
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Validation
Imports DTOs.Usuarios


Namespace Aplicacion
    Public Class Usuarios

        Public Function listarUsuarios() As List(Of DatosUsuario)

            Dim listaUsuarios As New List(Of DatosUsuario)
            Dim usersCollection As MembershipUserCollection = Membership.GetAllUsers
            Dim _profile As ProfileCommon

            For Each usr As MembershipUser In usersCollection
                Dim usrname As String = usr.UserName
                _profile = ProfileBase.Create(usrname)
                listaUsuarios.Add(New DatosUsuario With {.UserName = usrname,
                                                         .Email = usr.Email,
                                                         .IsApproved = usr.IsApproved,
                                                         .Comentario = usr.Comment,
                                                         .Legajo = _profile.Legajo,
                                                         .Modalidad = _profile.Modalidad,
                                                         .PeriodoAbierto = _profile.PeriodoAbierto,
                                                         .Centro = _profile.Centro,
                                                         .CUIL = _profile.CUIL,
                                                         .Dependencia = _profile.Dependencia
                                                        })
            Next

            Return listaUsuarios
        End Function

        Public Function listarUsuarios(grupo As String) As List(Of DatosUsuario)

            Dim listaUsuarios As New List(Of DatosUsuario)
            Dim usersCollection() As String = Roles.GetUsersInRole(grupo)
            Dim _profile As ProfileCommon


            For Each usrStr As String In usersCollection
                Dim usr As MembershipUser = Membership.GetUser(usrStr)
                If usr IsNot Nothing Then
                    _profile = ProfileBase.Create(usrStr)
                    listaUsuarios.Add(New DatosUsuario With {.UserName = usrStr,
                                                         .Email = usr.Email,
                                                         .IsApproved = usr.IsApproved,
                                                         .Comentario = usr.Comment,
                                                         .Legajo = _profile.Legajo,
                                                         .Modalidad = _profile.Modalidad,
                                                         .PeriodoAbierto = _profile.PeriodoAbierto,
                                                         .Centro = _profile.Centro,
                                                         .CUIL = _profile.CUIL,
                                                         .Dependencia = _profile.Dependencia})
                End If
            Next

            Return listaUsuarios
        End Function

        Public Function listarUsuariosActivos() As List(Of DatosUsuario)
            Dim q = From u In listarUsuarios() Where u.IsApproved Order By u.UserName Select u

            Return q.ToList

        End Function

        Public Function listarUsuariosDependientes(legajo As String) As List(Of DatosUsuario)
            Return (From u In listarUsuarios() Where u.Dependencia.Contains(legajo) Select u).ToList
        End Function

        Public Function listarUsuariosSuperiores() As List(Of DatosUsuario)
            Dim qAdmin = (From u In listarUsuarios(Grupos.Administrador)).ToList
            Dim qAvanz = (From u In listarUsuarios(Grupos.Avanzado)).ToList
            For Each us In qAvanz
                qAdmin.Add(us)
            Next

            Return qAdmin
        End Function

        Public Function listarUsuariosBasicosDelCentro(centro As String) As List(Of DatosUsuario)
            Dim q = From u In listarUsuarios(Grupos.Basico) Where u.Centro = centro Order By u.UserName Select u

            Return q.ToList
        End Function

        Public Function obtenerDatos(userName As String) As DTOs.Usuarios.DatosUsuario

            Dim pc As ProfileCommon = System.Web.Profile.ProfileBase.Create(userName)

            Dim datosUsuario As New DTOs.Usuarios.DatosUsuario With {
                .Centro = pc.Centro,
                .CUIL = pc.CUIL,
                .Legajo = pc.Legajo,
                .Dependencia = pc.Dependencia,
                .Modalidad = pc.Modalidad,
                .PeriodoAbierto = pc.PeriodoAbierto
                }

            Return datosUsuario
        End Function

    End Class
End Namespace
