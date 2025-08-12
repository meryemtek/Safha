document.addEventListener('DOMContentLoaded', function() {
    // Sekme değiştirme işlevi
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            tabButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            // Burada içerik değiştirme mantığı eklenebilir
        });
    });
    
    // Profil menüsü için tıklama olayı
    const profileMenu = document.querySelector('.profile-menu');
    if (profileMenu) {
        profileMenu.addEventListener('click', function(e) {
            e.stopPropagation();
            const dropdown = this.querySelector('.dropdown-menu');
            dropdown.style.display = dropdown.style.display === 'block' ? 'none' : 'block';
        });
        
        document.addEventListener('click', function() {
            const dropdown = profileMenu.querySelector('.dropdown-menu');
            if (dropdown) {
                dropdown.style.display = 'none';
            }
        });
    }
});
