using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BG_Library.Common
{
	public static class FileLib
	{
		#region Download
		public static void DownloadText(MonoBehaviour mono, string URL
			, System.Action<bool, string> onComplete, System.Action<float> onProgress = null)
		{
			mono.StartCoroutine(DownloadTextIE(URL, onComplete, onProgress));
		}

		public static IEnumerator DownloadTextIE(string URL, System.Action<bool, string> onComplete
			, System.Action<float> onProgress = null)
		{
			UnityWebRequest www = UnityWebRequest.Get(URL);
			www.SendWebRequest();

			while (!www.isDone)
			{
				onProgress?.Invoke(www.downloadProgress);
				yield return null;
			}

			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError("Error: " + www.error);
				onComplete?.Invoke(false, null);
			}
			else
			{
				onComplete?.Invoke(true, www.downloadHandler.text);
			}
		}

		/// <summary>
		/// Download va luu video vao bo nho thiet bi
		/// </summary>
		/// <param name="mono">Mono de chay Coroutine</param>
		/// <param name="videoURL">Link download video</param>
		/// <param name="shortPath">Duong dan luu video, VD: videos/v1.mp4</param>
		/// <param name="onComplete">khi hoan thanh tra ve fullPath local de chay video</param>
		/// <param name="onProgress">Event cap nhat tien trinh download</param>
		public static void DownloadAndSaveVideo(MonoBehaviour mono, string videoURL
			, string shortPath, System.Action<bool, string> onComplete, System.Action<float> onProgress = null)
		{
			mono.StartCoroutine(DownloadAndSaveVideoIE(videoURL, shortPath, onComplete, onProgress));
		}

		/// <summary>
		/// Download va luu video vao bo nho thiet bi
		/// </summary>
		/// <param name="videoURL">Link download video</param>
		/// <param name="shortPath">Duong dan luu video, VD: videos/v1.mp4</param>
		/// <param name="onComplete">khi hoan thanh tra ve fullPath local de chay video</param>
		/// <param name="onProgress">Event cap nhat tien trinh download</param>
		public static IEnumerator DownloadAndSaveVideoIE(string videoURL, string shortPath
			, System.Action<bool, string> onComplete, System.Action<float> onProgress = null)
		{
			UnityWebRequest wwwVideo = UnityWebRequest.Get(videoURL);

			wwwVideo.SendWebRequest();
			while (!wwwVideo.isDone)
			{
				onProgress?.Invoke(wwwVideo.downloadProgress);
				yield return null;
			}

			if (wwwVideo.result == UnityWebRequest.Result.ConnectionError
				|| wwwVideo.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError("Error: " + wwwVideo.error);
				onComplete?.Invoke(false, null);
			}
			else
			{
				var fullPath = ConvertShortPathToFullPath(shortPath);
				var writeTask = File.WriteAllBytesAsync(fullPath, wwwVideo.downloadHandler.data);
;
				yield return new WaitUntil(()=> writeTask.IsCompleted);
				onComplete?.Invoke(true, fullPath);
			}
		}

		public static void DownloadTexture(MonoBehaviour mono, string imageURL
			, System.Action<bool, Sprite> onComplete, System.Action<float> onProgress = null)
		{
			mono.StartCoroutine(DownloadTextureIE(imageURL, onComplete, onProgress));
		}

		/// <summary>
		/// Download texture
		/// </summary>
		/// <param name="imageURL">Link download texture</param>
		/// <param name="onComplete">Event khi hoan thanh download</param>
		/// <param name="onProgress">Event cap nhat tien trinh download</param>
		/// <returns></returns>
		public static IEnumerator DownloadTextureIE(string imageURL
			, System.Action<bool, Sprite> onComplete, System.Action<float> onProgress = null)
		{
			UnityWebRequest wwwTexture = UnityWebRequestTexture.GetTexture(imageURL);

			wwwTexture.SendWebRequest();
			while (!wwwTexture.isDone)
			{
				onProgress?.Invoke(wwwTexture.downloadProgress);
				yield return null;
			}

			if (wwwTexture.result == UnityWebRequest.Result.ConnectionError 
				|| wwwTexture.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError("Error: " + wwwTexture.error);
				onComplete?.Invoke(false, null);
			}
			else
			{
				Texture2D loadedTexture = DownloadHandlerTexture.GetContent(wwwTexture);
				onComplete?.Invoke(true, Sprite.Create(loadedTexture
					, new Rect(0f, 0f, loadedTexture.width, loadedTexture.height), Vector2.zero));
			}
		}
		#endregion

		#region Sprite
		public static byte[] ConvertSpriteToBytes(Sprite sp)
		{
			return sp.texture.EncodeToPNG();
		}

		public static Sprite ConvertBytesToSprite(byte[] bytes)
		{
			Texture2D texture = new Texture2D(2, 2);
			texture.filterMode = FilterMode.Trilinear;
			texture.LoadImage(bytes);

			return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
		}
		#endregion

		#region File
		/// <summary>
		/// Luu Asset vao duong dan
		/// </summary>
		/// <param name="shortPath">Duong dan cua Asset, VD: images/pic1.png</param>
		/// <param name="bytes"></param>
		public static void SaveFileOnDisk(string shortPath, byte[] bytes)
		{
			File.WriteAllBytes(ConvertShortPathToFullPath(shortPath), bytes);
		}

		public static Task SaveFileOnDiskAsync(string shortPath, byte[] bytes)
		{
			return File.WriteAllBytesAsync(ConvertShortPathToFullPath(shortPath), bytes);
		}

		/// <summary>
		/// Load Asset tu duong dan
		/// </summary>
		/// <param name="shortPath">Duong dan cua Asset, VD: images/pic1.png</param>
		/// <returns></returns>
		public static byte[] LoadFileOnDisk(string shortPath)
		{
			if (File.Exists(ConvertShortPathToFullPath(shortPath)))
			{
				return File.ReadAllBytes(ConvertShortPathToFullPath(shortPath));
			}

			return null;
		}

		public static Task<byte[]> LoadFileOnDiskAsync(string shortPath)
		{
			if (File.Exists(ConvertShortPathToFullPath(shortPath)))
			{
				return File.ReadAllBytesAsync(ConvertShortPathToFullPath(shortPath));
			}

			return null;
		}
		#endregion

		public static string ConvertShortPathToFullPath(string shortPath)
		{
			return $"{Application.persistentDataPath}/{shortPath}";
		}
	}
}