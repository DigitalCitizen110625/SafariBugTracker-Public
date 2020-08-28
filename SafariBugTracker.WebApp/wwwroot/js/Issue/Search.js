$(document).ready(function (e) {

    /*******************************
    *
    *  Initializers
    *
    *******************************/



    //Creates a date picker control once the user clicks on the item with the correct selector.
    //  Dates will be entered in the dd - mm - yy format
    //SOURCE: https://jqueryui.com/datepicker/
    $('*[data-calendar]').datepicker({ dateFormat: 'dd-mm-yy' });
    $('*[data-calendar]').datepicker("option", "showAnim", "slideDown");



    /*******************************
    *
    *  Event Handlers
    *
    *******************************/



    //Changes the text of the drop down to match the selected option, and creates a second
    //  input field if the user selects the "Between" option
    $(".dropdown-menu a.dropdown-item").click(function () {

        //Change the text of the drop down button to match the selected search option
        const selection = $(this).text()
        $(this).parent().prev(".dropdown-toggle").text(selection);


        //Traverse up the DOM hierarchy, and get the row which contains the related input fields
        //  Then drill down to the form groups containing the inputs
        const parentFormRow = $(this).closest('.form-row');
        const formGroups = parentFormRow.find('.form-group')
        const startRangeInput = parentFormRow.find('input').first();


        //Both the date and version inputs have a separate hidden input field for storing the selected search option
        //  i.e. matching, greater than, less than, between etc.
        const hiddenInput = formGroups.first().find("input[type=hidden]");
        hiddenInput.eq(0).val(selection);


        //If the user selects the between option, then we want to create another
        //  matching input field beside the current one
        if (selection == "Between")
        {         
            //Get the first form group and make it half as wide
            formGroups.first().removeClass('col-md-12').addClass('col-md-6')

            //Remember that eq(1) is actually the second element since formGroups is an array of
            //  jQuery objects
            formGroups.eq(1).removeClass('d-none');
            startRangeInput.attr("placeholder", "Start");
        }
        else
        {
            //Resize the first form group to be max length, and hide the second group
            formGroups.first().removeClass('col-md-6').addClass('col-md-12')
            formGroups.eq(1).addClass('d-none');

            //Clear the value of the inputs in the second group
            formGroups.eq(1).children().eq(1).val("");   

            //Reset the placeholder attribute of the first input
            startRangeInput.attr("placeholder", "");
        }
    });

});