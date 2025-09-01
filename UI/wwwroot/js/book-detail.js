document.addEventListener('DOMContentLoaded', function() {
    // Tab değiştirme işlevselliği
    const tabButtons = document.querySelectorAll('.tab-button');
    const tabContents = document.querySelectorAll('.tab-content');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Aktif tab butonunu değiştir
            tabButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            
            // İlgili içeriği göster
            const tabId = this.getAttribute('data-tab');
            tabContents.forEach(content => {
                content.classList.remove('active');
                if (content.id === tabId) {
                    content.classList.add('active');
                }
            });
        });
    });
    
    // Kitap durumu kartları için animasyon ve etkileşimler
    const statusCards = document.querySelectorAll('.status-card');
    
    statusCards.forEach(card => {
        // Kart tıklama animasyonu
        card.addEventListener('mousedown', function() {
            this.style.transform = 'scale(0.98)';
        });
        
        card.addEventListener('mouseup', function() {
            if (this.classList.contains('active')) {
                this.style.transform = 'translateY(-2px)';
            } else {
                this.style.transform = '';
            }
        });
        
        card.addEventListener('mouseleave', function() {
            if (this.classList.contains('active')) {
                this.style.transform = 'translateY(-2px)';
            } else {
                this.style.transform = '';
            }
        });
        
        // Form gönderimi sırasında görsel geri bildirim
        const form = card.closest('form');
        if (form) {
            form.addEventListener('submit', function(e) {
                // Eğer zaten aktifse ve tekrar tıklanırsa, işlemi engelle
                if (card.classList.contains('active')) {
                    e.preventDefault();
                    
                    // Animasyon efekti ile zaten seçili olduğunu göster
                    card.classList.add('already-selected');
                    setTimeout(() => {
                        card.classList.remove('already-selected');
                    }, 800);
                    
                    return;
                }
                
                // Form gönderilmeden önce yükleniyor animasyonu göster
                const statusText = card.querySelector('.status-text');
                const statusIcon = card.querySelector('.status-icon');
                const originalText = statusText.innerHTML;
                const originalIcon = statusIcon.innerHTML;
                
                // Yükleniyor animasyonu
                statusText.innerHTML = 'İşleniyor...';
                statusIcon.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
                
                // Tüm kartları devre dışı bırak
                statusCards.forEach(c => {
                    c.disabled = true;
                    c.style.opacity = '0.7';
                });
                
                // AJAX ile form gönderimi
                e.preventDefault();
                
                fetch(form.action, {
                    method: 'POST',
                    body: new FormData(form),
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Bir hata oluştu');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        // Başarılı işlem
                        
                        // Tüm kartlardan active sınıfını kaldır
                        statusCards.forEach(c => {
                            c.classList.remove('active');
                            c.disabled = false;
                            c.style.opacity = '';
                        });
                        
                        // Seçilen karta active sınıfını ekle
                        card.classList.add('active');
                        
                        // Başarı animasyonu
                        statusText.innerHTML = originalText;
                        statusIcon.innerHTML = originalIcon;
                        
                        // Başarı bildirimi göster
                        showStatusNotification('success', 'Kitap durumu güncellendi!');
                        
                        // Mevcut durum bilgisini güncelle
                        updateCurrentStatus(card.getAttribute('data-status'));
                    } else {
                        // Hata durumu
                        statusText.innerHTML = originalText;
                        statusIcon.innerHTML = originalIcon;
                        
                        // Kartları normale döndür
                        statusCards.forEach(c => {
                            c.disabled = false;
                            c.style.opacity = '';
                        });
                        
                        // Hata bildirimi göster
                        showStatusNotification('error', 'İşlem sırasında bir hata oluştu.');
                    }
                })
                .catch(error => {
                    console.error('Hata:', error);
                    
                    // Hata durumunda orijinal içeriği geri yükle
                    statusText.innerHTML = originalText;
                    statusIcon.innerHTML = originalIcon;
                    
                    // Kartları normale döndür
                    statusCards.forEach(c => {
                        c.disabled = false;
                        c.style.opacity = '';
                    });
                    
                    // Hata bildirimi göster
                    showStatusNotification('error', 'İşlem sırasında bir hata oluştu.');
                });
            });
        }
    });
    
    // Bildirim gösterme fonksiyonu
    function showStatusNotification(type, message) {
        const container = document.querySelector('.add-to-library-options');
        if (!container) return;
        
        // Varsa eski bildirimi kaldır
        const existingNotification = document.querySelector('.status-notification');
        if (existingNotification) {
            existingNotification.remove();
        }
        
        // Yeni bildirim oluştur
        const notification = document.createElement('div');
        notification.className = `status-notification ${type}`;
        
        const icon = type === 'success' ? 
            '<i class="fas fa-check-circle"></i>' : 
            '<i class="fas fa-exclamation-circle"></i>';
        
        notification.innerHTML = `
            ${icon}
            <span>${message}</span>
        `;
        
        // Bildirimi sayfaya ekle
        container.insertBefore(notification, container.firstChild);
        
        // Animasyon ile göster
        setTimeout(() => {
            notification.classList.add('show');
        }, 10);
        
        // Belirli süre sonra kaldır
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                notification.remove();
            }, 300);
        }, 3000);
    }
    
    // Mevcut durum bilgisini güncelleme
    function updateCurrentStatus(status) {
        const currentStatus = document.querySelector('.current-status');
        if (!currentStatus) return;
        
        let statusText = '';
        let statusClass = '';
        
        switch(status) {
            case 'want-to-read':
                statusText = 'Mevcut Durum: Okuyacaklarım';
                statusClass = 'want-to-read';
                break;
            case 'currently-reading':
                statusText = 'Mevcut Durum: Okuyorum';
                statusClass = 'currently-reading';
                break;
            case 'read':
                statusText = 'Mevcut Durum: Okuduklarım';
                statusClass = 'read';
                break;
        }
        
        // Mevcut durum bilgisini güncelle
        currentStatus.innerHTML = `<span class="status-badge ${statusClass}">${statusText}</span>`;
        
        // Kitaplık linkini göster (eğer yoksa)
        let libraryLink = document.querySelector('.library-link');
        if (!libraryLink) {
            const container = document.querySelector('.add-to-library-options');
            libraryLink = document.createElement('div');
            libraryLink.className = 'library-link';
            libraryLink.innerHTML = `
                <a href="/Book/MyBooks" class="btn btn-outline btn-block">
                    <i class="icon">📚</i> Kitaplığımı Görüntüle
                </a>
            `;
            container.appendChild(libraryLink);
        }
    }
    
    // Alıntı bölümü için fonksiyonlar
    initializeQuotesTab();
});

