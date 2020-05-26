using System.Windows;
using System.Windows.Controls;

namespace clipboardplus.Controls
{
    /// <summary>
    /// Interaction logic for ButtonImg.xaml
    /// </summary>
    public partial class ButtonImg : UserControl
    {
        public ButtonImg()
        {
            InitializeComponent();
        }


        public Canvas Image
        {
            get { return (Canvas)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(Canvas), typeof(ButtonImg), new UIPropertyMetadata(null));

        public event RoutedEventHandler Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null) Click(sender, e);
        }
    }
}
