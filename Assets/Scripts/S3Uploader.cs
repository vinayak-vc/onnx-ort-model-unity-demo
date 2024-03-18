using UnityEngine;

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using UnityEngine.Networking;

public class S3Uploader {
    public static void UploadFile(MonoBehaviour mono, string fileName, byte[] fileByte) {
        Debug.Log("https://3d-model-construction.s3.ap-south-1.amazonaws.com/wallcrack/" + fileName.Replace(' ', '+'));
        string filepath = Path.Combine(Application.persistentDataPath, fileName.Replace(' ', '+'));
        File.WriteAllBytes(filepath, fileByte);
        mono.StartCoroutine(UploadFile(fileName, fileByte));
    }
    static IEnumerator UploadFile(string fileName, byte[] fileByte) {

        string url = $"https://3d-model-construction.s3.ap-south-1.amazonaws.com/wallcrack/{fileName}";
        string contentType = "application/octet-stream";

        // Create HTTP PUT request
        UnityWebRequest request = UnityWebRequest.Put(url, fileByte);
        request.method = UnityWebRequest.kHttpVerbPUT;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error uploading file: " + request.downloadHandler.text);
        } else {
            Debug.Log("File uploaded successfully!");
        }
    }
}
