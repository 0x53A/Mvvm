using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Mvvm.App
{
    public interface INavigationService
    {
        /// <summary>
        /// Maps a ViewModel to a Page
        /// </summary>
        void Map<TViewModel, TPage>();
        /// <summary>
        /// Gets the ViewModel mapped to the Page
        /// </summary>
        /// <param name="tPage">The Type of the Page</param>
        /// <returns>The ViewModel mapped to the Page</returns>
        Type GetViewModel(Type tPage);
        void NavigateTo<T>();
        void NavigateTo<T>(object parameter);
        void NavigateBack();
        void NavigateForward();
        IList<PageStackEntry> BackStack { get; }
        IList<PageStackEntry> ForwardStack { get; }
    }
}
