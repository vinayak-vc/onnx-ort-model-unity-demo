using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

public class GalleryViewer : MonoBehaviour {

    public ScrollRect galleryScrollView;
    public RawImage rawImagePrefab;
    public RawImage rawImagePreview;
    public GridLayoutGroup gridLayoutGroup;
    public List<Texture2D> gallaryItem;
    public ImageRecognization imageRecognization;

    private void Start() {
        gridLayoutGroup.cellSize = Vector2.one * (Screen.width / 2 - 20);
        LoadLibrary();
    }
    public void LoadLibrary() {
        foreach (var VARIABLE in Directory.GetFiles(Application.persistentDataPath, "*.png")) {
            if (VARIABLE.Contains("Nothing") || gallaryItem.FindIndex(texture2D => texture2D.name == Path.GetFileName(VARIABLE)) != -1) {
                continue;
            }
            RawImage rawImage = Instantiate(rawImagePrefab, gridLayoutGroup.transform);
            rawImage.texture = CreateTexture(VARIABLE);
            rawImage.GetComponent<Button>().onClick.AddListener(delegate {
                MediaFileClickEvent((Texture2D)rawImage.texture);
            });
            gallaryItem.Add((Texture2D)rawImage.texture);
        }
        if (gallaryItem != null) {
            rawImagePreview.texture = gallaryItem[0];
        }
    }

    public Texture2D CreateTexture(string filePath) {
        Texture2D texture2D = new Texture2D(1440, 1440);
        // Load image data into the Texture2D
        texture2D.LoadImage(File.ReadAllBytes(filePath));
        texture2D.Apply();
        texture2D.name = Path.GetFileName(filePath);
        return texture2D;
    }

    public void MediaFileClickEvent(Texture2D texture2D) {
        imageRecognization.backToStream = false;
        imageRecognization.TakePicture(texture2D);
        galleryScrollView.gameObject.SetActive(false);
    }
}
