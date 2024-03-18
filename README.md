This project is POC to check the compability of "ORT" model in Unity to mobile platforms.

**Project Overview and Unity Project setup**
1. Used onnxruntime libraries by "https://github.com/asus4"
2. List of libraries are
   1."com.github.asus4.onnxruntime": "0.1.12"
   2."com.github.asus4.onnxruntime-extensions": "0.1.12"
   3."com.github.asus4.onnxruntime.unity": "0.1.12"
   4."com.github.asus4.texture-source": "0.2.2"
   if using Linux 
     5.a. "com.github.asus4.onnxruntime.linux-x64-gpu": "0.1.12",
   or
     5.b. "com.github.asus4.onnxruntime.win-x64-gpu": "0.1.12"
 Add this lines in "manifest.json"
``
   "scopedRegistries": [
    {
      "name": "NPM",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "com.github.asus4"
      ]
    }
  ]
  "dependencies": {
    // Core library
    "com.github.asus4.onnxruntime": "0.1.12",
    // (Optional) Utilities for Unity
    "com.github.asus4.onnxruntime.unity": "0.1.12",
    ... other dependencies
  }
`` 
3. Open up the scene Named "YoloX"
4. This is the master script
    ![image](https://github.com/vinayak-vc/onnx-ort-model-unity-demo/assets/47971927/999416d4-97d5-4897-8a1c-ef1eb754d644)
    4.a Model Asset is the "ORT model"
    4.b "Virtual Texture Source" will render the out put the "Video Preview" Gameobejct
    4.c Set the "Lable File" in "Options"
       4.c.1 every new line will be considered as a new class
    4.d Set the "Prob Threshold" as per the req
    5.d Set the "Nms Threshold" as per the req
 5. ``runtimeModel = new YOLOXNew(modelAsset.bytes, options);`` This line will read the model
 6.  ``runtimeModel.Run(MinMaxLocExample.Resize(thistexture, 128, 128));`` this will convert the texture to the 128*128 and sent ot model for infrence.
 7. ``
      if (TryGetComponent(out VirtualTextureSource source)) {
          virtualTextureSource = source;
          source.OnTexture.AddListener(OnTexture);
      }
    `` This will give create the texture from the "Virtual Texture Source" and send the event "OnTexture" (Just a technique to get the texture, can be changed as per need)
8. ``public Action<List<YOLOXNew.Detection>> classificationCompleted;`` this  Action will be called when the infrence with the model is completed.
9. Check the "YOLOXNEW.cs" for Output formation in method "GenerateProposals(ReadOnlySpan<float> outputTensor, float prob_threshold)"

   
**Credits**
https://github.com/asus4

