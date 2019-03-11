using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SqueezeNet.Library
{
    public class ObjectDetectionModel
    {
        private LearningModel _model;
        private LearningModelSession _session;
        private static string _deviceName = "default";
        private static List<string> _labels = new List<string>();        
        private static string _labelsFileName = "Assets/Labels.json";

        public void RunModel(string modelPath, byte[] image)
        {
            // Load and create the model 
            Console.WriteLine($"Loading modelfile '{modelPath}' on the '{_deviceName}' device");

            int ticks = Environment.TickCount;
            _model = LearningModel.LoadFromFilePath(modelPath);
            ticks = Environment.TickCount - ticks;
            Console.WriteLine($"model file loaded in { ticks } ticks");

            // Create the evaluation session with the model and device
            _session = new LearningModelSession(_model, new LearningModelDevice(LearningModelDeviceKind.Default));

            Console.WriteLine("Loading the image...");
            ImageFeatureValue imageTensor = LoadImageFile(image);

            // create a binding object from the session
            Console.WriteLine("Binding...");
            LearningModelBinding binding = new LearningModelBinding(_session);
            binding.Bind(_model.InputFeatures.ElementAt(0).Name, imageTensor);

            Console.WriteLine("Running the model...");
            ticks = Environment.TickCount;
            LearningModelEvaluationResult results = _session.Evaluate(binding, "RunId");
            ticks = Environment.TickCount - ticks;
            Console.WriteLine($"model run took { ticks } ticks");

            // retrieve results from evaluation
            TensorFloat resultTensor = results.Outputs[_model.OutputFeatures.ElementAt(0).Name] as TensorFloat;
            IReadOnlyList<float> resultVector = resultTensor.GetAsVectorView();

            foreach ( float item in resultVector )
            {
                Console.WriteLine(item);
            }

            PrintResults(resultVector, _labelsFileName);
        }

        

        private static void LoadLabels(string labelsFileName)
        {
            // Parse labels from label json file.  We know the file's 
            // entries are already sorted in order.
            string fileString = File.ReadAllText(labelsFileName);
            Dictionary<string, string> fileDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileString);
            foreach (KeyValuePair<string, string> kvp in fileDict)
            {
                _labels.Add(kvp.Value);
            }
        }

        
        private static T AsyncHelper<T> (IAsyncOperation<T> operation) 
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            operation.Completed = new AsyncOperationCompletedHandler<T>((op, status) =>
            {
                waitHandle.Set();
            });
            waitHandle.WaitOne();
            return operation.GetResults();
        }

        private static ImageFeatureValue LoadImageFile(byte[] image)
        {
            IRandomAccessStream ras2 = image.AsBuffer().AsStream().AsRandomAccessStream();
            BitmapDecoder decoder = AsyncHelper(BitmapDecoder.CreateAsync(ras2));
            SoftwareBitmap softwareBitmap = AsyncHelper(decoder.GetSoftwareBitmapAsync());
            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            return ImageFeatureValue.CreateFromVideoFrame(inputImage);
        }

        private static void PrintResults(IReadOnlyList<float> resultVector, string labelsFileName)
        {
            // load the labels
            LoadLabels(labelsFileName);

            List<(int index, float probability)> indexedResults = new List<(int, float)>();
            for (int i = 0; i < resultVector.Count; i++)
            {
                indexedResults.Add((index: i, probability: resultVector.ElementAt(i)));
            }
            indexedResults.Sort((a, b) =>
            {
                if (a.probability < b.probability)
                {
                    return 1;
                }
                else if (a.probability > b.probability)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"\"{ _labels[indexedResults[i].index]}\" with confidence of { indexedResults[i].probability}");
            }
        }
    }
}
