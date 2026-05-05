function register() {
    $("#spinner-placeholder").addClass("spinner");
    $("#register-button").prop("disabled", true);

    const registerUrl = "/api/User/Register";
    let registerData = {
        "firstName": $("#firstName").val(),
        "lastName": $("#lastName").val(),
        "email": $("#email").val(),
        "phone": $("#phone").val(),
        "username": $("#username").val(),
        "password": $("#password").val()
    }
    $.ajax({
        method: "POST",
        url: registerUrl,
        data: JSON.stringify(registerData),
        contentType: 'application/json'
    }).done(function (tokenData) {
        //console.log(tokenData);
        // 1. verify register response and login flow if backend response changes

        // Do login after registration
        jwtLogin();
    }).fail(function (err) {
        alert(err.responseText);

        localStorage.removeItem("JWT");
        $("#spinner-placeholder").removeClass("spinner");
        $("#register-button").prop("disabled", false);
    });
}
