
document.addEventListener('DOMContentLoaded', function () {
    const loginTab = document.getElementById('login-tab');
    const registerTab = document.getElementById('register-tab');
    const loginContent = document.getElementById('login-content');
    const registerContent = document.getElementById('register-content');
    const msgError = document.getElementById('msgError');

    loginTab.addEventListener('click', function () {
        loginTab.classList.add('active');
        registerTab.classList.remove('active');
        loginContent.style.display = 'block';
        registerContent.style.display = 'none';
    });

    registerTab.addEventListener('click', function () {
        registerTab.classList.add('active');
        loginTab.classList.remove('active');
        registerContent.style.display = 'block';
        loginContent.style.display = 'none';
    });

    function fadeOutMsgError() {
        if (msgError) {
            var opacity = 1;
            var intervalId = setInterval(function () {
                opacity -= 0.1;
                msgError.style.opacity = opacity;
                if (opacity <= 0) {
                    msgError.style.display = 'none';
                    msgError.remove();
                    clearInterval(intervalId);
                }
            }, 100);
        }
    }

    if (msgError) {
        msgError.style.opacity = '1';
        setTimeout(fadeOutMsgError, 5000);
    }
});
