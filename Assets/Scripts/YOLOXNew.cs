using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.ML.OnnxRuntime.Unity;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Mathematics;

public class YOLOXNew : ImageInference<float> {
    /// <summary>
    /// Options for Yolox
    /// </summary>
    [Serializable]
    public class Options : ImageInferenceOptions {
        [Header("Yolox options")]
        public TextAsset labelFile;
        [Range(1, 100)]
        public int maxDetections = 100;
        [Range(0f, 1f)]
        public float probThreshold = 0.3f;
        [Range(0f, 1f)]
        public float nmsThreshold = 0.45f;
    }
    [Serializable]
    public struct Detection : IComparable<Detection> {
        public int label;
        public Rect rect;
        public float probability;

        public Detection(Rect rect, int label, float probability) {
            this.rect = rect;
            this.label = label;
            this.probability = probability;
        }

        public int CompareTo(Detection other) {
            return other.probability.CompareTo(probability);
        }
    }
    private const int NUM_CLASSES = 3;
    private readonly Options options;
    private int detectionCount = 0;
    public List<string> labelNames = new List<string>();
    public List<Detection> Detections;

    public Action<List<YOLOXNew.Detection>> classificationCompleted;
    public YOLOXNew(byte[] model, Options options)
        : base(model, options) {
        this.options = options;

        int maxDetections = options.maxDetections;
        string[] labels = options.labelFile.text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        labelNames.AddRange(labels);
        Assert.AreEqual(NUM_CLASSES, labelNames.Count);
    }

    public override void Dispose() {
        base.Dispose();
    }

    protected override void PostProcess() {
        ReadOnlySpan<float> output = outputs[0].GetTensorDataAsSpan<float>();
        List<Detection> proposals = GenerateProposals(output, options.probThreshold);
        Detections = MinMaxLocExample.PerformNms(proposals, options.probThreshold, options.nmsThreshold);
        classificationCompleted?.Invoke(Detections);

    }
    public Rect ConvertToViewport(in Rect rect) {
        Rect unityRect = rect.FlipY();
        var mtx = InputToViewportMatrix;
        Vector2 min = mtx.MultiplyPoint3x4(unityRect.min);
        Vector2 max = mtx.MultiplyPoint3x4(unityRect.max);
        return new Rect(min, max - min);
    }
    private List<Detection> GenerateProposals(ReadOnlySpan<float> outputTensor, float prob_threshold) {
        float[,,] outputFloats = new float[1, 7, 336];
        int layer_Max = outputFloats.GetLength(0);
        int rows_max = outputFloats.GetLength(1);
        int colomuns_max = outputFloats.GetLength(2);
        List<Detection> detections = new List<Detection>();

        for (int i = 0; i < layer_Max; i++) {
            for (int j = 0; j < rows_max; j++) {
                for (int k = 0; k < colomuns_max; k++) {
                    // Calculate the index in the one-dimensional array
                    int index = i * rows_max * colomuns_max + j * colomuns_max + k;

                    // Assign the value from the one-dimensional array to the corresponding position in the three-dimensional array
                    outputFloats[i, j, k] = outputTensor[index];
                }
            }
        }

        outputFloats = MinMaxLocExample.Transpose(outputFloats);

        rows_max = outputFloats.GetLength(2);
        colomuns_max = outputFloats.GetLength(1);

        //Copy data from one-dimensional array to three-dimensional array
        for (int colomuns = 0; colomuns < colomuns_max; colomuns++) {

            List<float> classis_scores = new List<float>();
            for (int rows = 4; rows < rows_max; rows++) {
                float score = outputFloats[0, colomuns, rows]; // Extract the score
                classis_scores.Add(score);
            }
            // Slice the row to exclude the first 4 elements and add it to the list of class scores
            (float minScore, float maxScore, int minClassLoc, int maxClassLoc) = MinMaxLocExample.FindMinMaxLoc(classis_scores.ToArray());

            if (maxScore < prob_threshold)
                continue;

            // Extract the bounding box coordinates, score, and class ID from each element
            Rect box = new Rect {
                // ReSharper disable once PossibleLossOfFraction
                xMin = outputFloats[0, colomuns, 0] - (0.5f * outputFloats[0, colomuns, 2]), // ReSharper disable once PossibleLossOfFraction
                yMin = outputFloats[0, colomuns, 1] - (0.5f * outputFloats[0, colomuns, 3]),
                // width = outputFloats[0, colomuns, 2], // Extract the x2 coordinate
                // height = outputFloats[0, colomuns, 3] // Extract the y2 coordinate
            }; // Assuming each box has 4 coordinates (x1, y1, x2, y2)

            box.xMax = box.xMin + outputFloats[0, colomuns, 2];
            box.yMax = box.yMin + outputFloats[0, colomuns, 3];

            Detection detection = new Detection(box, maxClassLoc, maxScore);
            detections.Add(detection);
        }
        return detections;
    }
}
