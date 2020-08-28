using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SafariBugTracker.LoggerAPI.FileIO
{
    /// <summary>
    /// Specialized variant of the FileWriter base class. Provides methods for extracting the types and values of an objects properties, 
    /// and for writing/appending strings to text files.
    /// </summary>
    public class ThreadsafeSingletonTextWriter : FileWriter, IFileWriter
    {
        #region Fields, Properties, Constructors


        /// <summary>
        /// Thread safe lazy instantiation of the class. The LazyThreadSafetyMode option is used to ensure that only a single thread can initialize a single instance of this class.
        /// </summary>
        private static readonly Lazy<ThreadsafeSingletonTextWriter> _lazyInstance = new Lazy<ThreadsafeSingletonTextWriter>(() => new ThreadsafeSingletonTextWriter(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Simple lock reference used to allow only a single write operation at a time
        /// </summary>
        private readonly object _writeLock;

        /// <summary>
        /// Returns the single instance of the class
        /// </summary>
        public static ThreadsafeSingletonTextWriter Instance => _lazyInstance.Value;


        private ThreadsafeSingletonTextWriter()
        {
            _writeLock = new object();
        }



        #endregion
        #region PrivateMethods


        /// <summary>
        /// Attempts to extract the name and values of the objects properties
        /// </summary>
        /// <param name="srcObj"> Target object to get it's properties </param>
        /// <returns> Collection of strings representing the property names and their values </returns>
        private IEnumerable<string> ExtractProperties(object srcObj)
        {
            string[] propertyNames = srcObj.GetType().GetProperties().Select(_ => _.Name).ToArray();
            foreach (var prop in propertyNames)
            {
                var propName = srcObj.GetType().GetProperty(prop).Name;
                var propValue = srcObj.GetType().GetProperty(prop).GetValue(srcObj, null);
                if (propValue != null)
                {
                    yield return string.Format($"{propName}: {propValue.ToString()}");
                }
            }
        }


        #endregion
        #region PublicMethods


        /// <summary>
        /// Base overwrite method. Creates a new file, writes the specified string to the file, and then closes it.
        /// If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name="path"> Path, and name of the file </param>
        /// <param name="text"> String that will be written to the file </param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Overwrite(string path, string text)
        {
            lock(_writeLock)
            {
                File.WriteAllText(path, text);
            }
        }

        /// <summary>
        /// Base overwrite method. Writes the collection of strings to the file, overwriting the files previous contents if it exists
        /// </summary>
        /// <param name="path"> Path, and name of the file </param>
        /// <param name="text"> Strings that will be written to the file </param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Overwrite(string path, IEnumerable<string> text)
        {
            lock(_writeLock)
            {
                File.WriteAllLines(path, text);
            }
        }

        /// <summary>
        /// Base append method. Appends the string array to the specified file
        /// </summary>
        /// <param name="path"> Path, and name of the file </param>
        /// <param name="text"> Collection of strings to append to the file </param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Append(string path, List<string> text)
        {
            lock (_writeLock)
            {
                File.AppendAllLines(path, text);
            }
        }

        /// <summary>
        /// Base append method. Appends the string to the specified file
        /// </summary>
        /// <param name="path"> Path, and name of the file </param>
        /// <param name="text"> String to append to the file </param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Append(string path, string text)
        {
            lock (_writeLock)
            {
                using (StreamWriter writer = File.AppendText(path))
                {
                    writer.WriteLine(text);
                }
            }
        }


        #endregion
        #region IFileWriterImplementation


        /// <summary>
        /// Extracts the names and values of the objects properties, and converts them to strings.
        /// Creates a new file if the target file doesn't exist. Will overwrite the contents 
        /// of a pre-existing files 
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="path">System file path of where to write the objects </param>
        /// <param name="objToExtractText"> Source object to write to the file</param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Overwrite<T>(string path, object objToExtractText)
        {
            var propertiesAndValues = ExtractProperties(objToExtractText);
            Overwrite(path, propertiesAndValues);
        }

        /// <summary>
        /// Extracts the names and values of the objects properties, and converts them to strings.
        /// Creates a new file if the target file doesn't exist. Will overwrite the contents 
        /// of a pre-existing files 
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="path">System file path of where to write the objects </param>
        /// <param name="objsToExtractText"> Source objects to write to the file</param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Overwrite<T>(string path, object[] objsToExtractText)
        {
            var propertiesAndValues = new List<string>();
            foreach (var item in objsToExtractText)
            {
                var extractedStrings = ExtractProperties(item);
                foreach (var element in extractedStrings)
                {
                    propertiesAndValues.Add(element);
                }
            }
            Overwrite(path, propertiesAndValues.AsEnumerable());
        }

        /// <summary>
        /// Extracts the names and values from each key value pair and converts them to a strings (per pair).
        /// Creates a new file if the target file doesn't exist. 
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="path">System file path of where to write the object, and it's file type </param>
        /// <param name="kvpCollection"> Collection of key value pairs to concatenate, and write to the file</param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Append (string path, IEnumerable<KeyValuePair<string, object>> kvpCollection)
        {
            var textList = new List<string>();
            foreach (var item in kvpCollection)
            {
                textList.Add($"{item.Key} :  {item.Value}");
            }

            //Find the last element in the list, and add a carriage return at the end
            //  This will make it easier to distinguish between logs
            int lastElement = textList.Count() - 1;
            textList[lastElement] += System.Environment.NewLine;

            Append(path, textList);
        }

        /// <summary>
        /// Extracts the names and values of the objects properties, and converts them to strings.
        /// Creates a new file if the target file doesn't exist. 
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="path">System file path of where to write the object, and it's file type </param>
        /// <param name="objToExtractText"> Source object to write to the file</param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Append<T>(string path, object objToExtractText)
        {
            var propertiesAndValues = ExtractProperties(objToExtractText).ToList();

            //Find the last element in the list, and add a carriage return at the end
            //  This will make it easier to distinguish between logs
            int lastElement = propertiesAndValues.Count() - 1;
            propertiesAndValues[lastElement] += System.Environment.NewLine;

            Append(path, propertiesAndValues);
        }

        /// <summary>
        /// Iterates over the collection of objects to extract their properties, and append to the file once per object
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="path">System file path of where to write the objects </param>
        /// <param name="objsToExtractText"> Source object to write to the file</param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Append<T>(string path, object[] objsToExtractText)
        {
            foreach (var item in objsToExtractText)
            {
                Append<T>(path, item);
            }
        }

        /// <summary>
        /// Identical to its base class counterpart. Checks if the target file exists at the supplied file path
        /// </summary>
        /// <param name="path">System file path to the file</param>
        /// <returns>True if the file was found, false otherwise</returns>
        public new bool Exists(string path) => base.Exists(path);

        /// <summary>
        /// Identical to its base class counterpart. If the directory already exists, a matching DirectoryInfo object is returned 
        /// </summary>
        /// <param name="directoryPath">System file path of the directory</param>
        /// <returns>DirectoryInfo object with the directory matching the path argument, or null if the path is null, or an error occured </returns>
        public new DirectoryInfo CreateDirectory(string directoryPath) => base.CreateDirectory(directoryPath);

        /// <summary>
        /// Identical to its base class counterpart. Creates a new file of type and location specified in the path parameter. Note, it does not
        /// overwrite the file, if another file with the same name already exists at the destination
        /// </summary>
        /// <param name="path">System file path where to create the file, and its file type</param>
        /// <returns>True if the file was created successfully, false if not created, or exception on error</returns>
        public new bool CreateFile(string path) => base.CreateFile(path);

        /// <summary>
        /// Identical to its base class counterpart. Deletes the target file if it exists
        /// </summary>
        /// <param name="path"> System file path of the file to delete</param>
        /// <returns>True if deletion successful, false if not, and exception on error</returns>
        public new bool DeleteFile(string path) => base.DeleteFile(path);



        #endregion
    }//class
}//namespace