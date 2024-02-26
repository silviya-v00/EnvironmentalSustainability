
document.addEventListener('DOMContentLoaded', function () {
    const profileLink = document.getElementById('profile');
    const passwordLink = document.getElementById('change-password');
    const profileDiv = document.getElementById('profile-div');
    const passwordDiv = document.getElementById('change-password-div');
    const profileMsg = document.querySelector('.profile-msg');

    profileLink.addEventListener('click', function (e) {
        e.preventDefault();
        profileDiv.style.display = 'block';
        passwordDiv.style.display = 'none';
        profileLink.classList.add('active');
        passwordLink.classList.remove('active');
        if (profileMsg)
            profileMsg.remove();
    });

    passwordLink.addEventListener('click', function (e) {
        e.preventDefault();
        profileDiv.style.display = 'none';
        passwordDiv.style.display = 'block';
        profileLink.classList.remove('active');
        passwordLink.classList.add('active');
        if (profileMsg)
            profileMsg.remove();
    });
});
