// Cấu hình chung cho các thông báo của hệ thống
const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer)
        toast.addEventListener('mouseleave', Swal.resumeTimer)
    }
});

// Hàm hiển thị thông báo thành công (Có thể gọi ở bất kỳ đâu)
function showSuccessMsg(message) {
    Toast.fire({
        icon: 'success',
        title: message
    });
}

// Hàm hiển thị báo lỗi
function showErrorMsg(message) {
    Toast.fire({
        icon: 'error',
        title: message
    });
}