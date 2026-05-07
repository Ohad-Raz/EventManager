const logsGetByCountUrl = "/api/logs/get";
const logsCountUrl = "/api/logs/count";

function redirectToLoginAndClearToken() {
    // 1. clear JWT and go back to login
    localStorage.removeItem("JWT");
    window.location.href = "login.html";
}

function jwtLogout() {
    // 1. logout from local storage token
    redirectToLoginAndClearToken();
}

function renderLogsTable(logs) {
    // 1. ensure array value
    const items = Array.isArray(logs) ? logs : [];

    // 2. show empty state if there are no logs
    if (items.length === 0) {
        $("#placeholder").html('<div class="alert alert-info">No logs found.</div>');
        return;
    }

    // 3. build table rows
    let rowsHtml = "";
    for (const log of items) {
        rowsHtml += `
            <tr>
                <td>${log.id ?? ""}</td>
                <td>${log.timestamp ?? ""}</td>
                <td>${log.level ?? ""}</td>
                <td>${log.message ?? ""}</td>
                <td>${log.errorText ?? ""}</td>
            </tr>`;
    }

    // 4. render a clean Bootstrap table
    const tableHtml = `
        <div class="table-responsive">
            <table class="table table-striped table-bordered">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Timestamp</th>
                        <th>Level</th>
                        <th>Message</th>
                        <th>Error</th>
                    </tr>
                </thead>
                <tbody>${rowsHtml}</tbody>
            </table>
        </div>`;

    $("#placeholder").html(tableHtml);
}

function loadLogCount(jwt) {
    // 1. call count endpoint to display total logs
    return $.ajax({
        method: "GET",
        url: logsCountUrl,
        headers: {
            Authorization: `Bearer ${jwt}`
        }
    }).done(function (countValue) {
        $("#total-count").text(`Total logs: ${countValue}`);
    }).fail(function (err) {
        if (err.status === 401 || err.status === 403) {
            redirectToLoginAndClearToken();
            return;
        }
        $("#total-count").text("Total logs: unavailable");
    });
}

function loadLogs() {
    // 1. check JWT in localStorage and redirect to login if missing
    const jwt = localStorage.getItem("JWT");
    if (!jwt) {
        redirectToLoginAndClearToken();
        return;
    }

    // 2. read selected log count from dropdown (10/25/50)
    const selectedCount = $("#log-count").val();

    // 3. build request URL: /api/logs/get/{N}
    const requestUrl = `${logsGetByCountUrl}/${selectedCount}`;

    // 4. show spinner and disable refresh button
    $("#refresh-spinner-placeholder").addClass("spinner");
    $("#refresh-button").prop("disabled", true);

    // 5. update total count (best effort)
    loadLogCount(jwt);

    // 6. call logs endpoint with JWT
    $.ajax({
        method: "GET",
        url: requestUrl,
        headers: {
            Authorization: `Bearer ${jwt}`
        }
    }).done(function (logs) {
        // 7. render logs table
        renderLogsTable(logs);
    }).fail(function (err) {
        // 8. redirect on unauthorized, otherwise show friendly error
        if (err.status === 401 || err.status === 403) {
            redirectToLoginAndClearToken();
            return;
        }
        $("#placeholder").html('<div class="alert alert-danger">Failed to load logs. Please try again.</div>');
    }).always(function () {
        // 9. restore refresh button state
        $("#refresh-spinner-placeholder").removeClass("spinner");
        $("#refresh-button").prop("disabled", false);
    });
}

$(function () {
    // 1. check JWT in localStorage and redirect to login if missing
    const jwt = localStorage.getItem("JWT");
    if (!jwt) {
        redirectToLoginAndClearToken();
        return;
    }

    // 2. load logs immediately on page open
    loadLogs();
});
