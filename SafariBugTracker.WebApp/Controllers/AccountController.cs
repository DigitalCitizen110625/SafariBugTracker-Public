using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafariBugTracker.WebApp.Areas.Identity.Data;
using SafariBugTracker.WebApp.Extensions;
using SafariBugTracker.WebApp.Helpers;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Models.ViewModels;
using SafariBugTracker.WebApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SafariBugTracker.WebApp.Models.UIAlert;

namespace SafariBugTracker.WebApp.Controllers
{

    /// <summary>
    /// Responsible for handling all routing and logic required for the account related views
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {

        #region Fields, Properties, Constructor

        /// <summary>
        /// Service responsible for managing user accounts
        /// </summary>
        private readonly UserManager<UserContext> _userManager;

        /// <summary>
        /// Service responsible for performing validation checks on uploaded images
        /// </summary>
        private readonly ImageProcessingService _imageProcessor;

        public AccountController(UserManager<UserContext> userManager, ImageProcessingService imageProcessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException($" {nameof(userManager)} cannot be null");
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException($" {nameof(imageProcessor)} cannot be null");
        }



        #endregion
        #region GetMethods


        /// <summary>
        /// Presents the profile/account view for the selected user
        /// </summary>
        [HttpGet]
        public IActionResult ViewAccount()
        {
            return View();
        }


        /// <summary>
        /// Presents the profile/account edit view for the logged in user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            //Get the logged in user, and display their account information in the view
            var userContext = await _userManager.GetUserAsync(User);
            if (userContext== null)
            {
                return RedirectToAction("NotFound", "Dashboard");
            }

            var userViewModel = userContext.ToUserViewModel();
            return View(userViewModel);
        }



        #endregion
        #region PostMethods


        /// <summary>
        /// Searches the user database for an account matching the search query
        /// </summary>
        /// <param name="project">Project the searching user is part of. Users can only search for others in the same project</param>
        /// <param name="searchString">Name, or ID used to find a matching account</param>
        /// <returns>Task of the search results containing a list of matching user display names</returns>
        [HttpGet]
        public IQueryable<object> SearchUsers(string searchString)
        {
            // We want to do a general keyword search and return results that contain the search string
            var searchResult = _userManager.Users.Where(_ => 
                _.Email.Contains(searchString) ||
                _.DisplayName.Contains(searchString) ||
                _.FirstName.Contains(searchString) ||
                _.LastName.Contains(searchString))
                .Select(_ => new { firstName = _.FirstName, lastName = _.LastName, displayName = _.DisplayName, Id = _.Id});
            return searchResult;
        }


        /// <summary>
        /// Allows the user to edit their account details in the profile view
        /// </summary>
        /// <returns>Result of the edit operation</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel viewModel, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                //User filled in the form incorrectly -> reload the page and print the errors
                return View(viewModel);
            }

            if(image != null)
            {
                if (!_imageProcessor.IsFileSizeValid(image.Length))
                {
                    //image size was above the max allowable limit
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "File size cannot exceed 12 mb"));
                    return View(viewModel);
                }

                if (!_imageProcessor.IsImageTypeValid(image.ContentType))
                {
                    //image file type was not among the list of accepted mime types
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "File type must be an image (e.g. bmp, gif, jpeg, png, svg, webp, tiff"));
                    return View(viewModel);
                }

                //No errors detected
                viewModel.ProfileImage = SafariBugTracker.WebApp.Helpers.TypeConverter.ConvertToBytes(image);
                viewModel.ContentType = image.ContentType;
            }


            //Save all updated/valid values to the users profile
            var user = await _userManager.FindByNameAsync(viewModel.UserName);
            if (user == null)
            {
                //Somehow the user managed to access the edit account page, for an account that doesn't exist 
                // so lets redirect them to the teams page
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Selected Account Was Not Found In The Database"));
                return RedirectToAction("Team", null);
            }

            user.FromViewModel(viewModel);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            { 
                //Database returned errors with account update, so lets display those errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                //Reprint the page with the new errors
                return View(viewModel);
            }


            UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Profile Updated"));
            return View(viewModel);
        }


        /// <summary>
        /// Deletes the selected user from the database
        /// </summary>
        /// <param name="userId">ID of the record to delete</param>
        /// <returns>Reprints the view if any errors were detected, otherwise, it redirects to the default view of the controller</returns>
        [HttpPost]
        public async Task<IActionResult> Delete(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {

                //The user has somehow accessed the edit page for an account that doesn't exist
                // Transfer them back to the default not found page
                return RedirectToAction("NotFound", "Dashboard");
            }

            IdentityResult result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Something went wrong while deleting this user. Please contact your administrator, or send us a message"));
                return View();
            }

            
            //Check if a user just deleted their own account, or it was done by a project manager, or admin
            UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Account Successfully Deleted"));
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                //The logged in user deleted their account so they can no longer access the dashboard
                return RedirectToAction("Index", "Home");
            }

            //A project manager, or admin deleted the account so they're good to continue
            var previousPage = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(previousPage))
            {
                return Redirect(previousPage);
            }
            return RedirectToAction("Index", "Dashboard");
        }


        #endregion
    }//class
}//namespace