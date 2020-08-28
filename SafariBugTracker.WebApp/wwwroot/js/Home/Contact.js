$(document).ready(function () {

    /*******************************
    *
    *  Event Handlers
    *
    *******************************/


    //Create an onclick event for counting the remaining chars in the text area
    $('textarea').keydown(countChars);
    $('textarea').keyup(countChars);



    /*******************************
    *
    *  Functions
    *
    *******************************/


    /*
    *  FUNCTION      : countChars
    *  DESCRIPTION   : Counts the number of chars in the target element, and updates the remaining chars text in the modal
    */
    function countChars(event) {
        const source = event.target;

        const maxChars = source.getAttribute('data-max-char-limit');
        let strLength = source.value.length;
        let charsRemaining = (maxChars - strLength);

        //Find the next element right after the text area (i.e. the <span> element) which displays the remaining chars. and update the value
        const charCountDisplay = source.nextElementSibling;
        if (charsRemaining < 0) {
            charCountDisplay.innerHTML = '<span style="color: red;">You have exceeded the limit of ' + maxChars + ' characters</span>';
        }
        else {
            charCountDisplay.innerHTML = "Remaining: " + charsRemaining + " characters ";
        }
    }
});