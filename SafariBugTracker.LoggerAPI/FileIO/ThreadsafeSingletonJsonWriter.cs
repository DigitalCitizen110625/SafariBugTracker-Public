using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace SafariBugTracker.LoggerAPI.FileIO
{

    /// <summary>
    /// Specialized class of the FileWriter base class. Provides methods for serializing JSON objects, 
    /// and writing/appending to Json files.
    /// </summary>
    public class ThreadsafeSingletonJsonWriter : FileWriter, IFileWriter
    {

        #region Fields, Properties, Constructors



        /// <summary>
        /// Thread safe lazy instantiation of the class. The LazyThreadSafetyMode option is used to ensure that only a single thread can initialize a single instance of this class.
        /// </summary>
        private static readonly Lazy<ThreadsafeSingletonJsonWriter> _lazyInstance = new Lazy<ThreadsafeSingletonJsonWriter>(() => new ThreadsafeSingletonJsonWriter(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Simple lock reference used to allow only a single write operation at a time
        /// </summary>
        private readonly object _writeLock;

        /// <summary>
        /// Returns the single instance of the class
        /// </summary>
        public static ThreadsafeSingletonJsonWriter Instance => _lazyInstance.Value;


        private ThreadsafeSingletonJsonWriter()
        {
            _writeLock = new object();
        }



        #endregion
        #region PrivateMethods


        /// <summary>
        /// Checks if the .json file type is specified in the file path
        /// </summary>
        /// <param name="path"> File path, name, and file type of the target file</param>
        private void EnsureJsonFileType(string path)
        {
            const string jsonFileType = ".json";
            if (!path.EndsWith(jsonFileType))
            {
                throw new Exception($"{path} does not end with {jsonFileType} file type");
            }
        }


        #endregion
        #region PrivateMethods


        /// <summary>
        /// Converts the object to a string using the Microsoft Json serializer
        /// </summary>
        /// <param name="objToSerialize"> Object to convert to string </param>
        /// <returns> Serialized object as a string </returns>
        private string SerializeToString<T>(object objToSerialize)
        {
            return JsonSerializer.Serialize((T)objToSerialize);
        }

        /// <summary>
        /// Converts a collection of objects to strings using the Microsoft Json serializer
        /// </summary>
        /// <param name="objToSerialize">Array of objects to convert to strings </param>
        /// <returns> Serialized objects as  string</returns>
        private IEnumerable<string> SerializeToString<T>(object[] objToSerialize)
        {
            foreach (var obj in objToSerialize)
            {
                yield return JsonSerializer.Serialize((T)obj);
            }
        }


        #endregion
        #region IFileWriterImplementation


        /// <summary>
        /// Serializes the object using the Microsoft JsonSerializer, and writes the contents to a new file. 
        /// Overwrites the contents of the pre-existing file with the passed in data.
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to write to </param>
        /// <param name="serializeToText"> Json object to convert into a string </param>
        /// <returns>Throws exception on all errors</returns>
        public void Overwrite<T>(string path, object serializeToText)
        {
            EnsureJsonFileType(path);
            var contents = SerializeToString<T>(serializeToText);

            lock(_writeLock)
            {
                File.WriteAllText(path, "[" + Environment.NewLine + contents + Environment.NewLine + "]");
            }
        }

        /// <summary>
        /// Serializes the objects using the Microsoft JsonSerializer, and writes the contents to a new file. 
        /// Overwrites the contents of the pre-existing file with the passed in data.
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to write to </param>
        /// <param name="serializeToText"> Collection of Json objects to convert into a strings </param>
        /// <returns>Throws exception on all errors</returns>
        public void Overwrite<T>(string path, object[] serializeToText)
        {
            EnsureJsonFileType(path);
            var serializedJson = SerializeToString<T>(serializeToText);

            lock (_writeLock)
            {
                using (var sr = new StreamWriter(path))
                {
                    int i = 1;
                    sr.Write('[' + Environment.NewLine);
                    foreach (var line in serializedJson)
                    {
                        sr.Write(line);
                        if (i < serializedJson.Count())
                        {
                            sr.Write("," + Environment.NewLine);
                        }
                        i++;
                    }
                    sr.Write(Environment.NewLine + "]");
                }
            }
        }

        /// <summary>
        /// Extracts the names and values from each key value pair and converts them to a strings (per pair).
        /// Creates a new file if the target file doesn't exist. 
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="path">System file path of where to write the object, and it's file type </param>
        /// <param name="kvpCollection"> Collection of key value pairs to concatenate, and write to the file</param>
        /// <returns> Throws exceptions on any errors </returns>
        public void Append(string path, IEnumerable<KeyValuePair<string, object>> kvpCollection)
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

            Append<object>(path, textList);
        }

        /// <summary>
        /// Serializes the object using the Microsoft JsonSerializer, and appends the contents to the file.
        /// Throws exception if the file doesn't exist.
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to append to </param>
        /// <param name="serializeToText"> Json object to convert into a string </param>
        /// <returns>Throws exception on all errors</returns>
        public void Append<T>(string path, object serializeToText)
        {
            EnsureJsonFileType(path);
            var contents = SerializeToString<T>(serializeToText);

            //Creates the file if it does not already exist
            CreateFile(path);

            lock (_writeLock)
            {
                using (var fs = File.Open(@path, FileMode.Open, FileAccess.ReadWrite))
                using (var sr = new StreamWriter(fs))
                {
                    //Note: For json files to be valid, they must be enclosed in square brackets
                    if (new FileInfo(path).Length == 0)
                    {
                        //The file is empty
                        sr.Write("[" + Environment.NewLine);
                    }
                    else
                    {
                        //The file is not empty
                        const int seekToLastElem = -3;
                        fs.Seek(seekToLastElem, SeekOrigin.End);
                        sr.Write("," + Environment.NewLine);
                    }

                    sr.Write(contents + Environment.NewLine);
                    sr.Write("]");
                }
            }
        }

        /// <summary>
        /// Serializes the objects using the Microsoft JsonSerializer and appends the contents to the file
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to append to </param>
        /// <param name="serializeToText"> Collection of Json objects to convert into a string </param>
        /// <returns>Throws exception on all errors</returns>
        public void Append<T>(string path, object[] serializeToText)
        {
            EnsureJsonFileType(path);
            var contents = SerializeToString<T>(serializeToText);

            //Creates the file if it does not already exist
            CreateFile(path);

            lock (_writeLock)
            {
                using (var fs = File.Open(@path, FileMode.Open, FileAccess.ReadWrite))
                using (var sr = new StreamWriter(fs))
                {
                    if (new FileInfo(path).Length == 0)
                    {
                        //The file is empty
                        sr.Write("[" + Environment.NewLine);
                    }
                    else
                    {
                        //Seek to the last elements closing bracket '}' , and add a comma before appending each new object
                        const int seekToLastElem = -3;
                        fs.Seek(seekToLastElem, SeekOrigin.End);
                    }

                    foreach (var item in contents)
                    {
                        sr.Write("," + Environment.NewLine);
                        sr.Write(item);
                    }

                    sr.Write(Environment.NewLine + "]");
                }
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