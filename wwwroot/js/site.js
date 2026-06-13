// Site-wide UX helpers
(function () {
    const fallbackImage = '/images/no-image.jpg';
    document.querySelectorAll('img').forEach(function (img) {
        img.loading = img.loading || 'lazy';
        img.decoding = img.decoding || 'async';
        img.addEventListener('error', function () {
            if (!img.dataset.fallbackApplied) {
                img.dataset.fallbackApplied = 'true';
                img.src = img.alt && img.alt.toLowerCase().includes('avatar')
                    ? '/images/avatar-default.png'
                    : fallbackImage;
            }
        });
    });

    document.querySelectorAll('input[type="file"]').forEach(function (input) {
        input.addEventListener('change', function () {
            const fileName = input.files && input.files.length ? input.files[0].name : '';
            input.title = fileName || 'Chưa chọn tệp';
        });
    });
})();
