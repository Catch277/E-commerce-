document.addEventListener("DOMContentLoaded", function () {
    // 1. Logic Ẩn / Hiện mật khẩu bằng Bootstrap Icons
    const toggleButtons = document.querySelectorAll('.toggle-password');
    toggleButtons.forEach(button => {
        button.addEventListener('click', function () {
            const targetId = this.getAttribute('data-target');
            const inputField = document.getElementById(targetId);
            const icon = this.querySelector('.pwd-icon');

            if (inputField.type === 'password') {
                inputField.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                inputField.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        });
    });

    // 2. Logic Kiểm tra sức mạnh mật khẩu (Giữ nguyên thanh Progress khi đang gõ)
    let timeout = null;
    const passwordInput = document.getElementById('Password');
    const progressBar = document.getElementById('password-progress');

    // Cập nhật Regex độ dài thành 6 ký tự
    const rules = {
        length: { id: 'rule-length', regex: /.{6,}/ },
        upper: { id: 'rule-upper', regex: /[A-Z]/ },
        number: { id: 'rule-number', regex: /[0-9]/ },
        special: { id: 'rule-special', regex: /[@@$!%*?&]/ }
    };

    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            const pwd = this.value;
            clearTimeout(timeout);

            // Chỉ reset khi xóa sạch ô input
            if(pwd.length === 0) {
                resetValidationUI();
                return;
            }

            // Chỉ kiểm tra và thay đổi giao diện sau khi ngừng gõ 2 giây
            timeout = setTimeout(() => {
                validatePassword(pwd);
            }, 500);
        });
    }

    function validatePassword(pwd) {
        let passedCount = 0;
        const totalRules = Object.keys(rules).length;

        for (const key in rules) {
            const rule = rules[key];
            const el = document.getElementById(rule.id);
            if (!el) continue;

            const icon = el.querySelector('i');

            if (rule.regex.test(pwd)) {
                passedCount++;
                // Pass: Đổi icon sang màu vàng, text sáng
                el.classList.remove('text-muted');
                el.classList.add('text-success-custom');
                if(icon) {
                    icon.classList.remove('bi-check-circle');
                    icon.classList.add('bi-check-circle-fill');
                }
            } else {
                // Fail: Trả về màu xám
                el.classList.remove('text-success-custom');
                el.classList.add('text-muted');
                if(icon) {
                    icon.classList.remove('bi-check-circle-fill');
                    icon.classList.add('bi-check-circle');
                }
            }
        }

        if (progressBar) {
            const percent = (passedCount / totalRules) * 100;
            progressBar.style.width = percent + '%';

            if (passedCount <= 1) {
                progressBar.className = 'progress-bar bg-danger'; 
            } else if (passedCount <= 3) {
                progressBar.className = 'progress-bar bg-warning'; 
            } else {
                progressBar.className = 'progress-bar bg-success'; 
            }
        }
    }

    function resetValidationUI() {
        for (const key in rules) {
            const el = document.getElementById(rules[key].id);
            if (!el) continue;
            
            el.classList.remove('text-success-custom');
            el.classList.add('text-muted');
            
            const icon = el.querySelector('i');
            if(icon) {
                icon.classList.remove('bi-check-circle-fill');
                icon.classList.add('bi-check-circle');
            }
        }
        if (progressBar) {
            progressBar.style.width = '0%';
            progressBar.className = 'progress-bar';
        }
    }

    // 3. Phone Input Mask (Bắt buộc +84, chỉ nhập số)
    const phoneInput = document.getElementById('Phone');
    if (phoneInput) {
        // Khởi tạo giá trị mặc định nếu rỗng
        if (!phoneInput.value.startsWith('+84')) {
            phoneInput.value = '+84';
        }

        phoneInput.addEventListener('input', function (e) {
            let val = this.value;
            // Nếu người dùng cố tình xóa hoàn toàn hoặc không bắt đầu bằng +84
            if (!val.startsWith('+84')) {
                val = '+84' + val.replace(/\+84/g, '');
            }
            
            // Lấy phần phía sau +84 và loại bỏ tất cả các ký tự không phải số
            let suffix = val.substring(3).replace(/[^0-9]/g, '');
            
            // Giới hạn độ dài chính xác 9 chữ số
            if (suffix.length > 9) {
                suffix = suffix.substring(0, 9);
            }
            
            this.value = '+84' + suffix;
        });

        // Chặn xóa (Backspace/Delete) phần +84
        phoneInput.addEventListener('keydown', function (e) {
            if ((e.key === 'Backspace' || e.key === 'Delete')) {
                if (this.selectionStart <= 3 && this.selectionEnd <= 3) {
                    e.preventDefault();
                }
            }
        });

        // Đặt trỏ chuột về cuối nếu người dùng click vào phần +84
        phoneInput.addEventListener('click', function (e) {
            if (this.selectionStart <= 3) {
                this.setSelectionRange(this.value.length, this.value.length);
            }
        });
    }
});
