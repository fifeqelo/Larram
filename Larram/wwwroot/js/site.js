// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

showInPopup = (url) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal .modal-body').html(res);
            $('#form-modal').modal('show');
            feather.replace({ width: '1em', height: '1em' })
        }
    })
}

jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    location.reload();
                }
                else {
                    $('#form-modal .modal-body').html(res.html);
                    feather.replace({ width: '1em', height: '1em' })
                }
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}
$(function () {
    $("#loaderbody").addClass('hide');

    $(document).bind('ajaxStart', function () {
        $("#loaderbody").removeClass('hide');
    }).bind('ajaxStop', function () {
        $("#loaderbody").addClass('hide');
    });
});

function OpenSideNav() {
    document.getElementById("sideNav").style.width = "100%";
}
function CloseSideNav() {
    document.getElementById("sideNav").style.width = "0";
}
$('.sale-btn').click(function () {
    $('.sideNav ul .sale-show').toggleClass('show');
    $('.sideNav ul .sale').toggleClass('rotate');

})
$('.sale-men-btn').click(function () {
    $('.sideNav ul li ul .sale-men-show').toggleClass('show');
    $('.sideNav ul li ul .sale-men').toggleClass('rotate');

})
$('.sale-women-btn').click(function () {
    $('.sideNav ul li ul .sale-women-show').toggleClass('show');
    $('.sideNav ul li ul .sale-women').toggleClass('rotate');

})
$('.women-btn').click(function () {
    $('.sideNav ul .women-show').toggleClass('show');
    $('.sideNav ul .women').toggleClass('rotate');
})
$('.women-clothes-btn').click(function () {
    $('.sideNav ul li ul .women-clothes-show').toggleClass('show');
    $('.sideNav ul li ul .women-clothes').toggleClass('rotate');

})
$('.women-addit-btn').click(function () {
    $('.sideNav ul li ul .women-addit-show').toggleClass('show');
    $('.sideNav ul li ul .women-addit').toggleClass('rotate');

})
$('.men-btn').click(function () {
    $('.sideNav ul .men-show').toggleClass('show');
    $('.sideNav ul .men').toggleClass('rotate');
})
$('.men-clothes-btn').click(function () {
    $('.sideNav ul li ul .men-clothes-show').toggleClass('show');
    $('.sideNav ul li ul .men-clothes').toggleClass('rotate');

})
$('.men-addit-btn').click(function () {
    $('.sideNav ul li ul .men-addit-show').toggleClass('show');
    $('.sideNav ul li ul .men-addit').toggleClass('rotate');

})