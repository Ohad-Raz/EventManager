$(function () {
    const $message = $("#profileMessage");
    const $saveBtn = $("#saveProfileBtn");

    function showMessage(text, isSuccess) {
        $message
            .removeClass("d-none alert-success alert-danger")
            .addClass(isSuccess ? "alert-success" : "alert-danger")
            .text(text);
    }

    function fillProfileFields(profile) {
        $("#FirstName").val(profile.firstName);
        $("#LastName").val(profile.lastName);
        $("#Email").val(profile.email);
        $("#Phone").val(profile.phone ?? "");
    }

    $saveBtn.on("click", function () {
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            type: "POST",
            url: "/User/UpdateProfile",
            data: {
                __RequestVerificationToken: token,
                FirstName: $("#FirstName").val(),
                LastName: $("#LastName").val(),
                Email: $("#Email").val(),
                Phone: $("#Phone").val()
            },
            success: function (response) {
                if (response.success) {
                    showMessage(response.message, true);
                    if (response.profile) {
                        fillProfileFields(response.profile);
                    }
                } else {
                    showMessage(response.message || "Profile update failed.", false);
                }
            },
            error: function () {
                showMessage("An error occurred while updating your profile.", false);
            }
        });
    });
});
