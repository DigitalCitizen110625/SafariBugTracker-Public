$(document).ready(function ()
{

    /*******************************
    *
    *  Initializers
    *
    *******************************/


    //Starts the wow script thereby enabling animations on webpage load
    new WOW().init();


    //Activate the popover effect on any element with the matching data toggle attribute
    //https://getbootstrap.com/docs/4.0/components/popovers/
    $('[data-toggle="popover"]').popover();


    //Activate the tooltip effect on any element with the matching data toggle attribute
    //https://getbootstrap.com/docs/4.0/components/tooltips/
    $('[data-toggle="tooltip"]').tooltip()


    //Pops up a modal when the user lands on the main page
    //https://getbootstrap.com/docs/4.0/components/modal/#via-javascript
    $("#modalDemoPopup").modal('show');



    /*******************************
    *
    *  Selectors
    *
    *******************************/

    const goToTopButton = document.getElementById("gotoTopButton");

    const nabvarHeight = $('nav').outerHeight(false);
    const navbarWrapper = $('.nav-wrapper');
    const $navBar = $('.navbar');
    const distFromTop = $navBar.offset().top;



    /*******************************
    *
    *  Event Handlers
    *
    *******************************/


    //Add onclick event to smoothly scroll the window back to the top of the web-page when the goToTopButton element is pressed
    if (goToTopButton)
    {
        goToTopButton.addEventListener("click", function ()
        {
            $("html, body").animate({ scrollTop: 0 }, "slow");
        });
    }


    //Register the navbar and its container to resize on any of the following events
    $(window).on({
        load: function () {
            resizeElement(navbarWrapper, nabvarHeight);
            adjustNavBarText();
        },
        resize: function () {
            resizeElement(navbarWrapper, nabvarHeight);
            adjustNavBarText();
        },
        scroll: function () {
            toggleStickyNavbar($navBar, distFromTop);
        }
    });



    /*******************************
    *
    *  Functions
    *
    *******************************/


    /*
    *  FUNCTION      : resizeElement
    *  DESCRIPTION   : Sets the target html element to the new value of the height argument
    */
    function resizeElement(targetElement, newHeight)
    {
        targetElement.css('height', newHeight);
    }

    
    /*
    *  FUNCTION      : toggleStickyNavbar
    *  DESCRIPTION   : Toggles the navbar from a relative to fixed position upon scrolling down the page
    *  SOURCE        : http://jsfiddle.net/1e1cg6t5/
    */
    function toggleStickyNavbar(navbar, distFromTop)
    {
        //Get scroll position from top of the page
        var scrollPos = $(this).scrollTop();

        //Check if scroll position is >= the nav position
        if (scrollPos > distFromTop)
        {
            navbar.css('position', 'fixed');
        }
        else
        {
            navbar.css('position', 'relative');
        }
    }


    /*
    *  FUNCTION      : adjustNavBarText
    *  DESCRIPTION   : Drops the "Bug Tracker" sub string from the navbar title element 
    *                  to better adjust for smaller screens
    */
    function adjustNavBarText()
    {
        const navbarTitle = document.getElementById('navbarTitle');
        const screenWidth = screen.width;

        if (screenWidth <= 329)
        {
            navbarTitle.innerText = "Safari";
        }
        else
        {
            navbarTitle.innerText = "Safari Bug Tracker";
        }
    }
});