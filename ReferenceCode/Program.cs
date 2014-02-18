using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceCode
{
    public class ReferenceCode
    {
        public static void Main(string[] args) { }

        private static void RaiseBasePropertyChanged(object self, string property)
        {
            var type = self.GetType().BaseType;
            FieldInfo eventField = null;
            while (type != typeof(object))
            {
                eventField = type.GetField("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                if (eventField == null)
                {
                    type = type.BaseType;
                    continue;
                }
                else
                    break;
            }
            if (eventField == null)
                throw new InvalidOperationException("Could not find the event field");
            var eventFieldValue = (PropertyChangedEventHandler)eventField.GetValue(self);
            eventFieldValue.Invoke(self, new PropertyChangedEventArgs(property));
        }
    }
}
