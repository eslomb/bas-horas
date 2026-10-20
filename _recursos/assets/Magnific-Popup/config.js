// script que se usa para cargar popup

// popup
$(function () {
    $.extend(true, $.magnificPopup.defaults, {
        tClose: 'Cerrar (Esc)', // Alt text on close button
        tLoading: 'Cargando...', // Text that is displayed during loading. Can contain %curr% and %total% keys
        gallery: {
            tPrev: 'Anterior', // Alt text on left arrow
            tNext: 'Siguiente', // Alt text on right arrow
            tCounter: '%curr% de %total%' // Markup for "1 of 7" counter
        },
        image: {
            tError: '<a href="%url%">La imagen</a> no se pudo encontrar.' // Error message when image could not be loaded
        },
        ajax: {
            tError: '<a href="%url%">El contenido</a> no pudo ser cargado.' // Error message when ajax request failed
        },
        closeMarkup: '<button title="%title%" type="button" class="mfp-close"><i class="fa fa-times"></i></button>'
    });

    $(".popup").magnificPopup({
        type: "iframe"
    });

    $(".popup-image").magnificPopup({
        type: "image"
    });


})