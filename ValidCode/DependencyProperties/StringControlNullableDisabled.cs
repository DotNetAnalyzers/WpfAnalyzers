#nullable disable
namespace ValidCode.DependencyProperties
{
    using System.Windows;

    public class StringControlNullableDisabled : GenericControl<string>
    {
        /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StringControlNullableDisabled),
            new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}


