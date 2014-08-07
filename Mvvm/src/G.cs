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

    public class GridPositioning
    {
        public struct Position
        {
            public int? X;
            public int? Y;
            public int Width;
            public int Height;
        }

        public struct PointI
        {
            public int X;
            public int Y;
        }

        int nextColumn = 0;
        int nextRow = 0;
        Grid<bool> usedCells = new Grid<bool>();

        int width;
        int height;

        public GridPositioning(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        bool MoveNext()
        {
            nextColumn += 1;
            if (nextColumn >= width)
            {
                nextColumn = 0;
                nextRow += 1;
            }
            if (nextRow >= height)
                return false;
            if (usedCells[nextColumn, nextRow])
                return MoveNext();
            else
                return true;
        }

        public PointI Get(Position preset)
        {
            PointI ret = new PointI() { X = 0, Y = 0 };

            if (preset.Y.HasValue)
            {
                nextRow = preset.Y.Value;
                nextColumn = 0;
            }
            if (preset.X.HasValue)
                nextColumn = preset.X.Value;

            ret = new PointI() { X = nextColumn, Y = nextRow };

            for (int x = 0; x < preset.Width; x++)
                for (int y = 0; y < preset.Height; y++)
                    usedCells[nextColumn + x, nextRow + y] = true;
            MoveNext();
            return ret;
        }
    }

    internal class PerGridValues
    {
        internal struct Status
        {
            internal int Rows { get; private set; }
            internal int Columns { get; private set; }
            internal int NChildren { get; private set; }
            internal Status(int rows, int columns, int nChildren)
                : this()
            {
                Rows = rows;
                Columns = columns;
                NChildren = nChildren;
            }
        }

        Grid grid;
        Status status;
        Dictionary<UIElement, GridPositioning.Position> presetValues = new Dictionary<UIElement, GridPositioning.Position>();

        internal PerGridValues(Grid grid)
        {
            this.grid = grid;
        }

        internal void Recalculate()
        {
            // wpf resets all attached properties when a template gets destroyed, but this would cause some exceptions
            if (StackTraceHelper.Get().Contains("ClearTemplateChain"))
                return;

            var rows = grid.RowDefinitions.Count;
            if (rows == 0)
                rows = 1;
            var columns = grid.ColumnDefinitions.Count;
            if (columns == 0)
                columns = 1;
            var nChildren = grid.Children.Count;

            var newStatus = new Status(rows, columns, nChildren);
            if (newStatus.Equals(status))
                return;
            else
                status = newStatus;

            GridPositioning posAlgo = new GridPositioning(columns, rows);
            for (int i = 0; i < nChildren; i++)
            {
                var child = (FrameworkElement)grid.Children[i];
                GridPositioning.Position preset;
                if (presetValues.ContainsKey(child))
                    preset = presetValues[child];
                else
                {
                    var c = child.ReadLocalValue(Grid.ColumnProperty);
                    var r = child.ReadLocalValue(Grid.RowProperty);
                    var cs = child.ReadLocalValue(Grid.ColumnSpanProperty);
                    var rs = child.ReadLocalValue(Grid.RowSpanProperty);


                    int? column = c.Equals(DependencyProperty.UnsetValue) ? null : (int?)(int)c;
                    int? row = r.Equals(DependencyProperty.UnsetValue) ? null : (int?)(int)r;
                    int columnSpan = cs.Equals(DependencyProperty.UnsetValue) ? 1 : (int)cs;
                    int rowSpan = rs.Equals(DependencyProperty.UnsetValue) ? 1 : (int)rs;

                    //HACK
                    //column = null;
                    //row = null;
                    //HACK

                    preset = new GridPositioning.Position() { X = column, Y = row, Width = columnSpan, Height = rowSpan };
                    presetValues[child] = preset;
                }

                var pos = posAlgo.Get(preset);
                Grid.SetColumn((FrameworkElement)child, pos.X);
                Grid.SetRow((FrameworkElement)child, pos.Y);
            }
        }
    }

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

        static string[] ParseAndSplit(DependencyPropertyChangedEventArgs args)
        {
            var newValue = args.NewValue as String;
            if (newValue == null)
                throw new Exception();

            var allowedChars = "0123456789;aAuto*.";
            if (newValue.Any(c => !allowedChars.Contains(c)))
                throw new Exception();

            var substrings = newValue.Split(';');
            if (substrings.Any(s => String.IsNullOrWhiteSpace(s)))
                throw new Exception();
            return substrings;
        }

        static void OnRowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as Grid;
            if (grid == null)
                throw new Exception();

            var substrings = ParseAndSplit(args);

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

            var substrings = ParseAndSplit(args);

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

        static object gridValuesLock = new object();
        static Dictionary<Grid, PerGridValues> gridValues = new Dictionary<Grid, PerGridValues>();

        static void RecalculateGrid(Grid grid)
        {
            var gv = gridValues.GetFromKeyOrCreate(grid, gridValuesLock, () => new PerGridValues(grid));
            gv.Recalculate();
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
