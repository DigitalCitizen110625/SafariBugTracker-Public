$(document).ready(function () {

    /*******************************
    *
    *  Initializers
    *
    *******************************/


    //Activate the tooltip effect on any element with the matching data toggle attribute
    //https://getbootstrap.com/docs/4.0/components/tooltips/
    $('[data-toggle="tooltip"]').tooltip()



    /*******************************
    *
    *  Event Handlers
    *
    *******************************/


    //Apply onclick event to the menu arrow so that it rotates as the menu slides out
    document.getElementById('menu-toggle').addEventListener("click", ToggleSlideOutMenu);


    //Apply the.toggled class to the menu-toggle element when it's clicked
    $("#menu-toggle").click(function (e) {
        e.preventDefault();

        //Slide out the side bar from the left, pushing the content of the page to the right
        $("#wrapper").toggleClass("toggled");
    });



    /*******************************
    *
    *  Functions
    *
    *******************************/


    /*
    *  FUNCTION      : ToggleSlideOutMenu
    *  DESCRIPTION   : Activates an animation on the slide out menu
    */
    function ToggleSlideOutMenu(e)
    {
        e.currentTarget.classList.toggle("slideout-animation");
    }

});