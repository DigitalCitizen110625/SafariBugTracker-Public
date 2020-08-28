using Microsoft.Extensions.Options;
using SafariBugTracker.LoggerAPI.FileIO;
using SafariBugTracker.LoggerAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SafariBugTracker.LoggerAPI.Services
{

    /// <summary>
    /// Defines the degrees of severity for a logged event
    /// </summary>
    public enum LogSeverity
    {
        Debug,
        Information,
        Warning,
        Error
    }


    public interface ILoggerService
    {
        public Log  Get();
        public ILog Get(Guid logID);
        public bool Save(dynamic log);
    }


    /// <summary>
    /// Responsible for handling all CRUD operations for the LogController
    /// </summary>
    public class LoggerService : ILoggerService
    {
        #region Fields, Properties, Constructor


        /// <summary>
        /// Contains the string values from the appsettings.json file
        /// </summary>
        private readonly LogSettings _logsettings;

        public LoggerService(IOptions<LogSettings> appSettings)
        {
            _logsettings = appSettings.Value;
        }


        #endregion
        #region PublicMethods


        /// <summary>
        /// Finds all logs
        /// </summary>
        /// <returns>Task of all logs at the storage location, or null if no records were found</returns>
        public Log Get()
        {
            return null;
        }


        /// <summary>
        /// Finds a log with a matching GUID
        /// </summary>
        /// <param name="logID">GUID of the specific log to locate</param>
        /// <returns>Task of the matching log, or null if no record was found</returns>
        public ILog Get(Guid logID)
        {
            return null;
        }


        /// <summary>
        /// Saves the logs into a specific directory, and file type based on the app settings, and log properties.
        /// </summary>
        /// <param name="log"> Single log object that will be saved</param>
        /// <returns> True if no errors were encountered, false otherwise </returns>
        public bool Save(dynamic log)
        {
            //Convert the 
            log = SetLogDate(log);

            //Extract the file type from the app settings, and create the matching file writer
            var writer = FileWriterFactory.CreateFileWriter(_logsettings.FileName);

            //Ensure the directory exists before saving the file
            var directorypath = BuildDirectorypath(_logsettings.DirectoryPath, log);
            var directory = writer.CreateDirectory(directorypath);
            if (directory == null)
            {
                throw new Exception("Error encountered during directory creation or access");
            }

            //Save the file
            var filepath = directorypath + _logsettings.FileName;
            writer.Append(filepath, log.GetKeyValuePairs);

            return true;
        }


        #endregion
        #region PrivateMethods


        /// <summary>
        /// Uses the logs details to create the directory path where the files will be saved
        /// </summary>
        /// <param name="log"> Dynamic object containing the details of the log</param>
        /// <param name="directory">Base directory path of where to save the logs</param>
        /// <returns> Complete directory path string </returns>
        private string BuildDirectorypath(string directory, dynamic log)
        {
            //Set the directory path according to the logs properties
            StringBuilder path = new StringBuilder(directory);
            path.Replace("SOURCE", log.Application);
            path.Replace("DATE", log.Timestamp);
            path.Replace("LEVEL", log.Level);
            return path.ToString();
        }


        /// <summary>
        /// Sets the date component of a single log according to the current system time
        /// </summary>
        /// <param name="log"> Dynamic object which will have it's date/timestamp property set to dd-MM-yyyy</param>
        /// <returns>Single log with a set date property</returns>
        private dynamic SetLogDate(dynamic log)
        {
            log.Timestamp = DateTime.Now.ToString("dd-MM-yyyy");
            return log;
        }


        /// <summary>
        /// Iterates the collection of logs, and sets their date/timestamp component to the current system time
        /// </summary>
        /// <param name="logs">Collection of logs to set their date property</param>
        /// <returns> Enumerable collection of logs </returns>
        private IEnumerable<ILog> SetLogDate(dynamic[] logs)
        {
            foreach(var log in logs)
            {
                yield return SetLogDate(log);
            }
        }


        /// <summary>
        /// Generates a new GUID for the passed in log object
        /// </summary>
        /// <param name="log">Log object to generate a new GUID</param>
        /// <returns>ILog with it's GUID property filled in</returns>
        private ILog GenerateIDForLog(ILog log)
        {
            log.Id = Guid.NewGuid().ToString();
            return log;
        }



        #endregion
    }//class
}//namespace