// Alıntı bölümü başlatma
function initializeQuotesTab() {
    // Alıntılar tab'ı seçildiğinde alıntıları yükle
    const quotesTab = document.querySelector('[data-tab="quotes"]');
    if (quotesTab) {
        quotesTab.addEventListener('click', function() {
            // Kısa bir gecikme ile alıntıları yükle (tab değişimi tamamlandıktan sonra)
            setTimeout(() => {
                loadQuotes();
            }, 100);
        });
    }
    
    // Sayfa yüklendiğinde alıntılar tab'ı aktifse alıntıları yükle
    const activeQuotesTab = document.querySelector('.tab-button[data-tab="quotes"].active');
    if (activeQuotesTab) {
        loadQuotes();
    }
}

// Alıntıları yükle
function loadQuotes() {
    const quotesList = document.getElementById('quotesList');
    if (!quotesList) return;
    
    // Kitap ID'sini al
    const bookId = document.getElementById('quoteBookId')?.value;
    if (!bookId) {
        quotesList.innerHTML = '<div class="no-quotes"><div class="no-quotes-icon">📚</div><h4>Kitap Bulunamadı</h4><p>Bu kitap için alıntı eklenemez.</p></div>';
        return;
    }
    
    // Loading göster
    quotesList.innerHTML = '<div class="loading-quotes"><p>Alıntılar yükleniyor...</p></div>';
    
    // AJAX ile alıntıları getir
    fetch(`/Book/GetQuotes/${bookId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                displayQuotes(data.quotes);
            } else {
                quotesList.innerHTML = '<div class="no-quotes"><div class="no-quotes-icon">❌</div><h4>Hata</h4><p>Alıntılar yüklenirken bir hata oluştu.</p></div>';
            }
        })
        .catch(error => {
            console.error('Alıntılar yüklenirken hata:', error);
            quotesList.innerHTML = '<div class="no-quotes"><div class="no-quotes-icon">❌</div><h4>Hata</h4><p>Alıntılar yüklenirken bir hata oluştu.</p></div>';
        });
}

// Alıntıları görüntüle
function displayQuotes(quotes) {
    const quotesList = document.getElementById('quotesList');
    if (!quotesList) return;
    
    if (!quotes || quotes.length === 0) {
        quotesList.innerHTML = `
            <div class="no-quotes">
                <div class="no-quotes-icon">💬</div>
                <h4>Henüz Alıntı Yok</h4>
                <p>Bu kitap için henüz alıntı eklenmemiş. İlk alıntıyı siz ekleyin!</p>
            </div>
        `;
        return;
    }
    
    const quotesHTML = quotes.map(quote => createQuoteCard(quote)).join('');
    quotesList.innerHTML = quotesHTML;
    
    // Silme butonlarına event listener ekle
    document.querySelectorAll('.quote-action-btn.delete').forEach(btn => {
        btn.addEventListener('click', function() {
            const quoteId = this.dataset.quoteId;
            deleteQuote(quoteId);
        });
    });
}

// Alıntı kartı oluştur
function createQuoteCard(quote) {
    const metaItems = [];
    
    if (quote.author) {
        metaItems.push(`<div class="quote-meta-item"><span class="quote-meta-icon">✍️</span> ${quote.author}</div>`);
    }
    
    if (quote.source) {
        metaItems.push(`<div class="quote-meta-item"><span class="quote-meta-icon">📖</span> ${quote.source}</div>`);
    }
    
    if (quote.pageNumber > 0) {
        metaItems.push(`<div class="quote-meta-item"><span class="quote-meta-icon">📄</span> Sayfa ${quote.pageNumber}</div>`);
    }
    
    const metaHTML = metaItems.length > 0 ? `<div class="quote-meta">${metaItems.join('')}</div>` : '';
    
    const notesHTML = quote.notes ? `
        <div class="quote-notes">
            <h5>📝 Notlar</h5>
            <p>${quote.notes}</p>
        </div>
    ` : '';
    
    const deleteButton = quote.canDelete ? `
        <button class="quote-action-btn delete" data-quote-id="${quote.id}" title="Alıntıyı Sil">
            🗑️
        </button>
    ` : '';
    
    return `
        <div class="quote-card" data-quote-id="${quote.id}">
            <div class="quote-header">
                <div class="quote-user-info">
                    <img src="${quote.userProfilePicture}" alt="${quote.userName}" class="quote-user-avatar">
                    <div class="quote-user-details">
                        <h5 class="quote-user-name">${quote.userName}</h5>
                        <p class="quote-date">${quote.createdAt}</p>
                    </div>
                </div>
                <div class="quote-actions">
                    ${deleteButton}
                </div>
            </div>
            
            <div class="quote-content">
                <p class="quote-text">"${quote.content}"</p>
            </div>
            
            ${metaHTML}
            ${notesHTML}
        </div>
    `;
}

// Alıntı ekleme modal'ını göster
function showAddQuoteModal() {
    const modal = new bootstrap.Modal(document.getElementById('addQuoteModal'));
    modal.show();
    
    // Formu temizle
    document.getElementById('addQuoteForm').reset();
    
    // Form submit event listener ekle
    const form = document.getElementById('addQuoteForm');
    if (form) {
        form.removeEventListener('submit', handleAddQuote);
        form.addEventListener('submit', handleAddQuote);
    }
}

// Alıntı ekleme işlemi
function handleAddQuote(event) {
    event.preventDefault();
    
    const form = event.target;
    const formData = new FormData(form);
    
    // Form verilerini object'e çevir
    const quoteData = {
        Content: formData.get('Content'),
        Author: formData.get('Author') || '',
        Source: formData.get('Source') || '',
        PageNumber: parseInt(formData.get('PageNumber')) || 0,
        Notes: formData.get('Notes') || '',
        BookId: parseInt(formData.get('BookId'))
    };
    
    // Validasyon
    if (!quoteData.Content.trim()) {
        showAlert('danger', 'Alıntı metni zorunludur.');
        return;
    }
    
    if (quoteData.Content.length > 1000) {
        showAlert('danger', 'Alıntı metni en fazla 1000 karakter olabilir.');
        return;
    }
    
    // Submit butonunu devre dışı bırak
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Ekleniyor...';
    
    // AJAX ile alıntı ekle
    fetch('/Book/AddQuote', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(quoteData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert('success', data.message);
            
            // Modal'ı kapat
            const modal = bootstrap.Modal.getInstance(document.getElementById('addQuoteModal'));
            modal.hide();
            
            // Alıntıları yeniden yükle
            setTimeout(() => {
                loadQuotes();
            }, 500);
            
            // Formu temizle
            form.reset();
        } else {
            if (data.errors && data.errors.length > 0) {
                showAlert('danger', data.errors.join('<br>'));
            } else {
                showAlert('danger', data.message || 'Alıntı eklenirken bir hata oluştu.');
            }
        }
    })
    .catch(error => {
        console.error('Alıntı eklenirken hata:', error);
        showAlert('danger', 'Alıntı eklenirken bir hata oluştu.');
    })
    .finally(() => {
        // Submit butonunu tekrar aktif et
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
    });
}

// Alıntı silme işlemi
function deleteQuote(quoteId) {
    if (!confirm('Bu alıntıyı silmek istediğinizden emin misiniz?')) {
        return;
    }
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!token) {
        showAlert('danger', 'Güvenlik token\'ı bulunamadı.');
        return;
    }
    
    // AJAX ile alıntı sil
    fetch('/Book/DeleteQuote', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: `id=${quoteId}&__RequestVerificationToken=${token}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert('success', data.message);
            
            // Alıntı kartını kaldır
            const quoteCard = document.querySelector(`[data-quote-id="${quoteId}"]`);
            if (quoteCard) {
                quoteCard.style.animation = 'fadeOut 0.3s ease-out';
                setTimeout(() => {
                    quoteCard.remove();
                    
                    // Eğer hiç alıntı kalmadıysa boş durumu göster
                    const remainingQuotes = document.querySelectorAll('.quote-card');
                    if (remainingQuotes.length === 0) {
                        loadQuotes();
                    }
                }, 300);
            }
        } else {
            showAlert('danger', data.message || 'Alıntı silinirken bir hata oluştu.');
        }
    })
    .catch(error => {
        console.error('Alıntı silinirken hata:', error);
        showAlert('danger', 'Alıntı silinirken bir hata oluştu.');
    });
}

// Alert gösterme
function showAlert(type, message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // 3 saniye sonra otomatik kaldır
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 3000);
}

// Fade out animasyonu için CSS ekle
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeOut {
        from { opacity: 1; transform: scale(1); }
        to { opacity: 0; transform: scale(0.95); }
    }
`;
document.head.appendChild(style);
