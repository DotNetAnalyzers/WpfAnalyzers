namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Controls;

    public class UsingBoolBoxes : Control
    {
        /// <summary>Identifies the <see cref="IsTrue"/> dependency property.</summary>
        public static readonly DependencyProperty IsTrueProperty = DependencyProperty.Register(
            nameof(IsTrue),
            typeof(bool),
            typeof(UsingBoolBoxes),
            new PropertyMetadata(BooleanBoxes.False));

        public bool IsTrue
        {
            get => Equals(BooleanBoxes.True, this.GetValue(IsTrueProperty));
            set => this.SetValue(IsTrueProperty, BooleanBoxes.Box(value));
        }
    }
}
