const loginUrl = "/api/User/Login";

function jwtLogin() {
    $("#spinner-placeholder").addClass("spinner");
    $("#login-button").prop("disabled", true);

    let loginData = {
        "username": $("#username").val(),
        "password": $("#password").val()
    }
    $.ajax({
        method: "POST",
        url: loginUrl,
        data: JSON.stringify(loginData),
        contentType: 'application/json'
    }).done(function (tokenData) {
        //console.log(tokenData);
        // 1. verify token response format if backend changes
        localStorage.setItem("JWT", tokenData);

        $("#spinner-placeholder").removeClass("spinner");
        $("#login-button").prop("disabled", false);

        // redirect
        window.location.href = "logs.html";
    }).fail(function (err) {
        alert(err.responseText);

        localStorage.removeItem("JWT");
        $("#spinner-placeholder").removeClass("spinner");
        $("#login-button").prop("disabled", false);
    });
}
