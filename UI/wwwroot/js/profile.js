// Profil sayfası JavaScript fonksiyonları

document.addEventListener('DOMContentLoaded', function() {
    // Tab değiştirme işlevselliği
    initializeTabs();
    
    // Profil resmi ve kapak fotoğrafı yükleme işlevselliği
    initializePhotoUploads();
    
    // Takip butonu işlevselliği
    initializeFollowButton();
    
    // Kullanıcı alıntılarını yükle
    loadUserQuotes();
});

// Tab değiştirme işlevselliği
function initializeTabs() {
    const tabButtons = document.querySelectorAll('.tab-button');
    const tabContents = document.querySelectorAll('.tab-content');
    const subtabButtons = document.querySelectorAll('.subtab-button');
    
    // Ana sekmelerin işlevselliği
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            if (this.tagName.toLowerCase() === 'a' && !this.getAttribute('href').startsWith('#')) {
                return; // Eğer gerçek bir link ise, normal davranışını sürdür
            }
            
            const targetTab = this.getAttribute('data-tab');
            
            // Aktif tab'ı değiştir
            tabButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            
            // Tab içeriğini göster/gizle
            tabContents.forEach(content => {
                content.classList.remove('active');
                if (content.id === targetTab + '-tab') {
                    content.classList.add('active');
                }
            });
            
            // Kitaplık sekmesi seçildiyse alt sekmeleri göster, değilse gizle
            const librarySubtabs = document.getElementById('library-subtabs');
            if (librarySubtabs) {
                if (targetTab === 'books') {
                    librarySubtabs.style.display = 'flex';
                } else {
                    librarySubtabs.style.display = 'none';
                }
            }
        });
    });
    
    // Alt sekmelerin işlevselliği
    subtabButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (this.classList.contains('active')) {
                return; // Zaten aktifse bir şey yapma
            }
            
            // Aktif alt sekmeyi değiştir
            subtabButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

// Fotoğraf yükleme işlevselliği
function initializePhotoUploads() {
    // Profil resmi ve kapak fotoğrafı input'ları zaten HTML'de tanımlı
    // onchange event'leri ile uploadPhoto fonksiyonu çağrılıyor
}

// Fotoğraf yükleme fonksiyonu
function uploadPhoto(input, photoType) {
    const file = input.files[0];
    if (!file) return;
    
    // Dosya boyutu kontrolü (5MB)
    if (file.size > 5 * 1024 * 1024) {
        alert('Dosya boyutu 5MB\'dan küçük olmalıdır.');
        return;
    }
    
    // Dosya tipi kontrolü
    if (!file.type.startsWith('image/')) {
        alert('Lütfen geçerli bir resim dosyası seçin.');
        return;
    }
    
    // Loading göster
    showLoading(photoType);
    
    // FormData oluştur
    const formData = new FormData();
    formData.append('file', file);
    formData.append('photoType', photoType);
    
    // AJAX ile yükle
    fetch('/Profile/UploadPhoto', {
        method: 'POST',
        body: formData,
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => response.json())
    .then(data => {
        hideLoading(photoType);
        
        if (data.success) {
            // Başarılı yükleme
            if (photoType === 'profile') {
                updateProfilePicture(data.photoUrl);
            } else if (photoType === 'cover') {
                updateCoverPhoto(data.photoUrl);
            }
            
            showSuccessMessage(data.message);
        } else {
            // Hata durumu
            showErrorMessage(data.message);
        }
    })
    .catch(error => {
        hideLoading(photoType);
        showErrorMessage('Fotoğraf yüklenirken bir hata oluştu: ' + error.message);
    });
    
    // Input'u temizle
    input.value = '';
}

// Profil resmini güncelle
function updateProfilePicture(photoUrl) {
    const profileAvatar = document.querySelector('.avatar-image');
    if (profileAvatar) {
        profileAvatar.src = photoUrl;
    }
}

// Kapak fotoğrafını güncelle
function updateCoverPhoto(photoUrl) {
    const coverImage = document.querySelector('.cover-image');
    if (coverImage) {
        coverImage.style.backgroundImage = `url('${photoUrl}')`;
    }
}

// Loading göster
function showLoading(photoType) {
    let loadingElement = document.getElementById(`${photoType}-loading`);
    
    if (!loadingElement) {
        loadingElement = document.createElement('div');
        loadingElement.id = `${photoType}-loading`;
        loadingElement.className = 'loading-overlay';
        loadingElement.innerHTML = `
            <div class="loading-spinner">
                <div class="spinner"></div>
                <p>Yükleniyor...</p>
            </div>
        `;
        
        if (photoType === 'profile') {
            document.querySelector('.profile-avatar').appendChild(loadingElement);
        } else if (photoType === 'cover') {
            document.querySelector('.cover-image').appendChild(loadingElement);
        }
    }
    
    loadingElement.style.display = 'flex';
}

// Loading gizle
function hideLoading(photoType) {
    const loadingElement = document.getElementById(`${photoType}-loading`);
    if (loadingElement) {
        loadingElement.style.display = 'none';
    }
}

// Başarı mesajı göster
function showSuccessMessage(message) {
    showMessage(message, 'success');
}

// Hata mesajı göster
function showErrorMessage(message) {
    showMessage(message, 'error');
}

// Mesaj göster
function showMessage(message, type) {
    // Mevcut mesaj varsa kaldır
    const existingMessage = document.querySelector('.message-popup');
    if (existingMessage) {
        existingMessage.remove();
    }
    
    // Yeni mesaj oluştur
    const messageElement = document.createElement('div');
    messageElement.className = `message-popup message-${type}`;
    messageElement.innerHTML = `
        <div class="message-content">
            <span class="message-icon">${type === 'success' ? '✅' : '❌'}</span>
            <span class="message-text">${message}</span>
        </div>
    `;
    
    // Mesajı sayfaya ekle
    document.body.appendChild(messageElement);
    
    // Animasyon ile göster
    setTimeout(() => {
        messageElement.classList.add('show');
    }, 100);
    
    // 3 saniye sonra gizle
    setTimeout(() => {
        messageElement.classList.remove('show');
        setTimeout(() => {
            messageElement.remove();
        }, 300);
    }, 3000);
}

// Sayfa yüklendiğinde animasyonları başlat
window.addEventListener('load', function() {
    // Profil kartı animasyonu
    const profileHeader = document.querySelector('.profile-header-card');
    if (profileHeader) {
        profileHeader.style.opacity = '0';
        profileHeader.style.transform = 'translateY(30px)';
        
        setTimeout(() => {
            profileHeader.style.transition = 'all 0.6s ease';
            profileHeader.style.opacity = '1';
            profileHeader.style.transform = 'translateY(0)';
        }, 200);
    }
    
    // İstatistik kartları animasyonu
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, 400 + (index * 100));
    });
    
    // Okuma hedefi animasyonu
    const progressFill = document.querySelector('.progress-fill');
    if (progressFill) {
        const targetWidth = progressFill.style.width;
        progressFill.style.width = '0%';
        
        setTimeout(() => {
            progressFill.style.transition = 'width 1s ease';
            progressFill.style.width = targetWidth;
        }, 800);
    }
});

