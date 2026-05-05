const logsGetByCountUrl = "/api/logs/get";
const logsCountUrl = "/api/logs/count";
// TODO: replace endpoint if my backend route differs

function jwtLogout() {
    // 1. remove JWT from localStorage
    // 2. redirect user to login page
    localStorage.removeItem("JWT");
    window.location.href = "login.html";
}

function renderLogsTable(logs) {
    // 1. clear current placeholder content
    // 2. verify logs is an array
    // 3. if empty, render "No logs found"
    // 4. build a simple Bootstrap table (ID, Timestamp, Level, Message)
    // 5. append rows using available DTO fields
    // TODO: adjust rendering once final Log DTO shape is confirmed
    // TODO: keep defensive mapping if field names differ (timestamp/timeStamp, etc.)
}

function loadLogs() {
    // 1. check JWT in localStorage and redirect to login if missing
    const jwt = localStorage.getItem("JWT");
    if (!jwt) {
        window.location.href = "login.html";
        return;
    }

    // 2. read selected log count from dropdown (10/25/50)
    const selectedCount = $("#log-count").val();

    // 3. build request URL: /api/logs/get/{N}
    const requestUrl = `${logsGetByCountUrl}/${selectedCount}`;

    // 4. show spinner and disable refresh button
    $("#refresh-spinner-placeholder").addClass("spinner");
    $("#refresh-button").prop("disabled", true);

    console.log(requestUrl);
    // 5. call logs endpoint using AJAX + Authorization: Bearer <JWT>
    // 6. on success: call renderLogsTable(logs)
    // 7. handle unauthorized response (401/403) - clear JWT and redirect
    // 8. handle other errors and show message in placeholder
    // 9. hide spinner and re-enable refresh button
    // TODO: verify exact logs endpoint in your backend
    // TODO: optional use of /api/logs/count for total logs display
}

$(function () {
    // 1. check JWT in localStorage and redirect to login if missing
    const jwt = localStorage.getItem("JWT");
    if (!jwt) {
        window.location.href = "login.html";
        return;
    }
    // 2. attach click handler for refresh button if needed
    // 3. call loadLogs() for initial page load
    loadLogs(); // You can keep or replace this call during your implementation
});
