﻿$(document).ready(function () {

    //alert("document ready");
    $('#buttonUpdateCommonInfo').click(function () {
        //alert("button clicked");
        $.ajax({
            type: 'GET',
            url: '/admin/ServerInfo/UpdateCommonInfo',
            success: function (result) {
                $('#commonInfo').html(result)
            }
        });
    });

});