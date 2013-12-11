using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Mvvm
{
    public class G
    {
        /**********/
        /** Rows **/
        /**********/

        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(G), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnRowsChanged)));

        public static void SetRows(DependencyObject element, string value)
        {
            element.SetValue(RowsProperty, value);
        }

        public static string GetRows(DependencyObject element)
        {
            return (string)element.GetValue(RowsProperty);
        }

        public static void OnRowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as System.Windows.Controls.Grid;
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

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns", typeof(string), typeof(G), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnColumnsChanged)));

        public static void SetColumns(DependencyObject element, string value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        public static string GetColumns(DependencyObject element)
        {
            return (string)element.GetValue(ColumnsProperty);
        }

        public static void OnColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as System.Windows.Controls.Grid;
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
    }
}
