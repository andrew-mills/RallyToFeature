using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace RallyToFeature
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    public class Settings : List<Setting>
    {

        public Setting this[string name] { get { return Find(name); } }

        /// <summary>
        /// Override of ADD which adds the specified object but then returns it back to the caller. 
        /// </summary>
        /// <param name="obj">The obj to add.</param>
        /// <returns>A reference to the added object</returns>
        public new Setting Add(Setting obj)
        {
            base.Add(obj);
            return obj;
        }

        public Setting Add(string name, string value = null, string description = null)
        {
            var obj = new Setting { Name = name, Value = value, Description = description };
            base.Add(obj);
            return obj;
        }

        public Setting Find(string name)
        {
            return this.FirstOrDefault(setting => setting.Name.ToUpper().Equals(name.ToUpper()));
        }

        public bool Exists(string name)
        {
            return this.Find(setting => setting.Name.ToUpper().Equals(name.ToUpper())) != null;
        }

        public TR Get<TR>(string name, TR defaultValue = default(TR))
        {
            var obj = Find(name);
            if (obj == null || string.IsNullOrEmpty(obj.Value)) return defaultValue;
            return (TR)Convert.ChangeType(obj.Value, typeof(TR));
        }

    }

}