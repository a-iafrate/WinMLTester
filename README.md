# WinMLTester

WARNING
---------------------------------------------------------

The latest version is only compatible with Windows 10 RS5 and Windows 10 SDK > 17723.  Custom Vision service is not compatible (for now) with Onnx standard v1.2 on RS5 so use only compatible models like this
https://gallery.azure.ai/Model/FER-Emotion-Recognition-1-2-2

Info
---------------------------------------------------------
This app help to test custom vision model created with Custom Vision Service.

**Create your model**

How to create your model
- Go to [Custom Vision Website](https://www.customvision.ai)
- Create your project
- Train network
- Go to performance tab and export onnx file

**How to use**

Launch application, load your onnx file and choose your source to test



 **Test your network with**

- Single Image
- Image Folder
- Webcam
- Video

**Todo**

- Use Custom Vision online network
- Use other neural network other then Custom Vision