using Microsoft.Extensions.Options;
using SafariBugTracker.WebApp.Models.Settings;
using System;

namespace SafariBugTracker.WebApp.Services
{
    /// <summary>
    /// Responsible for ensuring uploaded images meet uniform requirements across the application
    /// </summary>
    public class ImageProcessingService
    {
        #region Properties, Fields, Constructor

        /// <summary>
        /// Application wide max limit of image upload size in bytes (13,631,488 bytes = 13 MB limit)
        /// </summary>
        public const int HardCodedMaxSizeBytes = 13631488;

        /// <summary>
        /// Application wide min limit of image upload size in bytes
        /// </summary>
        public const int HardCodedMinSizeBytes = 1; 

        /// <summary>
        ///  User adjustable file size limit set in the config file
        /// </summary>
        public long _maxFileSizeBytes;


        public ImageProcessingService(IOptions<ImageUploadSettings> imageSettings)
        {
            //NOTE: Mongodb has a hard coded max document size of 16 MB. Therefore, in order to accommodate large documents, the max image size will be set at a lower limit of 13mb
            int maxUploadSizeBytes = Int32.Parse(imageSettings.Value.MaxFileUploadSizeBytes);
            if(maxUploadSizeBytes > HardCodedMaxSizeBytes)
            {
                throw new ArgumentOutOfRangeException($" {nameof(ImageUploadSettings.MaxFileUploadSizeBytes)} cannot be above the 13 MB limit");
            }
            else if (maxUploadSizeBytes < HardCodedMinSizeBytes)
            {
                throw new ArgumentOutOfRangeException($" {nameof(ImageUploadSettings.MaxFileUploadSizeBytes)} cannot be below the 1 byte limit");
            }
            _maxFileSizeBytes = long.Parse(imageSettings.Value.MaxFileUploadSizeBytes);
        }



        #endregion
        #region PublicMethods



        /// <summary>
        /// Checks if the file size is within the max file size limit
        /// </summary>
        /// <returns>Bool true if the file size is less than or equal to the max value, false otherwise</returns>
        public bool IsFileSizeValid(long fileSize)
        {
            if(fileSize <= _maxFileSizeBytes)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Ensure the files mime type is actually an image by checking the MIME type
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns>Bool true if the mime type is one of the list of allowable file types, false otherwise</returns>
        public bool IsImageTypeValid(string contentType)
        {
            switch (contentType)
            {
                //Supported image mime types
                case "image/bmp":
                case "image/gif":
                case "image/jpeg":
                case "image/png":
                case "image/svg+xml":
                case "image/webp":
                case "image/tiff":
                    return true;

                //All other types will result in error return
                default:
                    return false;
            };
        }



        #endregion

    }//class
}//namespace