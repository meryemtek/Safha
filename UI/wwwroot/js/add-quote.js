// Alıntı Ekleme Sayfası JavaScript
document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('.add-quote-form');
    
    if (form) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Form verilerini topla
            const formData = new FormData(form);
            
            // Submit butonunu devre dışı bırak
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="icon">⏳</i> Ekleniyor...';
            
            // AJAX ile gönder
            fetch('/Profile/AddQuoteAjax', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .then(response => {
                console.log('Sunucu yanıtı:', response.status, response.statusText);
                if (!response.ok) {
                    throw new Error('Sunucu yanıtı başarısız: ' + response.status);
                }
                return response.json();
            })
            .then(data => {
                console.log('Alıntı ekleme yanıtı:', data);
                
                if (data.success) {
                    // Başarı mesajı göster
                    showSuccessMessage(data.message);
                    
                    // 1 saniye sonra profil sayfasına yönlendir (daha kısa süre)
                    setTimeout(() => {
                        try {
                            // Yönlendirme URL'sini kontrol et
                            const redirectUrl = data.redirectUrl || '/Profile';
                            console.log('Yönlendirme URL:', redirectUrl);
                            
                            // Sayfa yönlendirmesini güvenli şekilde yap
                            window.location.replace(redirectUrl);
                        } catch (redirectError) {
                            console.error('Yönlendirme hatası:', redirectError);
                            // Yönlendirme başarısız olursa manuel olarak yönlendir
                            window.location.href = '/Profile';
                        }
                    }, 1000);
                } else {
                    // Hata mesajı göster
                    showErrorMessage(data.message || 'Alıntı eklenirken bir hata oluştu');
                    
                    // Submit butonunu tekrar aktif et
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }
            })
            .catch(error => {
                console.error('Alıntı ekleme hatası:', error);
                showErrorMessage('Alıntı eklenirken bir hata oluştu: ' + error.message);
                
                // Submit butonunu tekrar aktif et
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            });
        });
    }
});

function showSuccessMessage(message) {
    // Mevcut mesajları temizle
    clearMessages();
    
    // Başarı mesajı oluştur
    const successDiv = document.createElement('div');
    successDiv.className = 'alert alert-success alert-dismissible fade show';
    successDiv.innerHTML = `
        <i class="icon">✅</i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Mesajı sayfanın üstüne ekle
    const container = document.querySelector('.add-quote-container');
    container.insertBefore(successDiv, container.firstChild);
}

function showErrorMessage(message) {
    // Mevcut mesajları temizle
    clearMessages();
    
    // Hata mesajı oluştur
    const errorDiv = document.createElement('div');
    errorDiv.className = 'alert alert-danger alert-dismissible fade show';
    errorDiv.innerHTML = `
        <i class="icon">❌</i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Mesajı sayfanın üstüne ekle
    const container = document.querySelector('.add-quote-container');
    container.insertBefore(errorDiv, container.firstChild);
}

function clearMessages() {
    // Mevcut alert mesajlarını temizle
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => alert.remove());
}
