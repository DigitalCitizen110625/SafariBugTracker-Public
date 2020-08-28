using SafariBugTracker.WebApp.Areas.Identity.Data;
using SafariBugTracker.WebApp.Models.ViewModels;

namespace SafariBugTracker.WebApp.Extensions
{
    /// <summary>
    /// Contains a collection of extension methods used for providing additional functionality to the UserViewModel, and UserContext models
    /// </summary>
    public static class UserExtensions
    {

        /// <summary>
        /// Copies the values from the view model, to the target user context object, if the values aren't null
        /// </summary>
        /// <param name="user">Receiving UserContext object that will have it's property values updated</param>
        /// <param name="sourceModel">Source object used to copy it's values to the userContext object</param>
        public static void FromViewModel(this UserContext user, UserViewModel sourceModel)
        {
            //only assign the new values if they aren't null
            user.FirstName = sourceModel.FirstName ?? user.FirstName;
            user.LastName = sourceModel.LastName ?? user.LastName;
            user.UserName = sourceModel.UserName ?? user.UserName;
            user.DisplayName = sourceModel.DisplayName ?? user.DisplayName;
            user.Email = sourceModel.Email ?? user.Email;
            user.Password = sourceModel.Password ?? user.Password;
            user.Project = sourceModel.Project ?? user.Project;
            user.Team = sourceModel.Team ?? user.Team;
            user.Position = sourceModel.Position ?? user.Position;
            user.ProfileImage = sourceModel.ProfileImage ?? user.ProfileImage;
            user.ContentType = sourceModel.ContentType ?? user.ContentType;
        }


        /// <summary>
        /// Creates a new view model, and copies the user contexts property values to it
        /// </summary>
        /// <param name="user">UserContext object containing the details of the users account</param>
        /// <returns>Object of type UserViewModel</returns>
        public static UserViewModel ToUserViewModel(this UserContext user)
        {
            return new UserViewModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Password = user.Password,
                Project = user.Project,
                Team = user.Team,
                Position = user.Position,
                ProfileImage = user.ProfileImage,
                ContentType = user.ContentType
            };
        }

    }//class
}//namespace