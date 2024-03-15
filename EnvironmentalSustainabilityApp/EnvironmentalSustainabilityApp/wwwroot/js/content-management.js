
$(document).ready(function () {
    $('#add-new-content').click(function () {
        $('#content-menu .nav-link').removeClass('active');
        $(this).addClass('active');
        ResetForm();
    });

    function ResetForm() {
        $('#content-form')[0].reset();
        $('#selected-image').hide();
        $('#panel-img #ExistingContentImageFileName').remove();
        ShowHideImageOverlay(false);
    }

    function ShowHideImageOverlay(show) {
        var cssDisplay = "none";
        if (show)
            cssDisplay = "";

        $("#image-overlay").css("display", cssDisplay);
        $("#remove-image-overlay").css("display", cssDisplay);
    }

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
                    $('#content-list').append('<li class="nav-item"><a class="nav-link nav-hover" data-content-id="' + item.contentID + '" href="#">' + item.contentTitle + '</a></li>');
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
                ResetForm();

                $('#ContentTitle').val(data.contentTitle);
                $('#ContentDescription').val(data.contentDescription);
                $('#ContentLink').val(data.contentLink);
                $('#IsContentActive').prop('checked', data.isContentActive);

                if (data.contentImageFileName) {
                    ShowHideImageOverlay(true);

                    var imageUrl = '/Home/GetContentImage?fileName=' + encodeURIComponent(data.contentImageFileName);
                    $('#selected-image').attr('src', imageUrl).show();
                    $('#panel-img').append('<input type="hidden" id="ExistingContentImageFileName" name="ExistingContentImageFileName" value="' + data.contentImageFileName + '">');
                }
            },
            error: function () {
                alert('An error occurred while loading content details.');
            }
        });
    }

    $('#save-content').click(function (e) {
        e.preventDefault();

        if (IsContentDataValid()) {
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
        }
    });

    function IsContentDataValid() {
        $("#errorMsg").text("");
        $("#errorMsg").css("display", "none");

        var formData = new FormData($('#content-form')[0]);
        var contentTitle = formData.get('ContentTitle');
        var contentLink = formData.get('ContentLink');

        if (contentTitle == "") {
            $("#errorMsg").text("Content Title cannot be empty!");
            $("#errorMsg").css("display", "");

            return false;
        }

        if (contentLink == "") {
            $("#errorMsg").text("Content Link cannot be empty!");
            $("#errorMsg").css("display", "");

            return false;
        }
        else {
            try {
                new URL(contentLink);

                return true;
            } catch (err) {
                $("#errorMsg").text("Content Link is invalid!");
                $("#errorMsg").css("display", "");

                return false;
            }
        }
    }

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
                ShowHideImageOverlay(true);
            };
            reader.readAsDataURL(file);
        } else {
            selectedImage.src = '#';
            selectedImage.style.display = 'none';
            ShowHideImageOverlay(false);
        }
    });

    $('#remove-image-icon').click(function () {
        $('#selected-image').attr('src', '').hide();
        $('#ContentImage').val('');
        $('#ExistingContentImageFileName').remove();
        ShowHideImageOverlay(false);
    });
});
