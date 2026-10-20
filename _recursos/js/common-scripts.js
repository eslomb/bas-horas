
$(function () {
    
    $('a[href="#"]').click(function (evt) {
        evt.preventDefault();
    });
    
    btn_default = ".btn-submit"
    $(btn_default).attr("data-loading-text", "Un momento...")

    $(btn_default).on('click', function (evt) {
        var $btn = $(this).button('loading')
    })

    $('#validation_modal').on('hidden.bs.modal', function (e) {
        $(btn_default).button('reset')
    })

    $('.dropdown-menu').find('input').click(function (e) {
        e.stopPropagation();
    });

//    tool tips
    $('.tooltips').tooltip();

//    popovers
    $('.popovers').popover();



//chosen

    if ($.fn.chosen) {
        $(".chosen").chosen({
            no_results_text: "No hay resultados con "
        })
    }

// FooTable
    if ($.fn.footable) {
        $('.footable').footable();
    }

//Datepicker
    /**/
    if ($.fn.datepicker) {
        $.fn.datepicker.dates['es'] = {
            days: ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"],
            daysShort: ["Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab"],
            daysMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            months: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"],
            monthsShort: ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"],
            today: "Hoy",
            clear: "Limpiar"
        };
        $('.default-date-picker, .input-group.date').datepicker(datepicker_settings);
    }
// datatables init

    if ($.fn.dataTable && datatables_settings) {
        $.extend($.fn.dataTable.defaults, datatables_settings);
        $.extend($.fn.dataTableExt.oStdClasses, {
            "sFilterInput": "form-control",
            "sLengthSelect": "form-control"
        });
        $.fn.dataTable.ext.type.search.string = function (data) {
            return !data ?
			        '' :
			        typeof data === 'string' ?
			            data
			                .replace(/\n/g, ' ')
			                .replace(/[áâàä]/g, 'a')
			                .replace(/[éêèë]/g, 'e')
			                .replace(/[íîìï]/g, 'i')
			                .replace(/[óôòö]/g, 'o')
			                .replace(/[úûùü]/g, 'u')
			                .replace(/ç/g, 'c') :
			            data;
        };
        $('.dataTable').DataTable(datatables_settings);
    }
});

var datepicker_settings = {
    format: 'dd/mm/yyyy'
    , language: 'es'
    , autoclose: true
    //, daysOfWeekDisabled: "0,6"
    //, endDate: getNextDate(0)
    //, startDate: getPreviousDate(30)
}

var datatables_settings = {
    language: {
        lengthMenu: "_MENU_ registros",
        zeroRecords: "No se encontraron datos",
        emptyTable: "No hay datos para mostrar",
        info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
        infoEmpty: "Mostrando 0 a 0 de 0 registros",
        infoFiltered: "(filtrado de _MAX_ registros en total)",
        search: "Buscar: ",
        processing: "Procesando...",
        paginate: {
            first: "<<",
            last: ">>",
            next: "Siguiente",
            previous: "Anterior"
        },
        aria: {
            sortAscending: "aplicar orden ascendente",
            sortDescending: "aplicar orden descendente"
        }
    },
    drawCallback: function () {
        if (Math.ceil((this.fnSettings().fnRecordsDisplay()) / this.fnSettings()._iDisplayLength) > 1) {
            $('.dataTables_paginate').css("display", "block");
        } else {
            $('.dataTables_paginate').css("display", "none");
        }
        //$('.dataTables_length select, .dataTables_filter input').addClass("form-control input-sm");
    },
    ordering: true,
    columnDefs: [
        { sortable: false, targets: ['noSort'] }  //Desactiva ordering en las columnas marcadas del listado 
    ],
    searching: true,
    lengthMenu: [[5, 10, 25, 50, -1], [5, 10, 25, 50, "Todos"]],
    displayLength: 25,
    stateSave: true,
    responsive: true
}