// Smooth scroll için
function smoothScrollTo(element) {
    element.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
    });
}

// Profil düzenleme sayfasına yönlendir
function goToEditProfile() {
    window.location.href = '/Profile/Edit';
}

// Profil düzenleme butonuna tıklandığında
document.addEventListener('click', function(e) {
    if (e.target.closest('.btn-edit-profile')) {
        e.preventDefault();
        goToEditProfile();
    }
});

// Responsive menü toggle (mobil için)
function toggleMobileMenu() {
    const sidebar = document.querySelector('.profile-sidebar');
    if (sidebar) {
        sidebar.classList.toggle('mobile-open');
    }
}

// Mobil menü toggle butonu ekle (eğer gerekirse)
function addMobileMenuToggle() {
    if (window.innerWidth <= 768) {
        const sidebar = document.querySelector('.profile-sidebar');
        if (sidebar && !document.querySelector('.mobile-toggle')) {
            const toggleButton = document.createElement('button');
            toggleButton.className = 'mobile-toggle';
            toggleButton.innerHTML = '☰';
            toggleButton.onclick = toggleMobileMenu;
            
            sidebar.insertBefore(toggleButton, sidebar.firstChild);
        }
    }
}

// Pencere boyutu değiştiğinde mobil menü toggle'ı ekle
window.addEventListener('resize', addMobileMenuToggle);

// Sayfa yüklendiğinde mobil menü toggle'ı ekle
document.addEventListener('DOMContentLoaded', addMobileMenuToggle);

