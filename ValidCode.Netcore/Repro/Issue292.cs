namespace ValidCode.Repro
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public static class Issue292
    {
        /// <summary>
        /// An <see cref="IEnumerable"/> of rows where each row is an <see cref="IEnumerable"/> with the values.
        /// </summary>
        public static readonly DependencyProperty EnumerableProperty = DependencyProperty.RegisterAttached(
            "Enumerable",
            typeof(IEnumerable),
            typeof(Issue292),
            new PropertyMetadata(default(IEnumerable)));

        /// <summary>Helper for setting <see cref="EnumerableProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DataGrid"/> to set <see cref="EnumerableProperty"/> on.</param>
        /// <param name="value">RowsSource property value.</param>
        public static void SetEnumerable(this DataGrid element, IEnumerable? value)
        {
            if (element is null)
            {
                throw new System.ArgumentNullException(nameof(element));
            }

            element.SetValue(EnumerableProperty, value);
        }

        /// <summary>Helper for getting <see cref="EnumerableProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DataGrid"/> to read <see cref="EnumerableProperty"/> from.</param>
        /// <returns>RowsSource property value.</returns>
        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IEnumerable? GetEnumerable(this DataGrid element)
        {
            if (element is null)
            {
                throw new System.ArgumentNullException(nameof(element));
            }

            return (IEnumerable)element.GetValue(EnumerableProperty);
        }

        public static DataGrid M1()
        {
            var dataGrid = new DataGrid();
            var xs = new ObservableCollection<int>(new[] { 1, 2 });
            dataGrid.SetValue(Issue292.EnumerableProperty, xs);
            return dataGrid;
        }

        public static DataGrid M2()
        {
            var dataGrid = new DataGrid();
            var xs = new ObservableCollection<ObservableCollection<int>>
            {
                new ObservableCollection<int>(new[] { 1, 2 }),
                new ObservableCollection<int>(new[] { 3, 4 }),
                new ObservableCollection<int>(new[] { 5, 6 }),
            };
            dataGrid.SetValue(Issue292.EnumerableProperty, xs);
            return dataGrid;
        }
    }
}
