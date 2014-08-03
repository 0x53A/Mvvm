using Mvvm.FirstClassEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Mvvm
{
    public class NavigatedFromEventArgs
    {
        public NavigationMode NavigationMode { get; private set; }
        public Dictionary<string, Object> State { get; private set; }

        public NavigatedFromEventArgs(NavigationMode mode, Dictionary<string, Object> state)
        {
            NavigationMode = mode;
            State = state;
        }
    }

    public class NavigatedToEventArgs
    {
        public NavigationMode NavigationMode { get; private set; }
        public Dictionary<string, Object> State { get; private set; }
        public object Parameter { get; private set; }
        public Type Source { get; private set; }

        public NavigatedToEventArgs(NavigationMode mode, Dictionary<string, Object> state, object parameter, Type source)
        {
            NavigationMode = mode;
            State = state;
            Parameter = parameter;
            Source = source;
        }
    }

    /// <summary>
    /// Represents an Object (a Page) which participates in navigation.
    /// </summary>
    public interface INavigationObject
    {
        /// <summary>
        /// called when the user navigates to this object.
        /// The ViewModel should load it's data.
        /// </summary>
        IEvent<INavigationObject, NavigatedFromEventArgs> NavigatedFrom { get; }
        /// <summary>
        /// Called when the user navigates away.
        /// The ViewModel should save it's data.
        /// </summary>
        IEvent<INavigationObject, NavigatedToEventArgs> NavigatedTo { get; }
    }
}
