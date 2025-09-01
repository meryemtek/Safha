document.addEventListener('DOMContentLoaded', function() {
    // Form validasyonu
    const form = document.querySelector('.add-review-form');
    const contentTextarea = document.querySelector('#Content');
    const bookSelect = document.querySelector('#BookId');
    const ratingInputs = document.querySelectorAll('input[name="Rating"]');
    
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
            const max = 2000;
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
    
    // Rating yıldızları için hover efekti
    ratingInputs.forEach(input => {
        const label = document.querySelector(`label[for="rating-${input.value}"]`);
        
        label.addEventListener('mouseenter', function() {
            // Hover olan yıldızdan önceki tüm yıldızları doldur
            const currentRating = parseInt(input.value);
            highlightStars(currentRating);
        });
        
        label.addEventListener('mouseleave', function() {
            // Seçili rating'i göster
            const selectedRating = document.querySelector('input[name="Rating"]:checked');
            if (selectedRating) {
                highlightStars(parseInt(selectedRating.value));
            } else {
                clearStarHighlight();
            }
        });
        
        input.addEventListener('change', function() {
            // Rating değiştiğinde yıldızları güncelle
            highlightStars(parseInt(this.value));
        });
    });
    
    // Yıldızları vurgulama
    function highlightStars(rating) {
        clearStarHighlight();
        
        for (let i = 1; i <= rating; i++) {
            const starLabel = document.querySelector(`label[for="rating-${i}"]`);
            if (starLabel) {
                starLabel.style.color = '#ffc107';
            }
        }
    }
    
    // Yıldız vurgusunu temizleme
    function clearStarHighlight() {
        ratingInputs.forEach(input => {
            const label = document.querySelector(`label[for="rating-${input.value}"]`);
            if (label) {
                label.style.color = '#dee2e6';
            }
        });
    }
    
    // Form gönderimi
    if (form) {
        form.addEventListener('submit', function(e) {
            let isValid = true;
            
            // İnceleme metni kontrolü
            if (!contentTextarea.value.trim()) {
                showFieldError(contentTextarea, 'İnceleme metni zorunludur');
                isValid = false;
            } else if (contentTextarea.value.length > 2000) {
                showFieldError(contentTextarea, 'İnceleme metni 2000 karakterden uzun olamaz');
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
            
            // Rating kontrolü
            const selectedRating = document.querySelector('input[name="Rating"]:checked');
            if (!selectedRating) {
                showRatingError('Lütfen bir puan seçin');
                isValid = false;
            } else {
                clearRatingError();
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
    
    // Rating hatası gösterme
    function showRatingError(message) {
        clearRatingError();
        
        const ratingGroup = document.querySelector('.rating-input');
        const errorDiv = document.createElement('div');
        errorDiv.className = 'text-danger';
        errorDiv.textContent = message;
        ratingGroup.parentNode.appendChild(errorDiv);
    }
    
    // Rating hatasını temizleme
    function clearRatingError() {
        const ratingGroup = document.querySelector('.rating-input');
        const existingError = ratingGroup.parentNode.querySelector('.text-danger');
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
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }
    
    // Form alanlarına focus olunduğunda hata mesajlarını temizle
    const formFields = form.querySelectorAll('.form-control');
    formFields.forEach(field => {
        field.addEventListener('focus', function() {
            clearFieldError(this);
        });
    });
    
    // Rating değiştiğinde hata mesajını temizle
    ratingInputs.forEach(input => {
        input.addEventListener('change', function() {
            clearRatingError();
        });
    });
});
