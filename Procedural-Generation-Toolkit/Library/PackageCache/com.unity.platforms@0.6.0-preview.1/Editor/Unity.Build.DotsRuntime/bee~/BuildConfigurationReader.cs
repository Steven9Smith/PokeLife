using System;
using System.Text;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NiceIO;

namespace Unity.Build.DotsRuntime
{
    internal class BuildConfigurationReader
    {
        public static void Read(NPath file, Type configType)
        {
            if (!file.FileExists())
            {
                return;
            }
            var json = file.ReadAllText();
            var jarray = JArray.Parse(json);
            foreach (var jobject in jarray)
            {
                var type = jobject["$type"].Value<string>();
                var proptype = Type.GetType(type.Substring(0, type.IndexOf(',')));

                foreach (var settingProp in configType.GetProperties())
                {
                    if (settingProp.PropertyType != proptype)
                    {
                        continue;
                    }
                    var settingValue = Convert.ChangeType(Activator.CreateInstance(proptype), proptype);
                    foreach (var jo in jobject.Children().OfType<JProperty>())
                    {
                        if (jo.Name == "$type")
                        {
                            continue;
                        }
                        var property = proptype.GetProperty(jo.Name);
                        if (property != null)
                        {
                            var val = jo.ToObject(property.PropertyType);
                            property.SetValue(settingValue, jo.ToObject(property.PropertyType));
                            continue;
                        }
                        var field = proptype.GetField(jo.Name);
                        if (field != null)
                        {
                            var val = jo.ToObject(field.FieldType);
                            field.SetValue(settingValue, jo.ToObject(field.FieldType));
                        }
                    }
                    settingProp.SetValue(null, settingValue);
                }
            }
        }
    }
}

