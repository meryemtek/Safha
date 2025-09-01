// Alıntı Ekleme Sayfası JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Form validasyonu
    const form = document.querySelector('.add-quote-form');
    const contentTextarea = document.querySelector('#Content');
    const bookSelect = document.querySelector('#BookId');
    
    // Karakter sayacı
    if (contentTextarea) {
        const charCount = document.createElement('div');
        charCount.className = 'char-count';
        charCount.style.cssText = `
            text-align: right;
            font-size: 0.875rem;
            color: #6c757d;
            margin-top: 5px;
        `;
        contentTextarea.parentNode.appendChild(charCount);
        
        function updateCharCount() {
            const current = contentTextarea.value.length;
            const max = 1000;
            charCount.textContent = `${current}/${max}`;
            
            if (current > max * 0.9) {
                charCount.style.color = '#dc3545';
            } else if (current > max * 0.7) {
                charCount.style.color = '#ffc107';
            } else {
                charCount.style.color = '#6c757d';
            }
        }
        
        contentTextarea.addEventListener('input', updateCharCount);
        updateCharCount();
    }
    
    // Form gönderimi
    if (form) {
        form.addEventListener('submit', function(e) {
            let isValid = true;
            
            // Alıntı metni kontrolü
            if (!contentTextarea.value.trim()) {
                showFieldError(contentTextarea, 'Alıntı metni zorunludur');
                isValid = false;
            } else if (contentTextarea.value.length > 1000) {
                showFieldError(contentTextarea, 'Alıntı metni 1000 karakterden uzun olamaz');
                isValid = false;
            } else {
                clearFieldError(contentTextarea);
            }
            
            // Kitap seçimi kontrolü
            if (!bookSelect.value) {
                showFieldError(bookSelect, 'Kitap seçimi zorunludur');
                isValid = false;
            } else {
                clearFieldError(bookSelect);
            }
            
            if (!isValid) {
                e.preventDefault();
                showNotification('Lütfen form hatalarını düzeltin', 'error');
            }
        });
    }
    
    // Alan hata gösterme
    function showFieldError(field, message) {
        clearFieldError(field);
        
        field.classList.add('is-invalid');
        
        const errorDiv = document.createElement('div');
        errorDiv.className = 'text-danger';
        errorDiv.textContent = message;
        field.parentNode.appendChild(errorDiv);
    }
    
    // Alan hatasını temizleme
    function clearFieldError(field) {
        field.classList.remove('is-invalid');
        
        const existingError = field.parentNode.querySelector('.text-danger');
        if (existingError) {
            existingError.remove();
        }
    }
    
    // Bildirim gösterme
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
        }, 3000);
        
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 3300);
    }
    
    // Form alanlarına focus olunduğunda hata mesajlarını temizle
    const formFields = form.querySelectorAll('.form-control');
    formFields.forEach(field => {
        field.addEventListener('focus', function() {
            clearFieldError(this);
        });
    });
});
