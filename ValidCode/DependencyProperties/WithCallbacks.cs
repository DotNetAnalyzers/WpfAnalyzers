namespace ValidCode.DependencyProperties
{
    using System;
    using System.Windows;

    public class WithCallbacks : FrameworkElement
    {
        /// <summary>Identifies the <see cref="P1"/> dependency property.</summary>
        public static readonly DependencyProperty P1Property = DependencyProperty.Register(
            "P1",
            typeof(int),
            typeof(WithCallbacks),
            new PropertyMetadata(
                default(int),
                (d, e) => ((WithCallbacks)d).OnP1Changed((int)e.OldValue, (int)e.NewValue),
                (d, o) => (int)o > 0 ? (int)o : 0),
            o => true);

        /// <summary>Identifies the <see cref="P2"/> dependency property.</summary>
        public static readonly DependencyProperty P2Property = DependencyProperty.Register(
            "P2",
            typeof(int),
            typeof(WithCallbacks),
            new PropertyMetadata(
                default(int),
                (d, e) => d.CoerceValue(P1Property),
                CoerceP2),
            o => true);

        public int P2
        {
            get => (int)this.GetValue(P2Property);
            set => this.SetValue(P2Property, value);
        }

        public int P1
        {
            get => (int)this.GetValue(P1Property);
            set => this.SetValue(P1Property, value);
        }

        /// <summary>This method is invoked when the <see cref="P1Property"/> changes.</summary>
        /// <param name="oldValue">The old value of <see cref="P1Property"/>.</param>
        /// <param name="newValue">The new value of <see cref="P1Property"/>.</param>
        protected void OnP1Changed(int oldValue, int newValue)
        {
        }

        private static object CoerceP2(DependencyObject d, object o)
        {
            switch (o)
            {
                case int i:
                    return Math.Max(0, i);
                default:
                    return -1;
            }
        }
    }
}
