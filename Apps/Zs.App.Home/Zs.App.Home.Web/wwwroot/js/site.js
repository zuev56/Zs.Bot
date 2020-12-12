// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {

    $('#buttonUpdateAll').click(function () {
        $.ajax({
            type: 'GET',
            url: '/admin/ServerInfo/UpdateAll',
            success: function (result) {
                $('#serverInfo').html(result)
            }
        });
    });

});