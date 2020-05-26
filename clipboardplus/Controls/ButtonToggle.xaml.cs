using System.Windows;
using System.Windows.Controls;

namespace clipboardplus.Controls
{
    /// <summary>
    /// Interaction logic for ButtonToggle.xaml
    /// </summary>
    public partial class ButtonToggle : UserControl
    {
        public ButtonToggle()
        {
            InitializeComponent();
        }

            public Canvas Image
        {
            get { return (Canvas)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(Canvas), typeof(ButtonToggle), new UIPropertyMetadata(null));


        public bool isChecked
        {
            get { return (bool)GetValue(isCheckedProperty); }
            set { SetValue(isCheckedProperty, value); }
        }

        public static readonly DependencyProperty isCheckedProperty =
           DependencyProperty.Register("isChecked", typeof(bool), typeof(ButtonToggle), new UIPropertyMetadata(false));



        public event RoutedEventHandler Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null) Click(sender, e);
        }
    }
}
