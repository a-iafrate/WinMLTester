using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning;


// 3f18cf6b-a582-4ef4-9444-6e3cd53bbb0a_1d4eb8eb-bd41-44b4-b7a5-b2146eb69fa4

namespace WinMLTester
{
    public sealed class Input
    {
        public ImageFeatureValue image; // shape(-1,3,416,416)
    }

    public sealed class Output
    {
        public TensorFloat grid; // shape(-1,125,13,13)
    }

    public sealed class CustomVisionModel
    {


        private LearningModel model;
        private LearningModelSession session;
        private LearningModelBinding binding;
        private string inputParameterName = "";
        private string outputParameterName = "";
        public int inputWidth = 0;
        public int inputHeight = 0;

        public static CustomVisionModel CreateFromFilePath(string path)
        {
            CustomVisionModel learningModel = new CustomVisionModel();
            learningModel.model = LearningModel.LoadFromFilePath(path);
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }

        public static async Task<CustomVisionModel> CreateFromStorageFile(StorageFile file)
        {
            CustomVisionModel learningModel = new CustomVisionModel();
            learningModel.model = await LearningModel.LoadFromStorageFileAsync(file);
            IReadOnlyList<ILearningModelFeatureDescriptor> input = learningModel.model.InputFeatures.ToList();
            MapFeatureDescriptor imgDesc = input[0] as MapFeatureDescriptor;

            TensorFeatureDescriptor tfDesc = input[0] as TensorFeatureDescriptor;
            learningModel.inputParameterName = input[0].Name;
            learningModel.inputWidth = (int)tfDesc.Shape[2];
            learningModel.inputHeight = (int)tfDesc.Shape[3];
            IReadOnlyList<ILearningModelFeatureDescriptor> output = learningModel.model.OutputFeatures.ToList();
            MapFeatureDescriptor imgDesc1 = output[0] as MapFeatureDescriptor;
            TensorFeatureDescriptor tfDesc1 = output[0] as TensorFeatureDescriptor;
            learningModel.outputParameterName = output[0].Name;
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }

        public async Task<Output> Evaluate(Input input)
        {
            binding.Clear();
            binding.Bind(inputParameterName, input.image);
            var result = await session.EvaluateAsync(binding, "0");
            var output = new Output();
            output.grid = result.Outputs[outputParameterName] as TensorFloat;
            return output;
        }
    }
}
