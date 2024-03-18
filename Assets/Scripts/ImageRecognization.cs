using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.ML.OnnxRuntime.Unity;

using TextureSource;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ImageRecognization : MonoBehaviour {
    [SerializeField] private YOLOXNew.Options options;
    public List<YOLOXNew.Detection> detections;
    public RawImage captureTexture;
    public RectTransform captureArea;
    public OrtAsset modelAsset;
    public AudioSource photoCaptured;
    public Image photoCaptureButton;
    public Sprite backButtonSprite;
    public Animation photoCapturedAnimation;
    private YOLOXNew runtimeModel;
    private IEnumerator updateDetectionBox;
    private Texture texture;
    private bool isPictureTaken;
    private Sprite cameraButtonSprite;

    private VirtualTextureSource virtualTextureSource;
    internal bool backToStream;
    private bool fromDirectFile;

    [Header("Visualization Options")]
    [SerializeField]
    private TMPro.TMP_Text detectionBoxPrefab;

    [SerializeField]
    private RectTransform detectionContainer;

    [SerializeField]
    private int maxDetections = 20;

    private TMP_Text[] detectionBoxes;
    private readonly StringBuilder sb = new();

    private void Start() {
        runtimeModel = new YOLOXNew(modelAsset.bytes, options);
        runtimeModel.classificationCompleted += UpdateDetectionBox;
        detectionBoxes = new TMPro.TMP_Text[maxDetections];
        for (int i = 0; i < maxDetections; i++) {
            TMP_Text box = Instantiate(detectionBoxPrefab, detectionContainer.GetChild(0));
            box.name = $"Detection {i}";
            box.gameObject.SetActive(false);
            detectionBoxes[i] = box;
        }
        if (TryGetComponent(out VirtualTextureSource source)) {
            virtualTextureSource = source;
            source.OnTexture.AddListener(OnTexture);
        }
        cameraButtonSprite = photoCaptureButton.sprite;
    }

    public void TakePicture() {
        fromDirectFile = false;
        TakePicture(null);
    }
    public void TakePicture(Texture2D texture2D = null) {
        if (!backToStream) {
            Texture2D thistexture = null;
            if (texture2D != null) {
                fromDirectFile = true;
                thistexture = texture2D;
            } else {
                photoCapturedAnimation.Play();
                photoCaptured.Play();
                thistexture = MinMaxLocExample.CropTexture(captureArea, captureArea.root,
                    MinMaxLocExample.Resize(this.texture,
                        (int)detectionContainer.rect.width,
                        (int)detectionContainer.rect.height));
            }
            captureTexture.texture = thistexture;
            runtimeModel.Run(MinMaxLocExample.Resize(thistexture, 128, 128));
            captureTexture.gameObject.SetActive(true);
            virtualTextureSource.enabled = false;
            photoCaptureButton.sprite = backButtonSprite;
        } else {
            BackToStream();
        }
        backToStream = !backToStream;
    }

    private void BackToStream() {
        photoCaptureButton.sprite = cameraButtonSprite;
        virtualTextureSource.enabled = true;
        captureTexture.gameObject.SetActive(false);

        for (int i = 0; i < maxDetections; i++) {
            detectionBoxes[i].gameObject.SetActive(false);
        }
    }
    private void OnDestroy() {
        if (TryGetComponent(out VirtualTextureSource source)) {
            source.OnTexture.RemoveListener(OnTexture);
        }
        runtimeModel.classificationCompleted -= UpdateDetectionBox;
        runtimeModel?.Dispose();
    }
    private void OnTexture(Texture texture) {
        this.texture = texture;

        // runtimeModel.Run(MinMaxLocExample.Resize(this.captureTexture.texture, 128, 128));
        // UpdateDetectionBox(runtimeModel.Detections);
    }

    private void UpdateDetectionBox(List<YOLOXNew.Detection> detections) {
        List<string> labels = runtimeModel.labelNames;
        Vector2 viewportSize = detectionContainer.rect.size;
        this.detections = new List<YOLOXNew.Detection>();
        this.detections.AddRange(detections);
        int length = Math.Min(detections.Count, maxDetections);
        MinMaxLocExample.NameOfMedia[] nameOfMedia = new MinMaxLocExample.NameOfMedia[length];
        int i;
        for (i = 0; i < length; i++) {
            YOLOXNew.Detection detection = detections[i];
            string label = labels[detection.label];

            TMP_Text box = detectionBoxes[i];

            switch (detection.label) {
                case 0:
                    box.color = Color.green;
                    box.transform.GetChild(0).GetComponent<Image>().color = Color.green;
                    break;
                case 1:
                    box.color = Color.yellow;
                    box.transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
                    break;
                case 2:
                    box.color = Color.red;
                    box.transform.GetChild(0).GetComponent<Image>().color = Color.red;
                    break;
                default:
                    break;
            }
            box.gameObject.SetActive(true);

            // Using StringBuilder to reduce GC
            sb.Clear();
            sb.Append(label);
            sb.Append(Environment.NewLine + (int)(detection.probability * 100));
            sb.Append('%');
            box.SetText(sb);
            // The detection rect is model space
            // Needs to be converted to viewport space
            RectTransform rt = box.rectTransform;
            float scaleFactorX = captureArea.rect.size.x / 128;
            float scaleFactorY = captureArea.rect.size.y / 128;

            //detection.rect = runtimeModel.ConvertToViewport(detection.rect);
            // rt.sizeDelta = rect.size * scaleFactor;
            // float posX = rect.position.x - (rect.size.x / 2) * (inputTexture1.width / 128);
            // float posY = rect.position.y - (rect.size.y / 2) * -1 * (inputTexture1.height / 128);
            rt.anchoredPosition = detection.rect.min * new Vector2(scaleFactorX, -scaleFactorY);
            rt.sizeDelta = detection.rect.size * new Vector2(scaleFactorX, scaleFactorY);

            // rt.anchoredPosition = rect.min * viewportSize;
            // rt.sizeDelta = rect.size * viewportSize;
            nameOfMedia[i] = new MinMaxLocExample.NameOfMedia() {
                objectclass = detection.label,
                x = detection.rect.xMin,
                y = detection.rect.yMin,
                width = detection.rect.width,
                height = detection.rect.height
            };
        }
        // Hide unused boxes
        for (; i < maxDetections; i++) {
            detectionBoxes[i].gameObject.SetActive(false);
        }
        if (!fromDirectFile) {
            string fileName = MinMaxLocExample.CreateName(nameOfMedia);
            if (fileName == "") {
                fileName = "Nothing " + DateTime.UtcNow.ToString("hmmss");
            }
            S3Uploader.UploadFile(this, fileName.Remove(fileName.Length - 1, 1) + ".png", ((Texture2D)captureTexture.texture).EncodeToPNG());
        }
    }
}
