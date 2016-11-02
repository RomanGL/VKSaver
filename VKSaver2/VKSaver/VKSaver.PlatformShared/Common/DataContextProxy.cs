using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Common
{
    public class DataContextProxy : FrameworkElement
    {
        public DataContextProxy()
        {
            this.Loaded += DataContextProxyLoaded;
        }

        void DataContextProxyLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= DataContextProxyLoaded;
            Binding binding = new Binding();
            if (!String.IsNullOrEmpty(BindingPropertyName))
            {
                binding.Path = new PropertyPath(BindingPropertyName);
            }
            binding.Source = this.DataContext;
            binding.Mode = BindingMode;
            this.SetBinding(DataSourceProperty, binding);
        }

        public object DataSource
        {
            get { return (object)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(Object), typeof(DataContextProxy), null);

        public string BindingPropertyName { get; set; }

        public BindingMode BindingMode { get; set; }
    }
}
