$(document).ready(function (e) {

    /*******************************
    *
    *  Selectors
    *
    *******************************/
    let collapsiblePanel = document.getElementById("collapsiblePanel");
    let addUserSubmitButton = document.getElementById("addUserSubmitButton");


    /*******************************
    *
    *  Event Handlers
    *
    *******************************/

    //The collapsiblePanel element may be null if the user accessing the teams page, does not have the Admin,
    //  or ProjectManager role. Thus we need to check before assigning the event
    if (collapsiblePanel)
    {
        collapsiblePanel.addEventListener("click", function ()
        {

            //Attach a css class to the clicked button, to indicate it's active state
            this.classList.toggle("active-panel");

            //Open the panel to reveal the content in the next element group
            //var content = this.nextElementSibling;
            const content = document.getElementById("content");
            if (content.style.maxHeight)
            {
                content.style.maxHeight = null;
            }
            else
            {
                content.style.maxHeight = content.scrollHeight + "px";
            }
        });
    }


});

