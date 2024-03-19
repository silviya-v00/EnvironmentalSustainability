
function PopUpConfirm(message) {
    $('.confirm-pop-up-overlay').show();

    $('.confirm-pop-up-modal').html(`
                <div class="confirm-pop-up-modal-content">
                    <div class="confirm-pop-up-modal-header">
                        <h2>Confirmation</h2>
                        <span class="confirm-pop-up-close">&times;</span>
                    </div>
                    <div class="confirm-pop-up-modal-body">
                        <p>${message}</p>
                        <div class="confirm-pop-up-modal-footer">
                            <button id="confirm-pop-up-yes" class="btn">Yes</button>
                            <button id="confirm-pop-up-no" class="btn btn-danger" style="background-color: #dc3545 !important;">No</button>
                        </div>
                    </div>
                </div>
            `).show();

    return new Promise(resolve => {
        $('#confirm-pop-up-yes').click(() => {
            $('.confirm-pop-up-overlay, .confirm-pop-up-modal').hide();
            resolve(true);
        });

        $('#confirm-pop-up-no, .confirm-pop-up-close').click(() => {
            $('.confirm-pop-up-overlay, .confirm-pop-up-modal').hide();
            resolve(false);
        });
    });
}
