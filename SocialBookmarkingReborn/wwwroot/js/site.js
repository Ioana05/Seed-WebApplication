// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


function goToBkmkShow(id) {
    window.location.href = `/Bookmarks/Show/${id}`;
}

function goToCatShow(id) {
    window.location.href = `/Categories/Show/${id}`;
}

window.onload = setCommentsSize();

function setCommentsSize() {
    let image = document.getElementById('img');
    let height = image.clientHeight;

    console.log(image);
    console.log('Img height is ' + height);

    let div = document.getElementById('div-comms');
    let padding = parseFloat(window.getComputedStyle(div).getPropertyValue('padding-top'));

    let bfcomms = document.getElementById('before-comms');
    let heightBefore = bfcomms.clientHeight;
    heightBefore += padding;

    let aftcomms = document.getElementById('after-comms');
    let heightAfter = aftcomms.clientHeight;
    heightAfter += padding;

    // am luat inaltimea imaginii => vrem sa setam o parte din ea ca fiind inaltimea
    // sectiunii de comentarii

    let comments = document.getElementById('comms');
    comments.style.height = (height - heightBefore - heightAfter) + "px";
}