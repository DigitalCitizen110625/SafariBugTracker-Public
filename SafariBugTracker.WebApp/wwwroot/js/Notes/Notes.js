$(document).ready(function () {

    /*******************************
    *
    *  Selectors
    *
    *******************************/


    let clickedNote = null;

    const noteCards = document.getElementsByClassName("sticky-note");


    /*******************************
    *
    *  Event Handlers
    *
    *******************************/


    //Create an onclick event for populating the modal window with the details of the clicked note
    document.getElementById('NoteModalTextArea').addEventListener("keydown", countChars);
    document.getElementById('NoteModalTextArea').addEventListener("keyup", countChars);


    //Create an onclick event for populating the modal window with the details of the clicked note
    if (noteCards) {
        for (let note of noteCards) {
            note.addEventListener("click", stickyNoteClick);
        }
    }

    //Create a new blank sticky note when the button is clicked
    $("#createNote").click(function () {
        const stickyNote = buildStickyNote();
        stickyNote.addEventListener("click", stickyNoteClick);
        $(this).before(stickyNote);
    });


    //Save, or update the upon clicking the corresponding button
    $("#SaveNote").click(function () {
        var toastHeader = document.getElementById('toast-header');

        const note = getNoteDetails();
        $.ajax({
            method: "POST",
            data: note,
            url: "/Note/SubmitNote",
            success: function (data)
            {

                //Data is the string returned from the controller, indicating the result of the save operation
                toastHeader.innerText = data;

                //Style the toast message to look like the default bootstrap alert-success message
                toastHeader.classList.remove("note-error");
                toastHeader.classList.add("note-success");

                //Updates the displayed text for the note card
                updateNoteCard(note);

                //Remove the infromational heading if it's still present on the page
                let noNotesHeading = document.getElementById("noNotesHeading");
                removeElement(noNotesHeading);

                $('.toast').toast('show');
            },
            error: function (data) {
                toastHeader.innerText = data;

                //Style the toast message to look like the default bootstrap alert-error message
                toastHeader.classList.remove("note-success");
                toastHeader.classList.add("note-error");

                $('.toast').toast('show');
            }
        });

    })


    //Delete the note when the corresponding delete button is clicked
    $("#DeleteNote").click(function () {
        const note = getNoteDetails();

        //If the note have been saved to the database, then it should have a row key,
        //  if it doesn't, then it was recently created, and just needs to be removed
        //  from the view
        if (note.RowKey == null || note.RowKey == "") {
            removeElement(clickedNote);
            clickedNote = null;
        }
        else {

            //The note has a row key, so we know it's been saved to the database and must be removed there as well
            $.ajax({
                method: "POST",
                data: note,
                url: "/Note/DeleteNote",
                success: function () {
                    removeElement(clickedNote);
                    clickedNote = null;
                    $('#NoteModal').modal('hide')
                },
                error: function (data) {
                    //String returned from the controller, indicating the result of the save operation
                    toastHeader.innerText = data;

                    //Style the toast message to look like the default bootstrap alert-error message
                    toastHeader.classList.remove("note-success");
                    toastHeader.classList.add("note-error");

                    $('.toast').toast('show');
                }
            });
        }
    })



    /*******************************
    *
    *  Functions
    *
    *******************************/



    /*
    *  FUNCTION      : stickyNoteClick
    *  DESCRIPTION   : Populates the popup modal with the contents of the clicked sticky note
    */
    function stickyNoteClick(e)
    {
        const clickedElement = e.target;
        let container = clickedElement;

        //Each note is contained in a parent with the sticky-note class
        if (!clickedElement.classList.contains("sticky-note"))
        {
            container = $(clickedElement).parents('.sticky-note').first().get(0);
        }

        //Get the text content of the selected note
        const noteTitle = container.querySelector("h5").innerText;
        const noteContent = container.querySelector("p").innerText;
        const noteId = container.querySelector("div [data-rowkey]").innerText;
        const noteTimestamp = container.querySelector("div [data-timestamp]").innerText;


        //Set the modals title and body content to that of the note
        const noteModal = document.getElementById("NoteModal");
        noteModal.querySelector("#NoteModalTitle").value = noteTitle;
        noteModal.querySelector("#NoteModalTextArea").value = noteContent;
        noteModal.querySelector("#NoteModalId").innerText = noteId;

        //The timestamp has been disabled for the moment
        //noteModal.querySelector("#NoteModalTimestamp").innerText = "Created: " + noteTimestamp;


        updateModalRemainingChars(document.getElementById('NoteModalTextArea'));


        $(noteModal).modal('show');
        clickedNote = container;
    }


    /*
     *  FUNCTION      : buildStickyNote
     *  DESCRIPTION   : Assembles the html elements that make up a sticky note
     */
    function buildStickyNote() {
        /* Final sticky note will look like this:
        *
        * <div class="card sticky-note">
        *     <div class="note-inner-container">
        *           <div class="card-header">
        *               <h5>Note Title</h5>
        *           </div>
        *           <div class="card-body">
        *               <p class="card-text">Note Content...</p>
        *           </div>
        *           <div class="d-none">
        *               <div>RowKey</div>
        *               <div>TimeStamp</div>
        *           </div>
        *       </div>
        *  </div>
        */
        var stickyNote = document.createElement("div");
        stickyNote.classList.add('card');
        stickyNote.classList.add('sticky-note');

        var stickyNoteInnerContainer = document.createElement("div");
        stickyNoteInnerContainer.classList.add('note-inner-container');

        stickyNoteInnerContainer.appendChild(document.createElement("div"));
        stickyNoteInnerContainer.appendChild(document.createElement("div"));
        stickyNoteInnerContainer.appendChild(document.createElement("div"));

        stickyNoteInnerContainer.children[0].classList.add('card-header');
        stickyNoteInnerContainer.children[0].appendChild(document.createElement("h5"));
        stickyNoteInnerContainer.children[1].classList.add('card-body');

        const noteText = document.createElement("p");
        noteText.classList.add("card-text");
        noteText.classList.add("truncate");
        stickyNoteInnerContainer.children[1].appendChild(noteText);

        stickyNoteInnerContainer.children[2].classList.add('d-none');
        const rowkeyDiv = document.createElement("div");
        rowkeyDiv.setAttribute("data-rowkey", "");
        stickyNoteInnerContainer.children[2].appendChild(rowkeyDiv);

        const timestampDiv = document.createElement("div");
        timestampDiv.setAttribute("data-timestamp", "");

        //The time stamp of the note will be set by the Azure table storage service when submitted
        timestampDiv.innerText = "";
        stickyNoteInnerContainer.children[2].appendChild(timestampDiv);

        stickyNote.appendChild(stickyNoteInnerContainer);
        return stickyNote;
    }


    /*
    *  FUNCTION      : updateNoteCard
    *  DESCRIPTION   : Updates the displayed text in the note card, to the values in the note object
    */
    function updateNoteCard(note) {
        clickedNote.querySelector("h5").innerText = note.Title;
        clickedNote.querySelector("p").innerText = note.Content;
    }


    /*
    *  FUNCTION      : countChars
    *  DESCRIPTION   : Passes the event target to the updateModalRemainingChars function where the char counting occurs
    */
    function countChars(event) {
        updateModalRemainingChars(event.target);
    }


    /*
    *  FUNCTION      : updateModalRemainingChars
    *  DESCRIPTION   : Counts the number of chars in the target element, and updates the remaining chars text in the modal
    */
    function updateModalRemainingChars(source)
    {
        const maxChars = source.getAttribute('data-max-char-limit');
        let strLength = source.value.length;
        let charsRemaining = (maxChars - strLength);

        const charCountDisplay = document.getElementById('NoteModalCharRemaining');
        if (charsRemaining < 0) {
            charCountDisplay.innerHTML = '<span style="color: red;">You have exceeded the limit of ' + maxChars + ' characters</span>';
        }
        else {
            charCountDisplay.innerHTML = "Remaining: " + charsRemaining + " characters ";
        }
    }


    /*
    *  FUNCTION      : generateUUID
    *  DESCRIPTION   : RFC4122 version 4 compliant GUID number generator
    *  SOURCE        : https://stackoverflow.com/questions/105034/how-to-create-guid-uuid
    */
    function generateUUID() {
        var d = new Date().getTime();

        //Time in microseconds since page-load or 0 if unsupported
        var d2 = (performance && performance.now && (performance.now() * 1000)) || 0;
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            //random number between 0 and 16
            var r = Math.random() * 16;
            if (d > 0) {
                //Use timestamp until depleted
                r = (d + r) % 16 | 0;
                d = Math.floor(d / 16);
            }
            else {
                //Use microseconds since page-load if supported
                r = (d2 + r) % 16 | 0;
                d2 = Math.floor(d2 / 16);
            }
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    }

    /*
    *  FUNCTION      : replaceAll
    *  DESCRIPTION   : Removes all instances of str1 from the string, and replaces them with str2
    */
    String.prototype.replaceAll = function (str1, str2, ignore) {
        return this.replace(new RegExp(str1.replace(/([\/\,\!\\\^\$\{\}\[\]\(\)\.\*\+\?\|\<\>\-\&])/g, "\\$&"), (ignore ? "gi" : "g")), (typeof (str2) == "string") ? str2.replace(/\$/g, "$$$$") : str2);
    }


    /*
    *  FUNCTION      : getNoteDetails
    *  DESCRIPTION   : Extracts the values from the modal popup, and packages them as a Note object
    */
    function getNoteDetails() {
        const title = document.getElementById("NoteModalTitle").value;
        const id = document.getElementById("NoteModalId").innerText;
        const content = document.getElementById("NoteModalTextArea").value;
        var timestamp = document.getElementById("NoteModalTimestamp").innerText;

        //The NoteModalTimestamp value also includes the heading sub string "Created: ".
        //We want to remove this leaving just the timestamp value
        var timestamp = timestamp.replaceAll("Created: ", "");
        return { "PartitionKey": "", "RowKey": id, "Title": title, "Content": content, "Timestamp": timestamp };
    }


    /*
    *  FUNCTION      : removeElement
    *  DESCRIPTION   : Takes an html element and removes it from the webpage, if it's not null or invalid
    */
    function removeElement(element)
    {
        if (element) {
            element.remove();
        }
    }
});