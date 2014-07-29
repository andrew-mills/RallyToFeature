using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RallyToFeature
{
    public enum DataTargetTypeEnum
    {
        [XmlEnum(Name = "RallyDev")]
        RallyDev
    }


    [Serializable]
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("{Type}")]
    public class DataTarget
    {

        public DataTarget(DataTargetTypeEnum type, bool isActive = true)
            : this()
        {
            Type = type;
            IsActive = isActive;
        }

        public DataTarget()
        {
            Settings = new Settings();
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public DataTargetTypeEnum Type { get; set; }

        [XmlAttribute]
        public bool IsActive { get; set; }

        [XmlElement("Settings", typeof(Settings), Form = XmlSchemaForm.Unqualified)]
        public Settings Settings { get; set; }

    }


    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    public class DataTargets : List<DataTarget>
    {

        public DataTarget this[DataTargetTypeEnum type] { get { return Find(type); } }

        /// <summary>
        /// Override of ADD which adds the specified object but then returns it back to the caller. 
        /// </summary>
        /// <param name="obj">The obj to add.</param>
        /// <returns>A reference to the added object</returns>
        public new DataTarget Add(DataTarget obj)
        {
            base.Add(obj);
            return obj;
        }

        public DataTarget Add(DataTargetTypeEnum type, bool isActive = false)
        {
            return Add(new DataTarget(type, isActive));
        }

        public DataTarget Find(DataTargetTypeEnum type)
        {
            return this.FirstOrDefault(setting => setting.Type.Equals(type));
        }

    }


    [Serializable]
    public class RallyConnectionSettings
    {
        private const string DefaultConfigFileName = "Rally.Settings.xml";

        public RallyConnectionSettings()
        {
        }

        private static RallyConnectionSettings _instance;

        public static RallyConnectionSettings Instance
        {
            get { return _instance ?? (_instance = new RallyConnectionSettings()); }
        }

        [XmlElement("DataTargets", typeof (DataTargets), Form = XmlSchemaForm.Unqualified)]
        public DataTargets DataTargets { get; set; }


        //===================================================================
        /// <summary>
        ///     Gets the name of the config file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private static string GetConfigFileName(string fileName)
        {

            if (string.IsNullOrEmpty(fileName))
                fileName = DefaultConfigFileName;
            
            // If we have a valid FileName at this point, return it (a filename was provided of the default is corect)
            // -------------------------------------------------------------------------------------------------------
            if (System.IO.File.Exists(fileName)) return fileName;

            var filePath = Path.GetFileName(fileName);
            if (!string.IsNullOrEmpty(filePath))
            {
                // If not, use the filename provided but look for it in the current directory
                // --------------------------------------------------------------------------------------------------------
                fileName = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                if (System.IO.File.Exists(fileName)) return fileName;

                // If not, then try to find it in the directory/folder where the application is resided 
                // ---------------------------------------------------------------------------------------------------------
                var assPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(assPath))
                {
                    fileName = Path.Combine(assPath, filePath);
                    if (System.IO.File.Exists(fileName)) return fileName;
                }
            }
            throw new LoggedException("Cannot find a Valid ArchieSyncConfig configuration file. ");
        }


        public static RallyConnectionSettings Load(string fileName = null)
        {
            try
            {
                using (var fs = new FileStream(GetConfigFileName(fileName), FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(RallyConnectionSettings));
                    var rallySettingsObj = (RallyConnectionSettings)serializer.Deserialize(fs);
                    _instance = rallySettingsObj;
                    return rallySettingsObj;
                }
            }
            catch (Exception ex)
            {
                throw new LoggedException("Could not LOAD the Configuration Data. Reason: " + ex.Message, ex);
            }
        }
    }
}