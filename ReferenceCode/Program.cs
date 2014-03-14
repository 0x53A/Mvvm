using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceCode
{
    public interface IDuck
    {
        string Foo { get; set; }
        string Bar { get; set; }
    }

    public class Thing
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class Duck : IDuck
    {
        Thing __thing;
        public Duck(Thing t)
        {
            __thing = t;
        }

        public string Foo
        {
            get
            {
                return __thing.Foo;
            }
            set
            {
                __thing.Foo = value;
            }
        }

        public string Bar
        {
            get
            {
                return __thing.Bar;
            }
            set
            {
                __thing.Bar = value;
            }
        }
    }

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

        public int AccessGenerics()
        {
            var asd = new { Index = 5, X = 33, Y = 55 };
            int index = asd.Index;
            return index;
        }
    }
}
