document.addEventListener('DOMContentLoaded', function() {
    // Sol menü navigasyon işlevselliği
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', function() {
            // Aktif menü öğesini güncelle
            navItems.forEach(nav => nav.classList.remove('active'));
            this.classList.add('active');
        });
    });

    // Arama çubuğu için otomatik odaklanma
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('focus', function() {
            this.parentElement.style.boxShadow = '0 0 0 3px rgba(134, 100, 31, 0.2)';
        });
        
        searchInput.addEventListener('blur', function() {
            this.parentElement.style.boxShadow = 'none';
        });
    }

    // Kitap alıntı kartları için hover efektleri
    const quoteCards = document.querySelectorAll('.quote-card');
    quoteCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Aktivite öğeleri için tıklama olayları
    const activityItems = document.querySelectorAll('.activity-item');
    activityItems.forEach(item => {
        item.addEventListener('click', function() {
            // Aktivite detaylarını gösterme (gelecekte implement edilebilir)
            console.log('Aktivite tıklandı:', this.querySelector('.activity-text').textContent);
        });
    });

    // Responsive menü toggle (mobil için)
    const createMobileMenuToggle = () => {
        if (window.innerWidth <= 768) {
            const sidebar = document.querySelector('.sidebar');
            const toggleButton = document.createElement('button');
            toggleButton.className = 'mobile-menu-toggle';
            toggleButton.innerHTML = '☰';
            toggleButton.style.cssText = `
                position: fixed;
                top: 20px;
                left: 20px;
                z-index: 1000;
                background: #86641f;
                color: white;
                border: none;
                border-radius: 50%;
                width: 50px;
                height: 50px;
                font-size: 1.5rem;
                cursor: pointer;
                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
                display: none;
            `;
            
            document.body.appendChild(toggleButton);
            
            toggleButton.addEventListener('click', function() {
                sidebar.classList.toggle('mobile-open');
            });
            
            // Sayfa dışına tıklandığında menüyü kapat
            document.addEventListener('click', function(e) {
                if (!sidebar.contains(e.target) && !toggleButton.contains(e.target)) {
                    sidebar.classList.remove('mobile-open');
                }
            });
        }
    };

    // Sayfa yüklendiğinde ve pencere boyutu değiştiğinde mobil menü toggle'ı oluştur
    createMobileMenuToggle();
    window.addEventListener('resize', createMobileMenuToggle);

    // Smooth scroll için
    const smoothScrollTo = (target) => {
        const element = document.querySelector(target);
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    };

    // Menü linklerine smooth scroll ekle
    const menuLinks = document.querySelectorAll('.sidebar-nav a[href^="#"]');
    menuLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const target = this.getAttribute('href');
            smoothScrollTo(target);
        });
    });

    // Arama formu submit olayı
    const searchForm = document.querySelector('.search-form');
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            const searchInput = this.querySelector('.search-input');
            if (!searchInput.value.trim()) {
                e.preventDefault();
                searchInput.style.borderColor = '#dc3545';
                searchInput.placeholder = 'Lütfen bir arama terimi girin...';
                
                setTimeout(() => {
                    searchInput.style.borderColor = '#D7CCC8';
                    searchInput.placeholder = 'Kitap adı, yazar veya ISBN ara...';
                }, 2000);
            }
        });
    }
});
