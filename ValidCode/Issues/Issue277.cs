namespace ValidCode.Issues.N
{
    using System.Windows;
    using AliasedType = AliasedType;

    public class Issue277 : FrameworkElement
    {
        /// <summary>Identifies the <see cref="AliasedType"/> dependency property.</summary>
        public static readonly DependencyProperty AliasedTypeProperty = DependencyProperty.Register(
            nameof(AliasedType),
            typeof(AliasedType),
            typeof(Issue277),
            new PropertyMetadata(default(AliasedType)));

        public AliasedType AliasedType
        {
            get => (AliasedType)this.GetValue(AliasedTypeProperty);
            set => this.SetValue(AliasedTypeProperty, value);
        }
    }
}

namespace ValidCode.Issues
{
    public class AliasedType
    {
    }
}
