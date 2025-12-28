// Gestión de carga de archivos múltiples con vista previa

(function() {
    'use strict';

    const fileInput = document.getElementById('fileInput');
    const uploadForm = document.getElementById('uploadForm');
    const uploadBtn = document.getElementById('uploadBtn');
    const clearBtn = document.getElementById('clearBtn');
    const previewCard = document.getElementById('previewCard');
    const previewContainer = document.getElementById('previewContainer');
    const progressCard = document.getElementById('progressCard');
    const progressBar = document.getElementById('progressBar');
    const uploadStatus = document.getElementById('uploadStatus');
    const resultsCard = document.getElementById('resultsCard');
    const resultsContainer = document.getElementById('resultsContainer');

    let selectedFiles = [];

    // Actualizar label del input file
    fileInput.addEventListener('change', function(e) {
        const files = Array.from(e.target.files);
        if (files.length > 0) {
            const label = fileInput.nextElementSibling;
            label.textContent = `${files.length} archivo(s) seleccionado(s)`;
            handleFileSelection(files);
        }
    });

    // Manejar selección de archivos
    function handleFileSelection(files) {
        selectedFiles = files;
        uploadBtn.disabled = false;
        clearBtn.style.display = 'inline-block';
        
        // Validar archivos
        const validationResults = validateFiles(files);
        
        if (validationResults.errors.length > 0) {
            showValidationErrors(validationResults.errors);
            uploadBtn.disabled = true;
            return;
        }

        // Mostrar vista previa
        showPreview(files);
    }

    // Validar archivos
    function validateFiles(files) {
        const errors = [];
        const maxSize = 100 * 1024 * 1024; // 100 MB
        const allowedTypes = [
            'image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp',
            'video/mp4', 'video/mpeg', 'video/quicktime', 'video/x-msvideo', 'video/webm'
        ];
        const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.mp4', '.mpeg', '.mov', '.avi', '.webm'];

        files.forEach(file => {
            // Validar tamaño
            if (file.size === 0) {
                errors.push(`${file.name}: El archivo está vacío.`);
            }
            if (file.size > maxSize) {
                errors.push(`${file.name}: Excede el tamaño máximo de 100 MB.`);
            }

            // Validar tipo MIME
            if (!allowedTypes.includes(file.type.toLowerCase())) {
                errors.push(`${file.name}: Tipo de archivo no permitido.`);
            }

            // Validar extensión
            const extension = '.' + file.name.split('.').pop().toLowerCase();
            if (!allowedExtensions.includes(extension)) {
                errors.push(`${file.name}: Extensión no permitida.`);
            }
        });

        return {
            isValid: errors.length === 0,
            errors: errors
        };
    }

    // Mostrar errores de validación
    function showValidationErrors(errors) {
        const errorHtml = '<div class="alert alert-danger"><ul class="mb-0">' +
            errors.map(e => `<li>${e}</li>`).join('') +
            '</ul></div>';
        previewContainer.innerHTML = errorHtml;
        previewCard.style.display = 'block';
    }

    // Mostrar vista previa
    function showPreview(files) {
        previewContainer.innerHTML = '';
        
        files.forEach((file, index) => {
            const col = document.createElement('div');
            col.className = 'col-md-3 col-sm-4 col-6';
            
            const preview = document.createElement('div');
            preview.className = 'file-preview';
            preview.dataset.index = index;

            if (file.type.startsWith('image/')) {
                const img = document.createElement('img');
                img.src = URL.createObjectURL(file);
                img.alt = file.name;
                preview.appendChild(img);
            } else if (file.type.startsWith('video/')) {
                const video = document.createElement('video');
                video.src = URL.createObjectURL(file);
                video.controls = true;
                video.style.maxHeight = '200px';
                preview.appendChild(video);
            }

            // Información del archivo
            const info = document.createElement('div');
            info.className = 'file-info';
            info.innerHTML = `
                <strong>${file.name}</strong><br>
                <small>${formatFileSize(file.size)}</small>
            `;
            preview.appendChild(info);

            // Botón para eliminar
            const removeBtn = document.createElement('button');
            removeBtn.className = 'remove-file';
            removeBtn.innerHTML = '×';
            removeBtn.onclick = () => removeFile(index);
            preview.appendChild(removeBtn);

            col.appendChild(preview);
            previewContainer.appendChild(col);
        });

        previewCard.style.display = 'block';
    }

    // Eliminar archivo de la selección
    function removeFile(index) {
        selectedFiles.splice(index, 1);
        
        // Actualizar input file
        const dataTransfer = new DataTransfer();
        selectedFiles.forEach(file => dataTransfer.items.add(file));
        fileInput.files = dataTransfer.files;
        
        // Actualizar vista previa
        if (selectedFiles.length === 0) {
            previewCard.style.display = 'none';
            uploadBtn.disabled = true;
            clearBtn.style.display = 'none';
            fileInput.nextElementSibling.textContent = 'Elegir archivos...';
        } else {
            showPreview(selectedFiles);
        }
    }

    // Limpiar selección
    clearBtn.addEventListener('click', function() {
        selectedFiles = [];
        fileInput.value = '';
        fileInput.nextElementSibling.textContent = 'Elegir archivos...';
        previewCard.style.display = 'none';
        uploadBtn.disabled = true;
        clearBtn.style.display = 'none';
        resultsCard.style.display = 'none';
    });

    // Enviar formulario
    uploadForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        if (selectedFiles.length === 0) {
            alert('Por favor, seleccione al menos un archivo.');
            return;
        }

        // Ocultar resultados anteriores
        resultsCard.style.display = 'none';

        // Mostrar progreso
        progressCard.style.display = 'block';
        progressBar.style.width = '0%';
        progressBar.textContent = '0%';
        uploadStatus.innerHTML = 'Preparando archivos...';
        uploadBtn.disabled = true;

        try {
            const formData = new FormData();
            selectedFiles.forEach(file => {
                formData.append('files', file);
            });

            const description = document.getElementById('description').value;
            const tags = document.getElementById('tags').value;
            const campaignId = document.getElementById('campaignId').value;

            if (description) formData.append('description', description);
            if (tags) formData.append('tags', tags);
            if (campaignId) formData.append('campaignId', campaignId);

            // Simular progreso
            let progress = 0;
            const progressInterval = setInterval(() => {
                progress += 10;
                if (progress <= 90) {
                    progressBar.style.width = progress + '%';
                    progressBar.textContent = progress + '%';
                }
            }, 200);

            const response = await fetch('/Content/UploadFiles', {
                method: 'POST',
                body: formData
            });

            clearInterval(progressInterval);
            progressBar.style.width = '100%';
            progressBar.textContent = '100%';

            if (!response.ok) {
                throw new Error('Error al cargar los archivos.');
            }

            const result = await response.json();
            showResults(result);

            // Limpiar formulario después de éxito
            setTimeout(() => {
                clearBtn.click();
            }, 3000);

        } catch (error) {
            console.error('Error:', error);
            uploadStatus.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
        } finally {
            uploadBtn.disabled = false;
            setTimeout(() => {
                progressCard.style.display = 'none';
            }, 2000);
        }
    });

    // Mostrar resultados
    function showResults(result) {
        resultsContainer.innerHTML = '';

        if (result.successfulUploads > 0) {
            const successHtml = `
                <div class="alert alert-success">
                    <h5><i class="fas fa-check-circle"></i> Carga Exitosa</h5>
                    <p>Se cargaron correctamente ${result.successfulUploads} de ${result.totalFiles} archivo(s).</p>
                </div>
            `;
            resultsContainer.innerHTML += successHtml;
        }

        if (result.failedUploads > 0) {
            const errorsHtml = `
                <div class="alert alert-warning">
                    <h5><i class="fas fa-exclamation-triangle"></i> Errores en la Carga</h5>
                    <ul>
                        ${result.errors.map(e => `<li><strong>${e.fileName}:</strong> ${e.errorMessage}</li>`).join('')}
                    </ul>
                </div>
            `;
            resultsContainer.innerHTML += errorsHtml;
        }

        resultsCard.style.display = 'block';
    }

    // Formatear tamaño de archivo
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }
})();

