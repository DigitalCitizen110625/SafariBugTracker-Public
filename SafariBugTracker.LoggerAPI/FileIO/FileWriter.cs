using System;
using System.Collections.Generic;
using System.IO;

namespace SafariBugTracker.LoggerAPI.FileIO
{
    public interface IFileWriter
    {
        public void Overwrite<T>(string path, object objToSerialize);
        public void Overwrite<T>(string path, object[] objsToSerialize);
        public void Append(string path, IEnumerable<KeyValuePair<string, object>> kvpCollection);
        public void Append<T>(string path, object objToSerialize);
        public void Append<T>(string path, object[] objsToSerialize);
        public bool Exists(string path);
        public DirectoryInfo CreateDirectory(string directoryPath);
        public bool CreateFile(string path);
        public bool DeleteFile(string path);
    }


    /// <summary>
    /// Base class for performing basic file write operations related to creating and deleting files/directories
    /// </summary>
    public class FileWriter
    {
        /// <summary>
        /// Checks if the target file exists at the supplied file path
        /// </summary>
        /// <param name="path">System file path to the file</param>
        /// <returns>True if the file was found, false otherwise</returns>
        protected bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// If the directory already exists, a matching DirectoryInfo object is returned 
        /// </summary>
        /// <param name="directoryPath">System file path of the directory</param>
        protected DirectoryInfo CreateDirectory(string directoryPath)
        {
            try
            {
                return Directory.CreateDirectory(directoryPath);
            }
            catch (Exception){ return null; }
        }

        /// <summary>
        /// Creates a new file of type and location specified in the path parameter. Note, it does not
        /// overwrite the file, if another file with the same name already exists at the destination
        /// </summary>
        /// <param name="path">System file path where to create the file, and its file type</param>
        /// <returns>True if the file was created successfully, false if not created, or exception on error</returns>
        protected bool CreateFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using FileStream fs = File.Create(path); 
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes the target file if it exists
        /// </summary>
        /// <param name="path"> System file path of the file to delete</param>
        /// <returns>True if deletion successful, false if not, and exception on error</returns>
        protected bool DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
                throw;
            }
        }


    }//class
}//namespace