// Takip butonu işlevselliği
function initializeFollowButton() {
    const followBtn = document.getElementById('followBtn');
    if (!followBtn) return;
    
    followBtn.addEventListener('click', async function() {
        const userId = this.getAttribute('data-user-id');
        const isFollowing = this.getAttribute('data-following') === 'true';
        const btn = this;
        
        // Butonu devre dışı bırak
        btn.disabled = true;
        const originalText = btn.textContent;
        btn.textContent = 'İşleniyor...';
        
        try {
            const action = isFollowing ? 'Unfollow' : 'Follow';
            
            // FormData kullanarak gönder
            const formData = new URLSearchParams();
            formData.append('userId', userId);
            
            const response = await fetch(`/Profile/${action}?userId=${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: formData
            });
            
            const data = await response.json();
            
            if (data.success) {
                // Buton durumunu güncelle
                if (isFollowing) {
                    btn.textContent = 'Takip Et';
                    btn.setAttribute('data-following', 'false');
                    btn.classList.remove('btn-following');
                    btn.classList.add('btn-primary');
                } else {
                    btn.textContent = 'Takibi Bırak';
                    btn.setAttribute('data-following', 'true');
                    btn.classList.remove('btn-primary');
                    btn.classList.add('btn-following');
                }
                
                // Takipçi sayılarını güncelle
                updateFollowerCounts(data.followerCount, data.followingCount);
                
                // Başarı mesajı göster
                showSuccessMessage(data.message);
            } else {
                // Hata mesajı göster
                showErrorMessage(data.message);
                btn.textContent = originalText;
                console.error('Takip işlemi başarısız:', data.message);
            }
        } catch (error) {
            console.error('Takip işlemi hatası:', error);
            showErrorMessage('Bir hata oluştu. Lütfen tekrar deneyin.');
            btn.textContent = originalText;
        } finally {
            // Butonu tekrar aktif et
            btn.disabled = false;
        }
    });
}

// Takipçi sayılarını güncelle
function updateFollowerCounts(followerCount, followingCount) {
    // Takipçi sayısını güncelle
    const followerCountElements = document.querySelectorAll('.follow-count');
    if (followerCountElements.length > 0 && followerCount !== undefined) {
        followerCountElements[0].textContent = followerCount;
    }
}

// Kullanıcı alıntılarını yükle
function loadUserQuotes() {
    const quotesList = document.getElementById('userQuotesList');
    if (!quotesList) return;
    
    // Profil sayfasındaki kullanıcı ID'sini al
    const followBtn = document.getElementById('followBtn');
    let profileUserId = null;
    
    if (followBtn) {
        // Başka birinin profili görüntüleniyorsa
        profileUserId = followBtn.getAttribute('data-user-id');
    } else {
        // Kendi profilimizi görüntülüyorsak, URL'den veya meta tag'den al
        profileUserId = getUserIdFromPage();
    }
    
    if (!profileUserId) {
        console.log('Kullanıcı ID bulunamadı');
        return;
    }
    
    fetch(`/Profile/GetUserQuotes/${profileUserId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.quotes && data.quotes.length > 0) {
                quotesList.innerHTML = data.quotes.map(quote => `
                    <div class="quote-card" data-quote-id="${quote.id}">
                        <div class="quote-header">
                            <div class="quote-book-info">
                                <img src="${quote.bookCoverImage}" alt="${quote.bookTitle}" class="quote-book-cover">
                                <div class="quote-book-details">
                                    <h4 class="quote-book-title">${quote.bookTitle}</h4>
                                    <p class="quote-book-author">${quote.bookAuthor}</p>
                                </div>
                            </div>
                            ${quote.canDelete ? `<button class="btn-delete-quote" onclick="deleteQuote(${quote.id})">🗑️</button>` : ''}
                        </div>
                        <div class="quote-body">
                            <p class="quote-content">"${quote.content}"</p>
                            ${quote.author ? `<p class="quote-author">— ${quote.author}</p>` : ''}
                            ${quote.pageNumber ? `<p class="quote-page">Sayfa: ${quote.pageNumber}</p>` : ''}
                            ${quote.notes ? `<p class="quote-notes"><strong>Notlar:</strong> ${quote.notes}</p>` : ''}
                        </div>
                        <div class="quote-footer">
                            <span class="quote-date">📅 ${quote.createdAt}</span>
                        </div>
                    </div>
                `).join('');
            } else {
                quotesList.innerHTML = '<div class="no-quotes"><p>Henüz alıntı eklenmemiş.</p></div>';
            }
        })
        .catch(error => {
            console.error('Alıntılar yüklenirken hata:', error);
            quotesList.innerHTML = '<div class="no-quotes"><p>Alıntılar yüklenirken bir hata oluştu.</p></div>';
        });
}

// Sayfadan kullanıcı ID'sini al (yedek metod)
function getUserIdFromPage() {
    // Hidden field'den kullanıcı ID'sini al
    const userIdElement = document.getElementById('profileUserId');
    if (userIdElement) {
        return userIdElement.value;
    }
    
    // URL'den veya başka bir yerden kullanıcı ID'sini almaya çalış (yedek)
    const url = window.location.href;
    const match = url.match(/\/Profile\/View\/(\d+)/);
    return match ? match[1] : null;
}

// Alıntı silme fonksiyonu
function deleteQuote(quoteId) {
    if (!confirm('Bu alıntıyı silmek istediğinizden emin misiniz?')) {
        return;
    }
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    fetch('/Profile/DeleteQuote', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ id: quoteId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Alıntı kartını DOM'dan kaldır
            const quoteCard = document.querySelector(`[data-quote-id="${quoteId}"]`);
            if (quoteCard) {
                quoteCard.style.transition = 'all 0.3s ease';
                quoteCard.style.opacity = '0';
                quoteCard.style.transform = 'translateX(-20px)';
                
                setTimeout(() => {
                    quoteCard.remove();
                    
                    // Eğer hiç alıntı kalmadıysa "henüz alıntı yok" mesajını göster
                    const quotesList = document.getElementById('userQuotesList');
                    if (quotesList && quotesList.children.length === 0) {
                        quotesList.innerHTML = '<div class="no-quotes"><p>Henüz alıntı eklenmemiş.</p></div>';
                    }
                }, 300);
            }
            
            showSuccessMessage(data.message);
        } else {
            showErrorMessage(data.message);
        }
    })
    .catch(error => {
        console.error('Alıntı silinirken hata:', error);
        showErrorMessage('Alıntı silinirken bir hata oluştu.');
    });
}
