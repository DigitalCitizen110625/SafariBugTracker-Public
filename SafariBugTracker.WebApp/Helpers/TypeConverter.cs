using Microsoft.AspNetCore.Http;
using System.IO;

namespace SafariBugTracker.WebApp.Helpers
{
    /// <summary>
    /// Contains methods used for converting objects/variables from one type to another
    /// </summary>
    static public class TypeConverter
    {

        /// <summary>
        /// Convert an uploaded file from an IFormFile type, to a byte array
        /// </summary>
        /// <param name="image">Image to be converted to a byte array</param>
        /// <returns>Byte array containing the image</returns>
        static public byte[] ConvertToBytes(IFormFile image)
        {
            byte[] CoverImageBytes = null;
            BinaryReader reader = new BinaryReader(image.OpenReadStream());
            CoverImageBytes = reader.ReadBytes((int)image.Length);
            return CoverImageBytes;
        }

    }//class
}//namespace