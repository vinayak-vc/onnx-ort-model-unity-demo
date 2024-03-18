using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class MinMaxLocExample {
    public static (float minScore, float maxScore, int minClassLoc, int maxClassLoc) FindMinMaxLoc(float[] classesScores) {
        // Find minimum and maximum values and their locations using LINQ
        float minScore = classesScores.Min();
        float maxScore = classesScores.Max();
        int minClassLoc = Array.IndexOf(classesScores, minScore);
        int maxClassLoc = Array.IndexOf(classesScores, maxScore);

        return (minScore, maxScore, minClassLoc, maxClassLoc);
    }

    public static List<YOLOXNew.Detection> PerformNms(List<YOLOXNew.Detection> boxes, float scoreThreshold, float iouThreshold) {
        List<YOLOXNew.Detection> selectedBoxes = new List<YOLOXNew.Detection>();

        // Sort boxes based on scores (descending order)
        List<int> indices = new List<int>();
        for (int i = 0; i < boxes.Count; i++) {
            indices.Add(i);
        }
        indices.Sort((a, b) => boxes[b].probability.CompareTo(boxes[b].probability));

        while (indices.Count > 0) {
            int topIndex = indices[0];
            float topScore = boxes[topIndex].probability;

            if (topScore < scoreThreshold) {
                break;
            }

            selectedBoxes.Add(boxes[topIndex]);
            indices.RemoveAt(0);

            for (int i = indices.Count - 1; i >= 0; i--) {
                int index = indices[i];
                float iou = CalculateIOU(boxes[topIndex].rect, boxes[index].rect);

                if (iou > iouThreshold) {
                    indices.RemoveAt(i);
                }
            }
        }

        return selectedBoxes;
    }
    // Method to transpose a 3D array (tensor)
    public static float[,,] Transpose(float[,,] tensor) {
        int dim0 = tensor.GetLength(0);
        int dim1 = tensor.GetLength(1);
        int dim2 = tensor.GetLength(2);

        // Create a new 3D array to store the transposed tensor
        float[,,] transposedTensor = new float[dim0, dim2, dim1];

        // Iterate through each slice along the third dimension and transpose it
        for (int i = 0; i < dim0; i++) {
            for (int j = 0; j < dim1; j++) {
                for (int k = 0; k < dim2; k++) {
                    transposedTensor[i, k, j] = tensor[i, j, k];
                }
            }
        }

        return transposedTensor;
    }
    public static (List<Rect>, List<float>, List<int>) PerformNMS(List<Rect> boxes, List<float> scores, List<int> classIDs, float scoreThreshold, float iouThreshold) {
        List<Rect> selectedBoxes = new List<Rect>();
        List<float> selectedScores = new List<float>();
        List<int> selectedClassids = new List<int>();

        // Sort boxes based on scores (descending order)
        List<int> indices = new List<int>();
        for (int i = 0; i < scores.Count; i++) {
            indices.Add(i);
        }
        indices.Sort((a, b) => scores[b].CompareTo(scores[a]));

        while (indices.Count > 0) {
            int topIndex = indices[0];
            float topScore = scores[topIndex];

            if (topScore < scoreThreshold) {
                break;
            }

            selectedBoxes.Add(boxes[topIndex]);
            selectedScores.Add(scores[topIndex]);
            selectedClassids.Add(classIDs[topIndex]);
            indices.RemoveAt(0);

            for (int i = indices.Count - 1; i >= 0; i--) {
                int index = indices[i];
                float iou = CalculateIOU(boxes[topIndex], boxes[index]);

                if (iou > iouThreshold) {
                    indices.RemoveAt(i);
                }
            }
        }

        return (selectedBoxes, selectedScores, selectedClassids);
    }
    public static Texture2D Resize(Texture texture2D, int targetX, int targetY) {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    public static Texture2D CropTexture(RectTransform cropRect, Transform canvas, Texture2D originalTexture) {
        // Create a new texture to hold the cropped image
        Texture2D croppedTexture = new Texture2D(
            (int)(cropRect.sizeDelta.x), //* canvas.localScale.x),
            (int)(cropRect.sizeDelta.y)); //* canvas.localScale.y));

        // Get the pixels from the original texture within the crop rectangle
        Color[] originalPixels = originalTexture.GetPixels((int)cropRect.anchoredPosition.x, (int)cropRect.anchoredPosition.y, croppedTexture.width, croppedTexture.height);
        // Set the pixels of the cropped texture
        croppedTexture.SetPixels(originalPixels);
        croppedTexture.Apply(); // Apply changes to the cropped texture
        return croppedTexture;
    }
    static float CalculateIOU(Rect boxA, Rect boxB) {
        float xA = Mathf.Max(boxA.xMin, boxB.xMin);
        float yA = Mathf.Max(boxA.yMin, boxB.yMin);
        float xB = Mathf.Min(boxA.xMax, boxB.xMax);
        float yB = Mathf.Min(boxA.yMax, boxB.yMax);

        float interArea = Mathf.Max(0, xB - xA + 1) * Mathf.Max(0, yB - yA + 1);

        float boxAArea = (boxA.width + 1) * (boxA.height + 1);
        float boxBArea = (boxB.width + 1) * (boxB.height + 1);

        float iou = interArea / (boxAArea + boxBArea - interArea);
        return iou;
    }
    public static string CreateName(NameOfMedia[] nameOfMedia) {
        return nameOfMedia.Aggregate("", (current, t) => current + $"{t.objectclass} {t.x} {t.y} {t.width} {t.height}\n");
    }
    public class NameOfMedia {
        public int objectclass;
        public float x;
        public float y;
        public float width;
        public float height;
    }
}
