using System;
using System.Collections.Generic;
using System.IO;
using Windows.AI.MachineLearning;
using SqueezeNet.Library;

namespace SqueezeNetObjectDetectionNC
{
    class ImageInference
    {
        // globals
        private static string _modelPath;
        private static string _imagePath;
        private static LearningModel _model = null;
        private static LearningModelSession _session;
        private static List<string> _labels = new List<string>();

        // usage: SqueezeNet [modelfile] [imagefile] [cpu|directx]
        static int Main(string[] args)
        {
            if (!ParseArgs(args))
            {
                Console.WriteLine("Usage: [executable_name] [modelfile] [imagefile] [cpu|directx]");
                return -1;
            }
            ObjectDetectionModel c = new ObjectDetectionModel();
            c.RunModel(_modelPath, _imagePath);
            
            return 0;
        }

        static bool ParseArgs(string[] args)
        {
            if (args.Length < 2)
            {
                return false;
            }
            // get the model file
            _modelPath = args[0];
            // get the image file
            _imagePath = args[1];
            if (!Path.IsPathFullyQualified(_imagePath))
            {
                _imagePath = Path.GetFullPath(_imagePath);
            }
            // did they pass a fourth arg?

            if (args.Length > 2)
            {
                string deviceName = args[2];
                if (deviceName == "cpu")
                {
                    _deviceKind = LearningModelDeviceKind.Cpu;
                }
                else if (deviceName == "directx")
                {
                    _deviceKind = LearningModelDeviceKind.DirectX;
                }
            }
            return true;
        }
    }
}