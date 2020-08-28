using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using static SafariBugTracker.WebApp.Models.UIAlert;

namespace SafariBugTracker.WebApp.Controllers
{

    /// <summary>
    /// Responsible for handling user administration by accounts with admin, or project manager roles
    /// </summary>
    [Authorize]
    public class UserManagement : Controller
    {

        #region Fields, Properties, Constructors 



        /// <summary>
        /// Service responsible for managing user accounts
        /// </summary>
        private readonly UserManager<UserContext> _userManager;

        /// <summary>
        /// Service responsible for managing Identity roles
        /// </summary>
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Service responsible for performing validation checks on uploaded images
        /// </summary>
        private readonly ImageProcessingService _imageProcessor;

        public UserManagement(UserManager<UserContext> userManager, RoleManager<IdentityRole> roleManager, ImageProcessingService imageProcessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException($" {nameof(userManager)} cannot be null");
            _roleManager = roleManager ?? throw new ArgumentNullException($" {nameof(roleManager)} cannot be null");
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException($" {nameof(imageProcessor)} cannot be null");
        }



        #endregion
        #region GetMethods



        /// <summary>
        /// Prints the default view for the Teams page of the dashboard. Populates the page with
        /// a list of all users in the same team as the logged in user
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Team()
        {
            //Get the team designation for the logged in user, and find all users with the same team
            var user = await _userManager.GetUserAsync(User);

            if (user.Team == null)
            {
                return View(null);
            }

            var queryableTeammates = _userManager.Users.Where(_=> _.Team == user.Team);
            var teamList = new List<UserViewModel>();
            foreach(var teammate in queryableTeammates)
            {
                var viewModel = teammate.ToUserViewModel();

                //Each account can have multiple roles assigned to it, but we only want the first one since 
                //  will the specifications only allow for a single role
                viewModel.Role = (await _userManager.GetRolesAsync(teammate))[0] ?? "Role Not Set";
                teamList.Add(viewModel);
            }

            return View(teamList);
        }


        /// <summary>
        /// Prints the default view for the Edit User page
        /// </summary>
        /// <param name="id">Id used to find the account</param>
        /// <returns>Populates the view  with the selected users details, else, it redirects to the "NotFound" page if the selected user doesn't exist in the database</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Dashboard");
            }

            return View(user.ToUserViewModel());
        }



        #endregion
        #region PostMethods



        /// <summary>
        /// Adds the new account to the database, or reprints the page with any errors
        /// </summary>
        /// <param name="userViewModel">View model containing the new values for the user to add</param>
        /// <returns>Task indicating the result of the operation</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager")]
        public async Task<IActionResult> AddUser(UserViewModel userViewModel)
        {
            //Note: The data is posted from the Team view, so if you want to return the same view, you must redirect instead of return View(), as there isn't a view with a matching name AddUser
            userViewModel.UserName = userViewModel.Email;
            if (!ModelState.IsValid)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Account details were invalid, please ensure all required feilds are filled in"));
                return RedirectToAction("Team", null);
            }

            var newUser = new UserContext();
            newUser.FromViewModel(userViewModel);

            //Create the user account in the database, and report on any errors
            IdentityResult result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Error encountered while creating new account. Please contact your administrator, or send us a message"));
                return RedirectToAction("Team", null);
            }

            //User account was created successfully => redirect back to the default View of the controller
            UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "User Registered Successfully"));
            return RedirectToAction("Team", null);
        }


        /// <summary>
        /// Updates the details of the target user account, according to the values provided in the view model
        /// </summary>
        /// <param name="userViewModel">View model containing the new values for the user to update</param>
        /// <returns>Task indicating the result of the operation</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager")]
        public async Task<IActionResult> EditUser(UserViewModel userViewModel, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }

            if (image != null)
            {
                if (!_imageProcessor.IsFileSizeValid(image.Length))
                {
                    //image size was above the max allowable limit
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "File size cannot exceed 12 mb"));
                    return View(userViewModel);
                }

                if (!_imageProcessor.IsImageTypeValid(image.ContentType))
                {
                    //image file type was not among the list of accepted mime types
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "File type must be an image (e.g. bmp, gif, jpeg, png, svg, webp, tiff"));
                    return View(userViewModel);
                }

                //No errors detected
                userViewModel.ProfileImage = SafariBugTracker.WebApp.Helpers.TypeConverter.ConvertToBytes(image);
                userViewModel.ContentType = image.ContentType;
            }

            //Find the target user
            var user = await _userManager.FindByNameAsync(userViewModel.UserName);
            if(user == null)
            {
                //Somehow the user managed to access the edit account page, for an account that doesn't exist 
                // so lets redirect them to the teams page
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Selected Account Was Not Found In The Database"));
                return RedirectToAction("Team", null);
            }

            //Update the user account
            user.FromViewModel(userViewModel);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return View(result);
                }
            }

            //User account was updated successfully => redirect back to the default View of the controller
            UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Account Successfully Updated"));
            return RedirectToAction("Team", null);
        }


        /// <summary>
        /// Updates the selected user accounts role according to the values submitted by the model
        /// </summary>
        /// <param name="viewModel">Model containing the target users id, and the new role name</param>
        /// <returns>Task indicating the result of the operation</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, Project Manager")]
        public async Task<IActionResult> EditUserRoles(RoleViewModel viewModel)
        {
            //Note: The data is posted from the Team view, so if you want to return the same view, you must redirect instead of return View(), as there isn't a view with a matching name EditUserRoles
            if (!ModelState.IsValid)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Role details were invalid, please ensure all fields area filled in"));
                return RedirectToAction("Team");
            }


            //Admin is a reserved keyword, so don't allow anyone to include it in their role
            if (viewModel.Role.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Roles may not contain the keyword \"admin\" "));
                return RedirectToAction("Team");
            }


            //Find the target user
            var user = await _userManager.FindByIdAsync(viewModel.UserId);
            if (user == null)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "The selected account was not found in the database"));
                return RedirectToAction("Team");
            }

            //Make sure they're not already registered with the same role
            if (await _userManager.IsInRoleAsync(user, viewModel.Role))
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Info, "User is already registered with that role. Update canceled"));
                return RedirectToAction("Team");
            }

            //Try and see if the role is already registered with the role manager
            var role = await _roleManager.FindByNameAsync(viewModel.Role);
            if(role == null)
            {
                //Role was not found, so lets create it first
                role = new IdentityRole
                {
                    Name = viewModel.Role
                };

                IdentityResult roleCreationResutlt = await _roleManager.CreateAsync(role);

                if (!roleCreationResutlt.Succeeded)
                {
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Unable to register new role. Please send us a message regarding the error"));
                    return RedirectToAction("Team");
                }
            }


            //Now we need to clear all roles from the user as each account can only have a single role
            var roles = await _userManager.GetRolesAsync(user);
            if(roles != null)
            {
                //User already has roles assigned to them
                var roleRemoval = await _userManager.RemoveFromRolesAsync(user, roles.ToArray());
                if(!roleRemoval.Succeeded)
                {
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Role update failed, please contact your admin, or send us a message"));
                    return RedirectToAction("Team");
                }
            }

            //Add the new role to the account
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Role update failed. Please contact your admin, or send us a message"));
                return View(viewModel);
            }


            //Success!
            UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Account Updated. Please ensure they re-login for the changes to take affect"));
            return RedirectToAction("Team");
        }


        #endregion
    }//class
}//namespace