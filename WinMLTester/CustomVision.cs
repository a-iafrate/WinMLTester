using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 3f18cf6b-a582-4ef4-9444-6e3cd53bbb0a_1d4eb8eb-bd41-44b4-b7a5-b2146eb69fa4

namespace WinMLTester
{
    public sealed class CustomVisionModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class CustomVisionModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public CustomVisionModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
            {
                { "casa", float.NaN },
            };
        }
    }

    public sealed class CustomVisionModel
    {
        private LearningModelPreview learningModel;
        public static async Task<CustomVisionModel> CreateCustomVisionModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            CustomVisionModel model = new CustomVisionModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<CustomVisionModelOutput> EvaluateAsync(CustomVisionModelInput input) {
            CustomVisionModelOutput output = new CustomVisionModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
