
$(function () {
    $('.btn_obs').click(function () {
        $(this).parent().parent().find(".observaciones").toggle();
    });

    $(".input-actividad, .input-producto").focus(function () {
        
        // seleccionar todo el texto del input para obligar a escribir la palabra a buscar en el autocompletar
        /// esto es porque en la actividad se devuelve el nombre y codigo que estan en campos separados de la tabla
        $(this).one('mouseup', function(event){
            event.preventDefault();
        }).select();
    });

    $('.input-actividad').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "WebService.asmx/Actividades",
                data: "{ 'pre':'" + request.term + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return ({
                            label: item.split('|')[0],
                            val: item.split('|')[1]
                        })
                    }))
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    console.log(textStatus);
                },
                failure: function (response) {
                    console.log(response.responseText);
                }
            });
        },
        select: function (e, i) {
            $(this).next().val(i.item.val);
        },
        open: function (event, ui) {
            //$(this).next().val("");
        }
    });

    $('.input-producto').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "WebService.asmx/Productos",
                data: "{ 'pre':'" + request.term + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    response($.map(data.d, function (item) {
                        return ({
                            label: item.split('|')[0],
                            val: item.split('|')[1]
                        })
                    }))
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    console.log(textStatus);
                },
                failure: function (response) {
                    console.log(response.responseText);
                }
            });
        },
        select: function (e, i) {
            $(this).next().val(i.item.val);
        },
        open: function (event, ui) {
            //$(this).next().val("");
        }
    });


});