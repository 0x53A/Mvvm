using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Mvvm.App
{
    public interface INavigationService
    {
        void NavigateTo(string target);
        void NavigateBack();
        void NavigateForward();
    }
}
