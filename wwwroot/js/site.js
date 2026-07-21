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

document.addEventListener('DOMContentLoaded', function () {
    // Lắng nghe sự kiện click trên toàn bộ body (Event Delegation)
    document.body.addEventListener('click', function (e) {

        // Kiểm tra xem phần tử được click (hoặc thẻ cha của nó) có class add-to-cart-btn không
        const btn = e.target.closest('.add-to-cart-btn');

        if (btn) {
            e.preventDefault(); // Ngăn chặn URL thêm dấu #

            const productId = btn.dataset.productId;
            const quantity = parseInt(btn.dataset.quantity) || 1;

            // Gọi AJAX
            fetch(`/Cart/AddToCart?productId=${productId}&quantity=${quantity}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Cập nhật số lượng trên badge
                        const badge = document.getElementById('cart-badge');
                        if (badge) {
                            badge.textContent = data.cartCount;
                        }

                        if (typeof showSuccessMsg === 'function') {
                            showSuccessMsg(data.message);
                        }
                    } else {
                        if (typeof showErrorMsg === 'function') {
                            showErrorMsg(data.message);
                        }
                    }
                })
                .catch(error => {
                    console.error('Lỗi API:', error);
                    if (typeof showErrorMsg === 'function') {
                        showErrorMsg('Có lỗi xảy ra, vui lòng thử lại.');
                    }
                });
        }
    });
});

// Hàm hiển thị thông báo (toast) đơn giản
function showToast(message, type = 'success') {
    // Nếu có sẵn thư viện toast, dùng nó.
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        // Tạo container nếu chưa có
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.style.position = 'fixed';
        container.style.top = '20px';
        container.style.right = '20px';
        container.style.zIndex = '9999';
        container.style.maxWidth = '350px';
        document.body.appendChild(container);
    }

    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.role = 'alert';
    alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
    document.getElementById('toast-container').appendChild(alert);

    // Tự động biến mất
    setTimeout(() => {
        if (alert && alert.parentNode) {
            alert.classList.remove('show');
            setTimeout(() => alert.remove(), 300);
        }
    }, 1500);
}

// Xử lý yêu thích (Favorite)
document.addEventListener("DOMContentLoaded", function () {
    const IMG_OUTLINE = "/images/is-favorited-no.svg";
    const IMG_FILLED = "/images/is-favorited-yes.svg";

    // Sử dụng Event Delegation để bắt sự kiện cho các nút được thêm động
    document.body.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-favorite');
        if (!btn) return;

        e.preventDefault();
        const productId = btn.getAttribute('data-id');
        const imgElement = btn.querySelector('.favorite-icon');
        if (!imgElement) return;

        if (btn.disabled) return;
        btn.disabled = true;

        // Hiệu ứng nhấn
        btn.style.transform = 'scale(0.85)';
        setTimeout(() => btn.style.transform = '', 150);

        // Lấy token chống giả mạo (nếu có)
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : '';

        fetch(`/Product/ToggleFavorite?productId=${productId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        })
            .then(response => {
                if (response.status === 401) {
                    // Chưa đăng nhập → hiển thị toast yêu cầu đăng nhập
                    if (typeof showErrorMsg === 'function') {
                        showErrorMsg('Vui lòng đăng nhập để thêm sản phẩm vào danh sách yêu thích.');
                    } else {
                        alert('Vui lòng đăng nhập để thêm sản phẩm vào danh sách yêu thích.');
                    }
                    // Có thể redirect sau 1s
                    setTimeout(() => window.location.href = '/Auth/Login', 1500);
                    throw new Error('Unauthorized');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Cập nhật mọi thẻ của cùng một sản phẩm.
                    document.querySelectorAll(`.btn-favorite[data-id="${productId}"]`).forEach(favoriteButton => {
                        const icon = favoriteButton.querySelector('.favorite-icon');
                        if (icon) icon.src = data.isFavorited ? IMG_FILLED : IMG_OUTLINE;
                        favoriteButton.setAttribute('aria-pressed', data.isFavorited ? 'true' : 'false');
                    });

                    // Khi bỏ yêu thích tại hồ sơ, cập nhật danh sách ngay không cần tải lại trang.
                    if (!data.isFavorited) {
                        const favoriteItem = btn.closest('.favorite-product-item');
                        if (favoriteItem) {
                            favoriteItem.remove();
                            const list = document.getElementById('favorite-products-list');
                            const emptyState = document.getElementById('favorite-products-empty');
                            if (list && !list.querySelector('.favorite-product-item')) {
                                list.classList.add('d-none');
                                emptyState?.classList.remove('d-none');
                            }
                        }
                    }

                    // Chỉ hiển thị toast khi THÊM yêu thích (isFavorited = true)
                    if (data.isFavorited && typeof showSuccessMsg === 'function') {
                        showSuccessMsg('Đã thêm vào danh sách yêu thích!');
                    }
                    // Khi bỏ yêu thích (isFavorited = false) => không hiển thị toast
                } else {
                    if (typeof showErrorMsg === 'function') {
                        showErrorMsg(data.message || 'Có lỗi xảy ra, vui lòng thử lại.');
                    }
                }
            })
            .catch(error => {
                console.error('Error toggling favorite:', error);
                if (typeof showErrorMsg === 'function') {
                    showErrorMsg('Không thể kết nối đến máy chủ.');
                }
            })
            .finally(() => {
                btn.disabled = false;
            });
    });
});
