document.addEventListener('DOMContentLoaded', function() {
    // Tab değiştirme işlevselliği
    const tabButtons = document.querySelectorAll('.tab-button');
    const tabContents = document.querySelectorAll('.tab-content');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetTab = this.getAttribute('data-tab');
            
            // Aktif tab'ı güncelle
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));
            
            this.classList.add('active');
            document.getElementById(targetTab + '-tab').classList.add('active');
        });
    });

    // Takip/Takibi bırak butonu işlevselliği
    const followButtons = document.querySelectorAll('.btn-follow, .btn-following');
    
    followButtons.forEach(button => {
        button.addEventListener('click', function() {
            const userId = this.getAttribute('data-user-id');
            const isFollowing = this.getAttribute('data-is-following') === 'true';
            
            // AJAX ile takip işlemi
            fetch(`/Profile/Follow/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Buton metnini güncelle
                    if (data.isFollowing) {
                        this.className = 'btn btn-following';
                        this.setAttribute('data-is-following', 'true');
                        this.textContent = 'Takip Ediliyor';
                    } else {
                        this.className = 'btn btn-follow';
                        this.setAttribute('data-is-following', 'false');
                        this.textContent = 'Takip Et';
                    }
                    
                    // Takipçi sayısını güncelle
                    const followerCountElement = document.querySelector('.stat-item:nth-child(2) .stat-value');
                    if (followerCountElement) {
                        followerCountElement.textContent = data.followerCount;
                    }
                } else {
                    alert(data.message || 'Bir hata oluştu!');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Bir hata oluştu!');
            });
        });
    });

    // Okuma ilerlemesi animasyonu
    const progressFill = document.querySelector('.reading-progress-fill');
    if (progressFill) {
        const targetWidth = progressFill.style.width;
        progressFill.style.width = '0%';
        
        setTimeout(() => {
            progressFill.style.width = targetWidth;
        }, 500);
    }

    // Kitap kartları hover efektleri
    const bookCards = document.querySelectorAll('.book-card');
    bookCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
});
