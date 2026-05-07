const loginUrl = "/api/User/Login";

function jwtLogin() {
    // 1. clear previous error
    $("#error-message").attr("hidden", true).text("");

    // 2. read credentials
    const username = $("#username").val().trim();
    const password = $("#password").val();

    if (!username || !password) {
        $("#error-message").text("Please enter both username and password.").removeAttr("hidden");
        return;
    }

    // 3. show loading state
    $("#spinner-placeholder").addClass("spinner");
    $("#login-button").prop("disabled", true);

    const loginData = {
        username: username,
        password: password
    };

    // 4. send login request
    $.ajax({
        method: "POST",
        url: loginUrl,
        data: JSON.stringify(loginData),
        contentType: 'application/json'
    }).done(function (tokenData) {
        // 5. store JWT and redirect on success
        localStorage.setItem("JWT", tokenData);
        window.location.href = "logs.html";
    }).fail(function (err) {
        // 6. show friendly error and clear invalid token
        localStorage.removeItem("JWT");
        const status = err.status;
        const backendMessage = typeof err.responseText === "string" ? err.responseText : "";

        let message = "Login failed. Please check your username and password.";
        if (status === 0) {
            message = "Cannot reach the server. Please make sure WebAPI is running.";
        } else if (backendMessage) {
            message = backendMessage;
        }
        $("#error-message").text(message).removeAttr("hidden");
    }).always(function () {
        // 7. restore button state
        $("#spinner-placeholder").removeClass("spinner");
        $("#login-button").prop("disabled", false);
    });
}

$(function () {
    // 1. if already authenticated, go directly to logs
    const jwt = localStorage.getItem("JWT");
    if (jwt) {
        window.location.href = "logs.html";
        return;
    }

    // 2. allow Enter key submit from password field
    $("#password").on("keydown", function (event) {
        if (event.key === "Enter") {
            jwtLogin();
        }
    });
});
