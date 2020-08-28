using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafariBugTracker.LoggerAPI.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class LogSettings
    {
        public string WriteDestination  { get; set; }
        public string FileName          { get; set; }
        public string DirectoryPath     { get; set; }
        public string ConnectionString  { get; set; }
    }
}