## About The Project

**Project Overview and Unity Project Setup**

This Proof of Concept (POC) project aims to assess the compatibility of the "ORT" model in Unity for mobile platforms.

## Getting Started

**Libraries Used**
1. **onnxruntime Libraries**: Obtained from [GitHub - asus4](https://github.com/asus4)
   - `com.github.asus4.onnxruntime`: Version 0.1.12
   - `com.github.asus4.onnxruntime-extensions`: Version 0.1.12
   - `com.github.asus4.onnxruntime.unity`: Version 0.1.12
   - `com.github.asus4.texture-source`: Version 0.2.2
   - If using Linux:
     - `com.github.asus4.onnxruntime.linux-x64-gpu`: Version 0.1.12
   - If using Windows:
     - `com.github.asus4.onnxruntime.win-x64-gpu`: Version 0.1.12

## Unity Project Setup:
1. Add the following lines to your `manifest.json`:
   ```json
   "scopedRegistries": [
     {
       "name": "NPM",
       "url": "https://registry.npmjs.com",
       "scopes": [
         "com.github.asus4"
       ]
     }
   ],
   "dependencies": {
     "com.github.asus4.onnxruntime": "0.1.12",
     "com.github.asus4.onnxruntime.unity": "0.1.12",
     ... // other dependencies
   }

## Usage

1. Open the scene named "YoloX".
2. Master Script Overview:
    *   ![image](https://github.com/vinayak-vc/onnx-ort-model-unity-demo/assets/47971927/999416d4-97d5-4897-8a1c-ef1eb754d644)
    *   Model Asset: Assign the "ORT model".
    *   Virtual Texture Source: Renders the output to the "Video Preview" GameObject.
    *   Label File: Set the file in "Options".
        *   Each new line represents a new class.
    *   Probability Threshold: Set as per requirements.
    *   NMS Threshold: Set as per requirements.
    *   Line of Inference:
       *   ```runtimeModel = new YOLOXNew(modelAsset.bytes, options);``` - Reads the model.
       *   ```runtimeModel.Run(MinMaxLocExample.Resize(thistexture, 128, 128));``` - Converts the texture to 128x128 and sends it to the model for inference.
4. Texture Handling:
        If a VirtualTextureSource component exists, it creates the texture from it and triggers the event OnTexture.
5. Event Handling:
        ```public Action<List<YOLOXNew.Detection>> classificationCompleted;``` - Triggered when inference with the model is completed.
6. Output Formation:
        Check ```YOLOXNEW.cs``` for output formation in the method ```GenerateProposals(ReadOnlySpan<float> outputTensor, float prob_threshold)```.
   
**Converting ONNX Model to ORT:**

Use the following command to convert the model to ORT:

```python -m onnxruntime.tools.convert_onnx_models_to_ort <onnx_model_file_or_directory>```

This command exports the model to the ONNX model directory. Use -runtime.ort model for conversion.

**Model Visualization**

Inspect the input and output of the model on [Netron](https://netron.app/).

**Credits**

The libraries used in this project are provided by [https://github.com/asus4](https://github.com/asus4) asus4.

[https://github.com/asus4/onnxruntime-unity](https://github.com/asus4/onnxruntime-unity) onnxruntime-unity

[https://github.com/asus4/TextureSource](https://github.com/asus4/TextureSource) TextureSource
