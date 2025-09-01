document.addEventListener('DOMContentLoaded', function() {
    // Dosya yükleme önizleme işlevselliği
    const profilePictureInput = document.getElementById('profilePictureInput');
    const coverPhotoInput = document.getElementById('coverPhotoInput');
    const profilePicturePreview = document.getElementById('profilePicturePreview');
    const coverPhotoPreview = document.getElementById('coverPhotoPreview');

    // Profil resmi önizleme
    if (profilePictureInput) {
        profilePictureInput.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                if (validateImageFile(file)) {
                    showImagePreview(file, profilePicturePreview);
                } else {
                    showNotification('Lütfen geçerli bir resim dosyası seçin (JPG, PNG, GIF)', 'error');
                    this.value = '';
                }
            }
        });
    }

    // Kapak fotoğrafı önizleme
    if (coverPhotoInput) {
        coverPhotoInput.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                if (validateImageFile(file)) {
                    showImagePreview(file, coverPhotoPreview);
                } else {
                    showNotification('Lütfen geçerli bir resim dosyası seçin (JPG, PNG, GIF)', 'error');
                    this.value = '';
                }
            }
        });
    }

    // Resim dosyası doğrulama
    function validateImageFile(file) {
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        const maxSize = 5 * 1024 * 1024; // 5MB
        
        if (!allowedTypes.includes(file.type)) {
            return false;
        }
        
        if (file.size > maxSize) {
            showNotification('Dosya boyutu 5MB\'dan küçük olmalıdır', 'error');
            return false;
        }
        
        return true;
    }

    // Resim önizleme gösterme
    function showImagePreview(file, previewElement) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            previewElement.innerHTML = `<img src="${e.target.result}" alt="Önizleme" />`;
            previewElement.style.border = '2px solid #86641f';
        };
        
        reader.readAsDataURL(file);
    }

    // Form validasyonu
    const editForm = document.querySelector('.edit-form');
    if (editForm) {
        editForm.addEventListener('submit', function(e) {
            if (!validateForm()) {
                e.preventDefault();
                showNotification('Lütfen tüm gerekli alanları doldurun', 'error');
            }
        });
    }

    function validateForm() {
        const requiredFields = editForm.querySelectorAll('[required]');
        let isValid = true;
        
        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                field.style.borderColor = '#dc3545';
                isValid = false;
            } else {
                field.style.borderColor = '#e9ecef';
            }
        });
        
        return isValid;
    }

    // Form alanları için real-time validasyon
    const formControls = document.querySelectorAll('.form-control');
    formControls.forEach(control => {
        control.addEventListener('blur', function() {
            validateField(this);
        });
        
        control.addEventListener('input', function() {
            if (this.style.borderColor === '#dc3545') {
                validateField(this);
            }
        });
    });

    function validateField(field) {
        const value = field.value.trim();
        const isRequired = field.hasAttribute('required');
        
        if (isRequired && !value) {
            field.style.borderColor = '#dc3545';
            showFieldError(field, 'Bu alan zorunludur');
        } else if (field.type === 'email' && value && !isValidEmail(value)) {
            field.style.borderColor = '#dc3545';
            showFieldError(field, 'Geçerli bir e-posta adresi giriniz');
        } else if (field.type === 'tel' && value && !isValidPhone(value)) {
            field.style.borderColor = '#dc3545';
            showFieldError(field, 'Geçerli bir telefon numarası giriniz');
        } else {
            field.style.borderColor = '#e9ecef';
            hideFieldError(field);
        }
    }

    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function isValidPhone(phone) {
        const phoneRegex = /^[\+]?[0-9\s\-\(\)]{10,}$/;
        return phoneRegex.test(phone);
    }

    function showFieldError(field, message) {
        let errorElement = field.parentNode.querySelector('.field-error');
        if (!errorElement) {
            errorElement = document.createElement('div');
            errorElement.className = 'field-error';
            errorElement.style.cssText = `
                color: #dc3545;
                font-size: 0.85rem;
                margin-top: 5px;
                display: block;
            `;
            field.parentNode.appendChild(errorElement);
        }
        errorElement.textContent = message;
    }

    function hideFieldError(field) {
        const errorElement = field.parentNode.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    // Karakter sayacı (biyografi için)
    const bioTextarea = document.querySelector('textarea[name="Bio"]');
    if (bioTextarea) {
        const charCounter = document.createElement('div');
        charCounter.className = 'char-counter';
        charCounter.style.cssText = `
            text-align: right;
            font-size: 0.8rem;
            color: #666;
            margin-top: 5px;
        `;
        bioTextarea.parentNode.appendChild(charCounter);
        
        function updateCharCounter() {
            const currentLength = bioTextarea.value.length;
            const maxLength = 500;
            charCounter.textContent = `${currentLength}/${maxLength}`;
            
            if (currentLength > maxLength * 0.9) {
                charCounter.style.color = '#dc3545';
            } else if (currentLength > maxLength * 0.7) {
                charCounter.style.color = '#ffc107';
            } else {
                charCounter.style.color = '#666';
            }
        }
        
        bioTextarea.addEventListener('input', updateCharCounter);
        updateCharCounter(); // İlk yükleme için
    }

    // Hedef kitap sayısı için range slider
    const targetBookInput = document.querySelector('input[name="TargetBookCount"]');
    if (targetBookInput) {
        const rangeSlider = document.createElement('input');
        rangeSlider.type = 'range';
        rangeSlider.min = '0';
        rangeSlider.max = '100';
        rangeSlider.value = targetBookInput.value || '0';
        rangeSlider.style.cssText = `
            width: 100%;
            margin-top: 10px;
            accent-color: #86641f;
        `;
        
        targetBookInput.parentNode.appendChild(rangeSlider);
        
        // Range slider ve number input senkronizasyonu
        rangeSlider.addEventListener('input', function() {
            targetBookInput.value = this.value;
        });
        
        targetBookInput.addEventListener('input', function() {
            rangeSlider.value = this.value;
        });
    }

    // Dosya yükleme alanları için drag & drop
    const fileUploadLabels = document.querySelectorAll('.file-upload-label');
    
    fileUploadLabels.forEach(label => {
        label.addEventListener('dragover', function(e) {
            e.preventDefault();
            this.style.borderColor = '#86641f';
            this.style.backgroundColor = 'rgba(134, 100, 31, 0.1)';
        });
        
        label.addEventListener('dragleave', function(e) {
            e.preventDefault();
            this.style.borderColor = '#d1d5db';
            this.style.backgroundColor = '#f9fafb';
        });
        
        label.addEventListener('drop', function(e) {
            e.preventDefault();
            this.style.borderColor = '#d1d5db';
            this.style.backgroundColor = '#f9fafb';
            
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                const file = files[0];
                const input = this.parentNode.querySelector('.file-input');
                
                if (input) {
                    input.files = files;
                    input.dispatchEvent(new Event('change'));
                }
            }
        });
    });

    // Başarı mesajları için notification sistemi
    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
                <span>${message}</span>
            </div>
        `;
        
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background-color: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8'};
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
            z-index: 1000;
            transform: translateX(400px);
            transition: transform 0.3s ease;
            max-width: 300px;
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);
        
        setTimeout(() => {
            notification.style.transform = 'translateX(400px)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }

    // Form gönderim başarı mesajı
    const successMessage = document.querySelector('.alert-success')?.textContent;
    if (successMessage) {
        showNotification(successMessage, 'success');
    }

    // Responsive tasarım için ek işlevler
    function handleResponsiveLayout() {
        const isMobile = window.innerWidth <= 768;
        const formRow = document.querySelector('.form-row');
        const imagePreviewContainer = document.querySelector('.image-preview-container');
        
        if (isMobile) {
            if (formRow) formRow.style.gridTemplateColumns = '1fr';
            if (imagePreviewContainer) imagePreviewContainer.style.gridTemplateColumns = '1fr';
        } else {
            if (formRow) formRow.style.gridTemplateColumns = '1fr 1fr';
            if (imagePreviewContainer) imagePreviewContainer.style.gridTemplateColumns = '1fr 1fr';
        }
    }

    handleResponsiveLayout();
    window.addEventListener('resize', handleResponsiveLayout);

    // Form alanları için otomatik kaydetme (localStorage)
    const formFields = editForm.querySelectorAll('input, textarea');
    
    formFields.forEach(field => {
        const fieldName = field.name;
        const savedValue = localStorage.getItem(`profile_edit_${fieldName}`);
        
        if (savedValue && field.type !== 'file') {
            field.value = savedValue;
        }
        
        field.addEventListener('input', function() {
            if (this.type !== 'file') {
                localStorage.setItem(`profile_edit_${fieldName}`, this.value);
            }
        });
    });

    // Form gönderildiğinde localStorage'ı temizle
    editForm.addEventListener('submit', function() {
        formFields.forEach(field => {
            const fieldName = field.name;
            localStorage.removeItem(`profile_edit_${fieldName}`);
        });
    });
});
