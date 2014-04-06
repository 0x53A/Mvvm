using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if !UNIVERSAL
using System.Windows.Controls;
using System.Windows.Markup;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Mvvm
{
#if WINDOWS_PHONE || UNIVERSAL
    public class GridLengthConverter
    {
        public object ConvertFrom(string s)
        {
            GridLength gl;
            if (s.ToLower() == "auto")
            {
                gl = new GridLength(1, GridUnitType.Auto);
            }
            else if (s == "*")
                gl = new GridLength(1, GridUnitType.Star);
            else if (System.Text.RegularExpressions.Regex.IsMatch(s,"[0-9]\\*"))
            {
                var n = Int32.Parse(s.Substring(0, 1));
                gl = new GridLength(n, GridUnitType.Star);
            }
            else
            {
                var n = Int32.Parse(s);
                gl = new GridLength(n);
            }
            return gl;
        }
    }
#endif
    public class G
    {
        /**********/
        /** Rows **/
        /**********/

        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(G), new PropertyMetadata("", new PropertyChangedCallback(OnRowsChanged)));

        public static void SetRows(DependencyObject element, string value)
        {
            element.SetValue(RowsProperty, value);
        }

        public static string GetRows(DependencyObject element)
        {
            return (string)element.GetValue(RowsProperty);
        }

        static void OnRowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as Grid;
            if (grid == null)
                throw new Exception();

            var newValue = args.NewValue as String;
            if (newValue == null)
                throw new Exception();

            var allowedChars = "0123456789;aAuto*.";
            if (newValue.Any(c => !allowedChars.Contains(c)))
                throw new Exception();

            var substrings = newValue.Split(';');
            if (substrings.Any(s => String.IsNullOrWhiteSpace(s)))
                throw new Exception();

            //clear old collection
            RowDefinitionCollection col = grid.RowDefinitions;
            col.Clear();

            var converter = new GridLengthConverter();
            foreach (var row in substrings)
                col.Add(new RowDefinition() { Height = (GridLength)converter.ConvertFrom(row) });
        }


        /*************/
        /** Columns **/
        /*************/

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns", typeof(string), typeof(G), new PropertyMetadata("", new PropertyChangedCallback(OnColumnsChanged)));

        public static void SetColumns(DependencyObject element, string value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        public static string GetColumns(DependencyObject element)
        {
            return (string)element.GetValue(ColumnsProperty);
        }

        static void OnColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as Grid;
            if (grid == null)
                throw new Exception();

            var newValue = args.NewValue as String;
            if (newValue == null)
                throw new Exception();

            var allowedChars = "0123456789;aAuto*.";
            if (newValue.Any(c => !allowedChars.Contains(c)))
                throw new Exception();

            var substrings = newValue.Split(';');
            if (substrings.Any(s => String.IsNullOrWhiteSpace(s)))
                throw new Exception();

            //clear old collection
            ColumnDefinitionCollection col = grid.ColumnDefinitions;
            col.Clear();

            var converter = new GridLengthConverter();
            foreach (var Column in substrings)
                col.Add(new ColumnDefinition() { Width = (GridLength)converter.ConvertFrom(Column) });
        }

        /* Autoset */

        public static readonly DependencyProperty AutoArrange = DependencyProperty.RegisterAttached("AutoArrange", typeof(bool), typeof(G), new PropertyMetadata(false, new PropertyChangedCallback(OnAutoArrangeChanged)));

        public static void SetAutoArrange(DependencyObject element, bool value)
        {
            element.SetValue(AutoArrange, value);
        }

        public static bool GetAutoArrange(DependencyObject element)
        {
            return (bool)element.GetValue(AutoArrange);
        }

        static Dictionary<Grid, Tuple<int, int, int>> gridValues = new Dictionary<Grid, Tuple<int, int, int>>();

        static void RecalculateGrid(Grid grid)
        {
            var rows = grid.RowDefinitions.Count;
            if (rows == 0)
                rows = 1;
            var columns = grid.ColumnDefinitions.Count;
            if (columns == 0)
                columns = 1;
            var nChildren = grid.Children.Count;

            var tuple = Tuple.Create(rows, columns, nChildren);
            if ((!gridValues.ContainsKey(grid)) || gridValues[grid] != tuple)
            {
                for (int i = 0; i < nChildren; i++)
                {
                    var column = i % columns;
                    var row = i / columns;
                    var child = grid.Children[i];

                    Grid.SetRow((FrameworkElement) child, row);
                    Grid.SetColumn((FrameworkElement)child, column);
                }
                gridValues[grid] = tuple;
            }
        }

        static void OnAutoArrangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as Grid;
            if (grid == null)
                throw new Exception();

            var newValue = (bool)args.NewValue;
            var oldValue = GetAutoArrange(grid);

            grid.Loaded -= grid_RoutedUpdated;
            grid.LayoutUpdated -= grid_Updated;
#if !WINDOWS_PHONE && !UNIVERSAL
            grid.IsVisibleChanged -= grid_IsVisibleChanged;
            grid.SourceUpdated -= grid_Updated;
            grid.TargetUpdated -= grid_Updated;
#endif
            grid.LayoutUpdated -= grid_Updated;
            if (newValue)
            {
                grid.Loaded += grid_RoutedUpdated;
                grid.LayoutUpdated += grid_Updated;
#if !WINDOWS_PHONE && !UNIVERSAL
                grid.IsVisibleChanged += grid_IsVisibleChanged;
                grid.SourceUpdated += grid_Updated;
                grid.TargetUpdated += grid_Updated;
#endif
                grid.LayoutUpdated += grid_Updated;
                RecalculateGrid(grid);
            }
        }

        static void grid_RoutedUpdated(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is Grid)
                RecalculateGrid(sender as Grid);
        }

        static void grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null && sender is Grid)
                RecalculateGrid(sender as Grid);
        }

#if !UNIVERSAL
        static void grid_Updated(object sender, EventArgs e)
#else
        static void grid_Updated(object sender, object e)
#endif
        {
            if (sender != null && sender is Grid)
                RecalculateGrid(sender as Grid);
        }
    }
}
