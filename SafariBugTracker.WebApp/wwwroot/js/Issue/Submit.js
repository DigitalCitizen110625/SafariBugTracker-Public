$(document).ready(function (e) {

    /*******************************
    *
    *  Initializers
    *
    *******************************/


    //Define the drop down options for the issue/bug input fields
    var platforms = ["Cloud", "Desktop", "Laptop", "Mobile", "Service", "Tablet", "Web"];
    var products = ["Arc-Light", "AutoDoc", "SafariBugTracker", "ServerlessLogger"];
    var versions = ["1.0", "1.1", "1.2", "1.3", "1.4", "1.5",];
    var categories = ["Account", "Install", "Patch", "General", "Crashes", "Network", "Performance", "Visuals", "Textual Content", "Feature Request", "Other"];

    /*******************************
    *
    *  Selectors
    *
    *******************************/


    //Get the input fields used when submitting issues/bug reports
    const platformInputField = document.getElementById("platformInput");
    const productInputField = document.getElementById("productInput");
    const versionInputField = document.getElementById("versionInput");
    const categoryInputField = document.getElementById("categoryInput");
    const userSearchField = document.getElementById("assignedToUserSearch");


    /*******************************
    *
    *  Event Handlers
    *
    *******************************/


    //Create an onclick event for counting the remaining chars in each text area
    $('textarea').keydown(countChars);
    $('textarea').keyup(countChars);

    //Create onclick event for auto seraching for a matching account as the user types
    $('#assignedToUserSearch').keyup(ajaxUserSearch);


    /*******************************
    *
    *  Functions
    *
    *******************************/

    autoSearch(platformInputField, platforms);
    autoSearch(productInputField, products);
    autoSearch(versionInputField, versions);
    autoSearch(categoryInputField, categories);

    /*
    *  FUNCTION      : ajaxUserSearch
    *  DESCRIPTION   : Auto searches for a matching account as the user enters characters into the input field
    */
    function ajaxUserSearch()
    {
        //All display names must be min 3 chars long
        if ($('#assignedToUserSearch').val().length > 3)
        {
            var data = userSearchField.value;
            $.ajax({
                method: "get",
                data: { searchString: data},
                cache: false,
                url: "/Account/SearchUsers",
                success: function (data)
                {
                    //Return object (data) should be a collection of user data in the form of
                    // string firstName, string lastName, string displayName, string Id

                    //Create a DIV element that will contain the items (i.e. auto search values)
                    var searchResultContainer = createSuggestionDropDown(userSearchField);

                    //Append the DIV element as a child of the autocomplete container
                    //In this case, "this" refers to the <input> reference we passed in
                    userSearchField.parentNode.appendChild(searchResultContainer);

                    var autoSearchItemElement, i;
                    for (i = 0; i < data.length; i++)
                    {
                        autoSearchItemElement = document.createElement("div");


                        var userString = data[i].firstName + " " + data[i].lastName + "  (" + data[i].displayName + ")"
                        autoSearchItemElement.innerHTML += userString.substr(userSearchField.length);
                        autoSearchItemElement.innerHTML += "<input type='hidden' value='" + userString + "'>";
                        autoSearchItemElement.innerHTML += "<input type='hidden' value='" + data[i].id + "'>";

                        //Execute a function when someone clicks on the item value (div element)
                        autoSearchItemElement.addEventListener("click", function (e)
                        {
                            //Insert the value for the autocomplete text field
                            userSearchField.value = this.getElementsByTagName("input")[0].value;

                            let assignedToIDField = document.getElementById('assignedToUserID');
                            assignedToIDField.value = this.getElementsByTagName("input")[1].value;
                        });
                        searchResultContainer.appendChild(autoSearchItemElement);
                    }
                },
                error: function (data)
                {
                    //An error return indicates a db connection issue
                    console.log(data);
                }
            });
        }
    }


    /*
    *  FUNCTION      : countChars
    *  DESCRIPTION   : Counts the number of chars in the target element, and updates the remaining chars text in the modal
    */
    function countChars(event)
    {
        const source = event.target;

        const maxChars = source.getAttribute('data-max-char-limit');
        let strLength = source.value.length;
        let charsRemaining = (maxChars - strLength);

        //Find the next element right after the text area (i.e. the <span> element) which displays the remaining chars. and update the value
        const charCountDisplay = source.nextElementSibling;
        if (charsRemaining < 0)
        {
            charCountDisplay.innerHTML = '<span style="color: red;">You have exceeded the limit of ' + maxChars + ' characters</span>';
        }
        else
        {
            charCountDisplay.innerHTML = "Remaining: " + charsRemaining + " characters ";
        }
    }


    /*
    *  FUNCTION      : autoSearch
    *  DESCRIPTION   : Auto searches through a list of elements, and populates text, and search type inputs with the matched strings
    *  SOURCE        : Original script was provided by https://www.w3schools.com/howto/howto_js_autocomplete.asp
    */
    function autoSearch(inputElement, autoSearchList)
    {
        //The autoSearch function takes two arguments, the text field element 
        //  and an array of possible autocompleted values

        //Create an onclick event whenever a character is entered into the input field
        inputElement.addEventListener("input", function (e)
        {
            //Close any open lists of autocompleted values
            closeAllLists();

            //Ensure the input field has something in it before continuing
            var inputContents = this.value;
            if (!inputContents) { return false; }

            //Create a DIV element that will contain the items (i.e. auto search values)
            var searchResultContainer = createSuggestionDropDown(this);

            //Append the DIV element as a child of the autocomplete container
            //In this case, "this" refers to the <input> reference we passed in
            this.parentNode.appendChild(searchResultContainer);

            //For each item in the array...
            var autoSearchItemElement;
            var i;
            for (i = 0; i < autoSearchList.length; i++)
            {
                //Check if the item starts with the same letters as the text field value
                if (autoSearchList[i].substr(0, inputContents.length).toUpperCase() == inputContents.toUpperCase())
                {
                    //Create a DIV element for each matching element
                    autoSearchItemElement = document.createElement("DIV");

                    //Make the matching letters bold
                    autoSearchItemElement.innerHTML = "<strong>" + autoSearchList[i].substr(0, inputContents.length) + "</strong>";
                    autoSearchItemElement.innerHTML += autoSearchList[i].substr(inputContents.length);

                    //Insert a input field that will hold the current array item's value
                    autoSearchItemElement.innerHTML += "<input type='hidden' value='" + autoSearchList[i] + "'>";

                    //Execute a function when someone clicks on the item value (DIV element)
                    autoSearchItemElement.addEventListener("click", function (e)
                    {
                        //Insert the value for the autocomplete text field
                        inputElement.value = this.getElementsByTagName("input")[0].value;

                        //Close the list of autocompleted values,(or any other open lists of autocompleted values
                        closeAllLists();
                    });
                    searchResultContainer.appendChild(autoSearchItemElement);
                }
            }
        });

        
        /*
        *  FUNCTION      : closeAllLists
        *  DESCRIPTION   : Close all autocomplete lists in the document, except the one passed as an argument
        *  SOURCE        : Original script was provided by https://www.w3schools.com/howto/howto_js_autocomplete.asp
        */
        function closeAllLists(elmnt)
        {
            var x = document.getElementsByClassName("autocomplete-items");
            for (var i = 0; i < x.length; i++)
            {
                if (elmnt != x[i] && elmnt != inputElement)
                {
                    x[i].parentNode.removeChild(x[i]);
                }
            }
        }


        //Execute a function when someone clicks in the document
        document.addEventListener("click", function (e)
        {
            closeAllLists(e.target);
        });
    }


    /*
    *  FUNCTION      : createSuggestionDropDown
    *  DESCRIPTION   : Creates a new div element with prefilled id, and class attributes. 
    *                  The div element will act as a container to hold all auto search suggestions
    *                  for it's parent input element
    */
    function createSuggestionDropDown(inputElement) {
        var searchResultContainer = document.createElement("DIV");
        searchResultContainer.setAttribute("id", inputElement.id + "autocomplete-list");
        searchResultContainer.setAttribute("class", "autocomplete-items");
        return searchResultContainer;
    }


});