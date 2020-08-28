$(document).ready(function () {

    /*
    *  FUNCTION     : InitializeMagnificPopup
    *  DESCRIPTION  : Starts the Magnific script thereby enabling the lightbox js library to work
    *  REMARKS      : Note that Magnific Popup requires an href attribute in target element, even
    *                 if its an <img> and not an <a> element
    */
    $('.image-popup-no-margins').magnificPopup({
        type: 'image',
        closeOnContentClick: true,
        closeBtnInside: false,
        fixedContentPos: true,
        mainClass: 'mfp-no-margins mfp-with-zoom', // class to remove default margin from left and right side
        image: {
            verticalFit: true
        },
        zoom: {
            enabled: true,
            duration: 200
        }
    });



});