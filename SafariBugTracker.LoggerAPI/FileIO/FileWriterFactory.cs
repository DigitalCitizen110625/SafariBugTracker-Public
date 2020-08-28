using System;
using System.Text.RegularExpressions;

namespace SafariBugTracker.LoggerAPI.FileIO
{
    /// <summary>
    /// Factory pattern class used to instantiate specialized variants of the FileWriter base class, and IFileWriter interface 
    /// </summary>
    public static class FileWriterFactory
    {
        /// <summary>
        /// Creates a class of type IFileWriter based on the parameters
        /// </summary>
        /// <param name="fileType"> File type defining which file writer is required </param>
        public static IFileWriter CreateFileWriter(string fileName)
        {
            var fileType = ExtractFileType(fileName);
            switch (fileType)
            {
                case "txt":
                    return ThreadsafeSingletonTextWriter.Instance;

                case "json":
                    return ThreadsafeSingletonJsonWriter.Instance;

                default:
                    return ThreadsafeSingletonBinaryWriter.Instance;
            }
        }


        /// <summary>
        /// Attempts to extract the intended file type from the parameter. 
        /// Checks for txt, json, and xml file types 
        /// </summary>
        /// <param name="appSettingsFileType"> File type string </param>
        /// <returns> Matched file type </returns>
        private static string ExtractFileType(string fileType)
        {
            fileType = fileType.ToLower();

            //Matches: 
            //  • Any letter, decimal digit, or an underscore one or more times
            //  • The decimal char only once
            //  • txt, json, xml or any character except \n zero or more times
            Regex rx = new Regex(@"^\w+[.](txt|json|xml|)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match match = rx.Match(fileType);
            if(!match.Success)
            {
                //If the file type couldn't be identified, then assume its custom or binary and return unaltered
                return fileType;
            }

            //Due to the regex pattern above, the first matched group will always be at index 1. 
            //  This is a hard coded value and shouldn't be changed
            const int FILE_TYPE_INDEX = 1;
            return match.Groups[FILE_TYPE_INDEX].Value.ToString();
        }


        #region EnumAlternative


        /// <summary>
        /// Allows the class to recognize the following basic file types
        /// </summary>
        public enum FileType
        { 
            txt,
            xml,
            json,
            other
        }


        /// <summary>
        /// Attempts to convert the string file type into its corresponding enum.
        /// Defaults to a txt file type if the parse was unsuccessful.
        /// </summary>
        /// <param name="settingsFileType"> The write method defined in the appsettings.json file</param>
        /// <returns> File type as an enum </returns>
        private static FileType ConvertToFileTypeEnum(string settingsFileType)
        {
            FileType fileType;
            var parseSuccess = Enum.TryParse(settingsFileType, true, out fileType);
            if (!parseSuccess)
            {
                fileType = FileType.txt;
            }
            return fileType;
        }


        #endregion
    }//class
}//namespace