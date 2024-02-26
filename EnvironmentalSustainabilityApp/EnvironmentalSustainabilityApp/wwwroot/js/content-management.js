
$(document).ready(function () {
    $('#add-new-content').click(function () {
        $('#content-menu .nav-link').removeClass('active');
        $(this).addClass('active');

        $('#content-form')[0].reset();
        $('#selected-image').hide();
    });

    LoadContentList();

    $('#content-list').on('click', '.nav-link', function () {
        $('#content-menu .nav-link').removeClass('active');
        $(this).addClass('active');

        var contentId = $(this).data('content-id');
        LoadContentDetailsForm(contentId);
    });

    function LoadContentList() {
        $.ajax({
            url: '/Home/GetContentList',
            type: 'GET',
            success: function (data) {
                $('#content-list').empty();
                $.each(data, function (index, item) {
                    $('#content-list').append('<li class="nav-item"><a class="nav-link" data-content-id="' + item.contentID + '" href="#">' + item.contentTitle + '</a></li>');
                });
            },
            error: function () {
                alert('An error occurred while loading content list.');
            }
        });
    }

    function LoadContentDetailsForm(contentId) {
        $.ajax({
            url: '/Home/GetContentDetails',
            type: 'GET',
            data: { contentId: contentId },
            success: function (data) {
                $('#ContentTitle').val(data.contentTitle);
                $('#ContentDescription').val(data.contentDescription);
                $('#ContentLink').val(data.contentLink);
                $('#IsContentActive').prop('checked', data.isContentActive);

                if (data.contentImageFileName) {
                    var imageUrl = '/Home/GetContentImage?fileName=' + encodeURIComponent(data.contentImageFileName);
                    $('#selected-image').attr('src', imageUrl).show();
                    $('#panel-img').append('<input type="hidden" id="ExistingContentImageFileName" name="ExistingContentImageFileName" value="' + data.contentImageFileName + '">');
                } else {
                    $('#selected-image').hide();
                }
            },
            error: function () {
                alert('An error occurred while loading content details.');
            }
        });
    }

    $('#save-content').click(function (e) {
        e.preventDefault();

        var formData = new FormData($('#content-form')[0]);

        var contentId = 0;
        var activeNavItem = $('.nav-link.active[data-content-id]');
        if (activeNavItem.length > 0) {
            contentId = activeNavItem.data('content-id');
        }
        formData.append('ContentID', contentId);

        var existingImage = "";
        var existingImageItem = $('#ExistingContentImageFileName');
        if (existingImageItem.length > 0) {
            existingImage = existingImageItem.val();
        }
        formData.append('ExistingContentImageFileName', existingImage);

        var imageFile = $('#ContentImage')[0].files[0];
        formData.append('ContentImage', imageFile);

        var isContentActiveString = formData.get('IsContentActive');
        var isContentActive = isContentActiveString === 'on';
        formData.set('IsContentActive', isContentActive);

        $.ajax({
            url: '/Home/SaveContent',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                location.reload();
            },
            error: function () {
                alert('An error occurred while saving content.');
            }
        });
    });

    $('#delete-content').click(function () {
        var activeNavItem = $('.nav-link.active[data-content-id]');

        if (activeNavItem.length > 0) {
            var contentId = activeNavItem.data('content-id');

            if (confirm('Are you sure you want to delete this content?')) {
                $.ajax({
                    url: '/Home/DeleteContent',
                    type: 'POST',
                    data: { contentId: contentId },
                    success: function () {
                        location.reload();
                    },
                    error: function () {
                        alert('An error occurred while deleting content.');
                    }
                });
            }
        } else {
            alert('No content is selected for deletion.');
        }
    });

    const contentImageInput = document.getElementById('ContentImage');
    const selectedImage = document.getElementById('selected-image');

    contentImageInput.addEventListener('change', function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                selectedImage.src = e.target.result;
                selectedImage.style.display = 'block';
            };
            reader.readAsDataURL(file);
        } else {
            selectedImage.src = '#';
            selectedImage.style.display = 'none';
        }
    });

    $('#remove-image-icon').click(function () {
        $('#selected-image').attr('src', '').hide();
        $('#ContentImage').val('');
        $('#ExistingContentImageFileName').remove();
    });
});
