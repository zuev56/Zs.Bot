$(document).ready(function () {

    $("#VkUserActivity").hide();
    $("#VkDetailedUserActivity").hide();
    LoadUsers(null);

});

function LoadUsers(filterText) {
    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetUsers",
        data: {
            filterText: filterText
        },
        success: function (result) {
            $('#VkUsers').html(result);
        },
        error: function (error) {
            alert("Ошибка подгрузки списка пользователей")
        }
    });
};

function LoadUsersWithActivity() {
    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetUsersWithActivity",
        data: {
            filterText: $("#usersFilter").val(),
            fromDate: $('#FromDate').val(),
            toDate: $('#ToDate').val()
        },
        success: function (result) {
            $('#VkUsers').html(result);
        },
        error: function (error) {
            alert("Ошибка подгрузки списка пользователей")
        }
    });
};

function SelectUser(id) {
    $("#UserId").val(id);
    GetPeriodUserActivity();
};

function GetPeriodUserActivity() {
    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetUserActivity",
        data: {
            userId:   $("#UserId").val(),
            fromDate: $('#FromDate').val(),
            toDate:   $('#ToDate').val()
        },
        success: function (result) {
            $("#VkDetailedUserActivity").hide();
            $('#VkUserActivity').show().html(result);
        },
        error: function (error) {
            alert("Ошибка вывода активности пользователя");
        }
    });
};

function GetDetailedUserActivity() {
    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetDetailedUserActivity",
        data: { userId: $("#UserId").val() },
        success: function (result) {
            $("#VkUserActivity").hide();
            $("#VkDetailedUserActivity").show().html(result);
        },
        error: function (error) {
            alert("Ошибка выполнения детального анализа");
        }
    });
};

function OnTimeIntervalChanged() {
    if ($("#UserId").val() != "")
        GetPeriodUserActivity();
};

function GetUserActivity(id, fromDate, toDate) {

};