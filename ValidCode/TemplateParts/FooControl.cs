namespace ValidCode.TemplateParts
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    [TemplatePart(Name = "PART_Baz", Type = typeof(Control))]
    public class FooControl : Control
    {
        private const string PartBar = "PART_Bar";

        private FrameworkElement bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = null;
            if (this.GetTemplateChild(PartBar) is Border border)
            {
                this.bar = border;
            }

            this.bar = (Border)this.GetTemplateChild(PartBar);
            this.bar = (FrameworkElement)this.GetTemplateChild(PartBar);
            this.bar = this.GetTemplateChild(PartBar) as Border;
            this.bar = this.GetTemplateChild(PartBar) as FrameworkElement;
            switch (this.GetTemplateChild(PartBar))
            {
                case Border b:
                    this.bar = b;
                    break;
            }

            var baz = this.GetTemplateChild("PART_Baz") as FrameworkElement;
            baz = this.GetTemplateChild("PART_Baz") as Control;
            baz = this.GetTemplateChild("PART_Baz") as DataGrid;
        }
    }
}
