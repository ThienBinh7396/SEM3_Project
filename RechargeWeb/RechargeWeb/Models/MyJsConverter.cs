using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace RechargeWeb.Models
{
    public class MyJsConverter<T> : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new List<Type>() { typeof(T) }; }
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            var result = obj.GetType()
                 .GetProperties()
                 .ToDictionary(p => p.Name,
                    p =>
                    {
                        var value = p.GetValue(obj, null);
                        if (value.GetType() == typeof(DateTime))
                            value = ((DateTime)value).ToShortDateString();

                        return value;
                    });

            return result;
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            var foo = Activator.CreateInstance<T>();

            foreach (var property in type.GetProperties())
            {
                var value = dictionary[property.Name];
                if (property.PropertyType == typeof(DateTime))
                    value = DateTime.Parse(value.ToString());

                property.SetValue(foo, value, null);
            }

            return foo;
        }
    }
}