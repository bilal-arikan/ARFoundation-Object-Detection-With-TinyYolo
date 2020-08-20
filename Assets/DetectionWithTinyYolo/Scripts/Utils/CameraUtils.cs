// using System;
// using System.Collections;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using UnityEngine;
// using UnityEngine.XR.ARSubsystems;

// public static class CameraUtils
// {

//     public static IEnumerator CameraScreenToTexture(XRCameraImage image, Action<Texture2D> onComplete)
//     {
//         Debug.Log(10);

//         // Create the async conversion request
//         var request = image.ConvertAsync(new XRCameraImageConversionParams
//         {
//             // Use the full image
//             inputRect = new RectInt(0, 0, image.width, image.height),

//             outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

//             // Color image format
//             outputFormat = TextureFormat.RGB24,

//             // Flip across the Y axis
//             transformation = CameraImageTransformation.MirrorY
//         });

//         // Wait for it to complete
//         while (!request.status.IsDone())
//             yield return null;

//         // Check status to see if it completed successfully.
//         if (request.status != AsyncCameraImageConversionStatus.Ready)
//         {
//             // Something went wrong
//             Debug.LogErrorFormat("Request failed with status {0}", request.status);

//             // Dispose even if there is an error.
//             request.Dispose();
//             yield break;
//         }

//         //
//         // !!! APP CRASH !!!
//         //
//         var rawData = request.GetData<byte>();
//         // Image data is ready. Let's apply it to a Texture2D.

//         // Create a texture if necessary

//         var lastReceived = new Texture2D(
//             request.conversionParams.outputDimensions.x,
//             request.conversionParams.outputDimensions.y,
//             request.conversionParams.outputFormat,
//             false);

//         // Copy the image data into the texture
//         lastReceived.LoadRawTextureData(rawData);
//         lastReceived.Apply();

//         Debug.Log(14);
//         // Need to dispose the request to delete resources associated
//         // with the request, including the raw data.
//         request.Dispose();
//         image.Dispose();
//         //var bytes = lastCameraTexture.EncodeToPNG();
//         //var path = Application.persistentDataPath + "/camera_texture.png";
//         //Debug.Log(Application.persistentDataPath);
//         //File.WriteAllBytes(path, bytes);

//         Debug.Log(15);
//         onComplete?.Invoke(lastReceived);
//         Debug.Log(16);
//     }

//     public static unsafe void CameraScreenToTextureUnSafe(XRCameraImage image, Action<Texture2D> onComplete)
//     {
//         var conversionParams = new XRCameraImageConversionParams
//         {
//             // Get the entire image
//             inputRect = new RectInt(0, 0, image.width, image.height),

//             // Downsample by 2
//             outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

//             // Choose RGBA format
//             outputFormat = TextureFormat.RGBA32,

//             // Flip across the vertical axis (mirror image)
//             transformation = CameraImageTransformation.MirrorY
//         };

//         // See how many bytes we need to store the final image.
//         int size = image.GetConvertedDataSize(conversionParams);

//         // Allocate a buffer to store the image
//         var buffer = new NativeArray<byte>(size, Allocator.Temp);

//         // Extract the image data
//         image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

//         // The image was converted to RGBA32 format and written into the provided buffer
//         // so we can dispose of the CameraImage. We must do this or it will leak resources.
//         image.Dispose();

//         // At this point, we could process the image, pass it to a computer vision algorithm, etc.
//         // In this example, we'll just apply it to a texture to visualize it.

//         // We've got the data; let's put it into a texture so we can visualize it.
//         var lastReceived = new Texture2D(
//             conversionParams.outputDimensions.x,
//             conversionParams.outputDimensions.y,
//             conversionParams.outputFormat,
//             false);

//         lastReceived.LoadRawTextureData(buffer);
//         lastReceived.Apply();

//         // Done with our temporary data
//         buffer.Dispose();

//         onComplete?.Invoke(lastReceived);
//     }
// }