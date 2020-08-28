$(document).ready(function () {

    /*******************************
    *
    *  Event Handlers
    *
    *******************************/


    //Create an onclick event listener for each element identified as an accordion panel
    //SOURCE: https://www.w3schools.com/howto/tryit.asp?filename=tryhow_js_accordion_symbol
    $('.accordion-panel').click(function () {
        this.classList.toggle("toggled-panel");
        let panel = this.nextElementSibling;
        expandPanel(panel);
    });



    /*******************************
    *
    *  Functions
    *
    *******************************/


    /*
    *  FUNCTION      : expandPanel
    *  DESCRIPTION   : Changes the maxHeight of the argument, based on the height of its content
    */
    function expandPanel(element)
    {
        if (element.style.maxHeight) {
            element.style.maxHeight = null;
        }
        else {
            element.style.maxHeight = element.scrollHeight + "px";
        }
    }
});