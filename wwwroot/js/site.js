// Cấu hình chung cho các thông báo của hệ thống
const Toast = typeof Swal === 'undefined'
    ? null
    : Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

// Hàm hiển thị thông báo thành công (Có thể gọi ở bất kỳ đâu)
function showSuccessMsg(message) {
    if (Toast) {
        Toast.fire({
            icon: 'success',
            title: message
        });
    }
}

// Hàm hiển thị báo lỗi
function showErrorMsg(message) {
    if (Toast) {
        Toast.fire({
            icon: 'error',
            title: message
        });
    }
}

// Nút cuộn lên đầu trang chỉ có trên trang chủ.
const homePagePaths = ['/', '/home', '/home/index'];
const isHomePage = homePagePaths.includes(window.location.pathname.toLowerCase());
let homeScrollTopButton = document.getElementById('home-scroll-top');

if (isHomePage && !homeScrollTopButton) {
    homeScrollTopButton = document.createElement('button');
    homeScrollTopButton.className = 'home-scroll-top';
    homeScrollTopButton.id = 'home-scroll-top';
    homeScrollTopButton.type = 'button';
    homeScrollTopButton.setAttribute('aria-label', 'Lên đầu trang');
    homeScrollTopButton.innerHTML = '<img src="/images/arrow-up.svg" alt="Lên đầu trang" width="24" height="24" />';
    document.body.appendChild(homeScrollTopButton);
}

if (homeScrollTopButton) {
    const scrollThreshold = 450;

    const updateScrollTopButton = () => {
        homeScrollTopButton.classList.toggle('is-visible', window.scrollY > scrollThreshold);
    };

    window.addEventListener('scroll', updateScrollTopButton, { passive: true });
    homeScrollTopButton.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    updateScrollTopButton();
}
