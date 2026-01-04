// Site-wide JavaScript for Autonomous Marketing Platform

// ============================================
// SISTEMA DE LOADING GLOBAL
// ============================================

/**
 * Muestra un overlay de loading global
 */
function showGlobalLoading(message) {
    message = message || 'Procesando...';
    
    // Crear overlay si no existe
    if ($('#globalLoadingOverlay').length === 0) {
        $('body').append(`
            <div id="globalLoadingOverlay" class="global-loading-overlay">
                <div class="global-loading-content">
                    <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                        <span class="sr-only">Cargando...</span>
                    </div>
                    <div class="mt-3">
                        <h5>${message}</h5>
                    </div>
                </div>
            </div>
        `);
    } else {
        $('#globalLoadingOverlay .global-loading-content h5').text(message);
        $('#globalLoadingOverlay').fadeIn(200);
    }
}

/**
 * Oculta el overlay de loading global
 */
function hideGlobalLoading() {
    $('#globalLoadingOverlay').fadeOut(200, function() {
        // No eliminamos el overlay para reutilizarlo
    });
}

/**
 * Muestra loading en un botón específico
 */
function showButtonLoading($button, message) {
    if (!$button || $button.length === 0) return;
    
    message = message || '<i class="fas fa-spinner fa-spin mr-1"></i>Procesando...';
    
    $button.data('original-html', $button.html());
    $button.data('original-disabled', $button.prop('disabled'));
    $button.prop('disabled', true);
    $button.html(message);
}

/**
 * Oculta loading de un botón específico
 */
function hideButtonLoading($button) {
    if (!$button || $button.length === 0) return;
    
    var originalHtml = $button.data('original-html');
    var originalDisabled = $button.data('original-disabled');
    
    if (originalHtml) {
        $button.html(originalHtml);
    }
    if (originalDisabled !== undefined) {
        $button.prop('disabled', originalDisabled);
    } else {
        $button.prop('disabled', false);
    }
}

/**
 * Muestra loading en un formulario
 */
function showFormLoading($form, message) {
    if (!$form || $form.length === 0) return;
    
    message = message || 'Guardando...';
    
    var $submitBtn = $form.find('button[type="submit"], input[type="submit"]');
    if ($submitBtn.length > 0) {
        showButtonLoading($submitBtn, '<i class="fas fa-spinner fa-spin mr-1"></i>' + message);
    }
    
    // Deshabilitar todos los campos del formulario
    $form.find('input, select, textarea, button').prop('disabled', true);
    
    // Agregar overlay al formulario
    if ($form.find('.form-loading-overlay').length === 0) {
        $form.css('position', 'relative');
        $form.append(`
            <div class="form-loading-overlay">
                <div class="form-loading-spinner">
                    <div class="spinner-border text-primary" role="status">
                        <span class="sr-only">Cargando...</span>
                    </div>
                </div>
            </div>
        `);
    }
}

/**
 * Oculta loading de un formulario
 */
function hideFormLoading($form) {
    if (!$form || $form.length === 0) return;
    
    var $submitBtn = $form.find('button[type="submit"], input[type="submit"]');
    if ($submitBtn.length > 0) {
        hideButtonLoading($submitBtn);
    }
    
    // Habilitar todos los campos del formulario
    $form.find('input, select, textarea, button').prop('disabled', false);
    
    // Remover overlay del formulario
    $form.find('.form-loading-overlay').remove();
}

// ============================================
// INICIALIZACIÓN
// ============================================

$(document).ready(function() {
    // Inicialización general
    console.log('Autonomous Marketing Platform loaded');
    
    // ============================================
    // LOADING AUTOMÁTICO EN FORMULARIOS
    // ============================================
    
    // Aplicar loading a todos los formularios al enviar
    $('form').on('submit', function(e) {
        var $form = $(this);
        
        // Solo aplicar loading si el formulario es válido
        if ($form[0].checkValidity()) {
            showFormLoading($form, 'Guardando...');
        }
    });
    
    // ============================================
    // LOADING AUTOMÁTICO EN BOTONES DE ACCIÓN
    // ============================================
    
    // Botones con clase .btn-action
    $('.btn-action').on('click', function() {
        var $btn = $(this);
        if (!$btn.prop('disabled')) {
            showButtonLoading($btn, '<i class="fas fa-spinner fa-spin mr-1"></i>Procesando...');
        }
    });
    
    // Botones de confirmación (toggle active, delete, etc.)
    $('form[onsubmit*="confirm"]').on('submit', function(e) {
        var $form = $(this);
        var $btn = $form.find('button[type="submit"]');
        if ($btn.length > 0) {
            showButtonLoading($btn, '<i class="fas fa-spinner fa-spin mr-1"></i>Procesando...');
        }
    });
    
    // ============================================
    // LOADING AUTOMÁTICO EN LLAMADAS AJAX
    // ============================================
    
    // Interceptar todas las llamadas AJAX para mostrar loading global
    $(document).ajaxStart(function() {
        // Solo mostrar loading global si no hay un loading específico activo
        if ($('.form-loading-overlay:visible').length === 0 && 
            $('.btn[disabled]:has(.fa-spinner)').length === 0) {
            showGlobalLoading('Procesando solicitud...');
        }
    }).ajaxStop(function() {
        hideGlobalLoading();
    });
    
    // ============================================
    // ADMINLTE WIDGETS
    // ============================================
    
    // Pushmenu (botón de menú hamburguesa) - AdminLTE lo maneja automáticamente
    // Si no funciona, inicializamos manualmente
    $('[data-widget="pushmenu"]').on('click', function(e) {
        e.preventDefault();
        if ($('body').hasClass('sidebar-collapse')) {
            $('body').removeClass('sidebar-collapse');
        } else {
            $('body').addClass('sidebar-collapse');
        }
    });
    
    // Treeview (menú lateral) - AdminLTE lo maneja automáticamente con data-widget="treeview"
    
    // Card widget refresh
    $('[data-card-widget="refresh"]').on('click', function(e) {
        e.preventDefault();
        var $card = $(this).closest('.card');
        var source = $(this).data('source');
        
        // Agregar clase de loading
        $card.addClass('card-refreshing');
        
        if (source) {
            // Hacer petición AJAX para refrescar
            $.get(source)
                .done(function(data) {
                    console.log('Card refreshed', data);
                    // Aquí podrías actualizar el contenido de la card con los nuevos datos
                })
                .fail(function() {
                    console.error('Error refreshing card');
                })
                .always(function() {
                    setTimeout(function() {
                        $card.removeClass('card-refreshing');
                    }, 500);
                });
        } else {
            // Si no hay source, solo simular refresh
            setTimeout(function() {
                $card.removeClass('card-refreshing');
            }, 500);
        }
    });
    
    // Tooltips de Bootstrap
    $('[data-toggle="tooltip"]').tooltip();
    
    // Popovers de Bootstrap
    $('[data-toggle="popover"]').popover();
    
    // Dropdowns de Bootstrap (asegurar que funcionen)
    $('.dropdown-toggle').dropdown();
    
    // Inicializar DataTables si existen tablas
    if ($.fn.DataTable) {
        $('.data-table').DataTable({
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json"
            },
            "responsive": true,
            "pageLength": 25
        });
    }
    
    console.log('AdminLTE widgets initialized');
    console.log('Loading system initialized');
});

