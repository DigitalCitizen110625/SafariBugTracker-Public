using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace SafariBugTracker.LoggerAPI.FileIO
{

    /// <summary>
    /// Specialized variant of the FileWriter base class. Provides methods for serializing objects into binary, 
    /// and writing/appending to binary files.
    /// </summary>
    public class ThreadsafeSingletonBinaryWriter : FileWriter, IFileWriter
    {
        #region Fields, Properties, Constructors



        /// <summary>
        /// Thread safe lazy instantiation of the class. The LazyThreadSafetyMode option is used to ensure that only a single thread can initialize a single instance of this class.
        /// </summary>
        private static readonly Lazy<ThreadsafeSingletonBinaryWriter> _lazyInstance = new Lazy<ThreadsafeSingletonBinaryWriter>(() => new ThreadsafeSingletonBinaryWriter(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Simple lock reference used to allow only a single write operation at a time
        /// </summary>
        private readonly object _writeLock;

        /// <summary>
        /// Returns the single instance of the class
        /// </summary>
        public static ThreadsafeSingletonBinaryWriter Instance => _lazyInstance.Value;


        private ThreadsafeSingletonBinaryWriter()
        {
            _writeLock = new object();
        }



        #endregion
        #region PrivateMethods



        /// <summary>
        /// Converts the object into a binary byte array
        /// </summary>
        /// <typeparam name="T"> Type of object to serialize </typeparam>
        /// <param name="objToSerialize"> Target object to serialize </param>
        /// <returns>Byte array of the serialized object </returns>
        private byte[] SerializeToBytes<T>(object objToSerialize)
        {
            using (var ms = new MemoryStream())
            {
                IFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, (T)objToSerialize);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts all objects into separate byte arrays
        /// </summary>
        /// <typeparam name="T"> Type of object to serialize </typeparam>
        /// <param name="objsToSerialize"> Collection of objects that will be serialized </param>
        /// <returns>Byte array of each serialized object </returns>
        private IEnumerable<byte[]> SerializeToBytes<T>(object[] objsToSerialize)
        {
            using (var ms = new MemoryStream())
            {
                IFormatter bf = new BinaryFormatter();
                foreach (var obj in objsToSerialize)
                {
                    bf.Serialize(ms, (T)obj);
                    yield return ms.ToArray();
                }
            }
        }



        #endregion
        #region IFileWriterImplementation



        /// <summary>
        /// Serializes the object, and writes the contents a new file if one does not exist.
        /// Otherwise, it overwrites the contents of an existing file
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to write </param>
        /// <param name="objToSerialize"> Object to convert into a byte array </param>
        /// <returns>Throws exception on all errors</returns>
        public void Overwrite<T>(string path, object objToSerialize)
        {
            lock (_writeLock)
            {
                var binaryData = SerializeToBytes<T>(objToSerialize);
                File.WriteAllBytes(path, binaryData);
            }
        }

        /// <summary>
        /// Serializes the objects, and writes the contents a new file if one does not exist.
        /// Otherwise, it overwrites the contents of an existing file
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to write </param>
        /// <param name="objsToSerialize"> Collection of objects to convert into byte arrays </param>
        /// <returns>Throws exception on all errors</returns>
        public virtual void Overwrite<T>(string path, object[] objsToSerialize)
        {
            var binaryData = SerializeToBytes<T>(objsToSerialize);

            lock (_writeLock)
            {
                foreach (var item in binaryData)
                {
                    File.WriteAllBytes(path, item);
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
        /// Serializes the object, and appends the contents to the file
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to append to </param>
        /// <param name="objToSerialize"> Object to convert into a byte array </param>
        /// <returns>Throws exception on all errors</returns>
        public virtual void Append<T>(string path, object objToSerialize)
        {
            var binaryData = SerializeToBytes<T>(objToSerialize);

            //SOURCE: https://stackoverflow.com/questions/52020383/filestreams-filemode-openorcreate-overwrites-file
            //Note that File.Open and File.Append are not synonymous. FileMode.OpenOrCreate does not overwrite the file, but the 
            //  stream will start at the beginning of the file if it already exists. This causes the contents to be
            //  overwritten by the StreamWriter, not the FileStream constructor overwriting the file.
            //  Thus, we seek to the end of the file before appending to it

            lock(_writeLock)
            {
                using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
                {
                    writer.Seek(0, SeekOrigin.End);
                    writer.Write(binaryData, 0, binaryData.Length);
                }
            }
        }

        /// <summary>
        /// Serializes the objects, and appends the contents to the file
        /// </summary>
        /// <typeparam name="T"> Type of object that will be serialized </typeparam>
        /// <param name="path"> Path, name and file type to append to </param>
        /// <param name="objsToSerialize"> Collection of objects to convert into byte arrays </param>
        /// <returns>Throws exception on all errors</returns>
        public virtual void Append<T>(string path, object[] objsToSerialize)
        {
            var binaryData = SerializeToBytes<T>(objsToSerialize);

            //https://stackoverflow.com/questions/52020383/filestreams-filemode-openorcreate-overwrites-file
            //Note that File.Open and File.Append are not synonymous. FileMode.OpenOrCreate is not overwriting the file, but the 
            //  stream does start at the beginning of the file if one already exists. This causes the contents to be
            //  overwritten by the StreamWriter, not the FileStream constructor overwriting the file.
            //  Thus, we seek to the end of the file before appending to it

            lock (_writeLock)
            {
                using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
                {
                    writer.Seek(0, SeekOrigin.End);
                    foreach (var item in binaryData)
                    {
                        writer.Write(item, 0, item.Length);
                    }
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