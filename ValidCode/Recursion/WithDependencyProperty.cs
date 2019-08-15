namespace ValidCode.Recursion
{
    using System.Windows;
    using System.Windows.Controls;

   public class WithDependencyProperty : Control
    {
        /// <summary>Identifies the <see cref="P1"/> dependency property.</summary>
        public static readonly DependencyProperty P1Property = DependencyProperty.Register(
           nameof(P1),
           typeof(int),
           typeof(WithDependencyProperty),
           new PropertyMetadata(CreateDefault()));

        public int P1
        {
            get => (int)this.GetValue(P1Property);
            set => this.SetValue(P1Property, value);
        }

        private static int CreateDefault() => CreateDefault();
    }
}
