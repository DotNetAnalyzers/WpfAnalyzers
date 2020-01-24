namespace ValidCode.AttachedProperties
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public static class ScrollViewer
    {
        public static readonly DependencyProperty AutoScrollToBottomProperty = DependencyProperty.RegisterAttached(
            "AutoScrollToBottom",
            typeof(bool),
            typeof(ScrollViewer),
            new PropertyMetadata(
                false,
                OnAutoScrollToBottomChanged));

        /// <summary>Helper for getting <see cref="AutoScrollToBottomProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="System.Windows.Controls.ScrollViewer"/> to read <see cref="AutoScrollToBottomProperty"/> from.</param>
        /// <returns>AutoScrollToBottom property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.ScrollViewer))]
        public static bool GetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element) => (bool)element.GetValue(AutoScrollToBottomProperty);

        /// <summary>Helper for setting <see cref="AutoScrollToBottomProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="System.Windows.Controls.ScrollViewer"/> to set <see cref="AutoScrollToBottomProperty"/> on.</param>
        /// <param name="value">AutoScrollToBottom property value.</param>
        public static void SetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element, bool value) => element.SetValue(AutoScrollToBottomProperty, value);

        private static void OnAutoScrollToBottomChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.NewValue is bool value)
            {
                if (value)
                {
                    scrollViewer.AutoScrollToBottom();
                    scrollViewer.ScrollChanged += ScrollChanged;
                }
                else
                {
                    scrollViewer.ScrollChanged -= ScrollChanged;
                }
            }
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.ExtentHeightChange > 0)
            {
                scrollViewer.AutoScrollToBottom();
            }
        }

        private static void AutoScrollToBottom(this System.Windows.Controls.ScrollViewer scrollViewer)
        {
            if (Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ActualHeight - scrollViewer.ExtentHeight) < 1)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
