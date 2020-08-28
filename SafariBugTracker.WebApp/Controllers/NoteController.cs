using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafariBugTracker.WebApp.Areas.Identity.Data;
using SafariBugTracker.WebApp.Helpers;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Services;
using static SafariBugTracker.WebApp.Models.UIAlert;
using Log = Serilog.Log;

namespace SafariBugTracker.WebApp.Controllers
{

    /// <summary>
    /// Responsible for displaying all views under the Notes directory, and for directing all CRUD operations results from those views
    /// </summary>
    [Authorize]
    public class NoteController : Controller
    {

        #region Fields, Properties, Constructor



        /// <summary>
        /// Service responsible for executing all CRUD operations related to notes entities
        /// </summary>
        private readonly NoteRepositoryService _noteRepoService;

        /// <summary>
        /// Service responsible for managing user accounts
        /// </summary>
        private readonly UserManager<UserContext> _userManager;


        public NoteController(NoteRepositoryService noteService, UserManager<UserContext> userManager)
        {
            _noteRepoService = noteService ?? throw new ArgumentNullException($" {nameof(noteService)} cannot be null");
            _userManager = userManager ?? throw new ArgumentNullException($" {nameof(userManager)} cannot be null");
        }



        #endregion
        #region GetMethods



        /// <summary>
        /// Gets a collection of the users notes, and prints the prints them to the Notes view of the dashboard
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //Use the users id to query for all the notes related to their account
            var user = await _userManager.GetUserAsync(User);
            Dictionary<string, string> searchParams = new Dictionary<string, string>()
            {
                {"PartitionKey", user.Id }
            };

            try
            {
                //Get all notes for the logged in user
                var response = await _noteRepoService.QueryTableAsync(searchParams);
                if(response.error != null)
                {
                    //Query returned an error, print the message to the screen
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, response.error));
                    return View(new List<Note>());
                }

                //No errors encountered
                return View(response.Notes);
            }
            catch (Exception e)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Internal server error, please try again later"));
                Log.Error(e, e.Message);
                return View(null);
            }
        }



        #endregion
        #region PostMethods



        /// <summary>
        /// Essentially an upsert method, as it passes the note to the repository service for saving, 
        /// or updating depending on if the record already exists in the database
        /// </summary>
        /// <param name="note">Note object containing the details of the target note</param>
        /// <returns>Task string indicating the results of the operation</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<string>> SubmitNote(Note note)
        {
            var user = await _userManager.GetUserAsync(User);
            note.PartitionKey = user.Id;
            if (note.RowKey == null)
            {
                note.RowKey = Guid.NewGuid().ToString();
                return await _noteRepoService.InsertNoteAsync(note);
            }
            else
            {
                return await _noteRepoService.UpdateNoteAsync(note);
            }
        }


        /// <summary>
        /// Sends the delete command to the note repository service, for the selected note
        /// </summary>
        /// <param name="note">Note model containing the details of the record to delete</param>
        /// <returns>Task string indicating the result of the delete operation </returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<string>> DeleteNote(Note note)
        {
            var user = await _userManager.GetUserAsync(User);
            note.PartitionKey = user.Id;
            if (note.RowKey == null)
            {
                //The JS did not catch the null before submitting the note
                Log.Warning("Javascript failure, note.RowKey was null for {@User}", user.UserName);
                return "Delete failed, please send us a message detailing the issue";
            }
            return await _noteRepoService.DeleteNoteAsync(note);
        }


        #endregion
    }//class
}//namespace