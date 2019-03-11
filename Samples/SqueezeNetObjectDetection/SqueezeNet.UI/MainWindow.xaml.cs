using System.IO;
using SqueezeNet.Library;
using System.Windows;

namespace SqueezeNet.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            ObjectDetectionModel c = new ObjectDetectionModel();
            var bytes = File.ReadAllBytes(@"..\..\..\..\..\SharedContent\media\fish.png");
            c.RunModel(@"..\..\..\..\..\SharedContent\models\SqueezeNet.onnx", bytes);
        }
    }
}
