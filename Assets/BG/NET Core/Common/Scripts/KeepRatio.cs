using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.Common
{
	/// <summary>
	/// Giu cho ti le khong bi thay doi
	/// </summary>
	public class KeepRatio : MonoBehaviour
	{
		[BoxGroup("CONFIGS"), SerializeField] RectTransform sampleRect;
		[BoxGroup("CONFIGS"), SerializeField] Mode mode;
		[BoxGroup("CONFIGS"), SerializeField] Vector2 sampleRatio;

		[BoxGroup("CONFIGS"), SerializeField] bool playAwake = true;

		public RectTransform SampleRect { get => sampleRect; set => sampleRect = value; }
		public Mode Mode1 { get => mode; set => mode = value; }
		public Vector2 SampleRatio { get => sampleRatio; set => sampleRatio = value; }

		public bool PlayAwake { get => playAwake; set => playAwake = value; }

		RectTransform rectTrans;
		public RectTransform RectTrans
		{
			get
			{
				if (rectTrans == null)
				{
					rectTrans = GetComponent<RectTransform>();
				}

				return rectTrans;
			}
		}

		public enum Mode
		{
			/// <summary>
			/// Object duoc cat tren duoi hoac trai phai de hien thi full theo size cua Sample
			/// </summary>
			FullSample = 0,
			/// <summary>
			/// Object khong bi cat va hien thi full ben trong Sample 
			/// </summary>
			FullObject = 1,
			/// <summary>
			/// Object duoc keo theo chieu rong
			/// </summary>
			FullWidth = 2,
			/// <summary>
			/// Object duoc keo theo chieu cao
			/// </summary>
			FullHeight = 3
		}

		private void Start()
		{
			if (playAwake)
			{
				Resize();
			}
		}

		[Button]
		public void Resize()
		{
			RectTrans.anchorMin = new Vector2(0.5f, 0.5f);
			RectTrans.anchorMax = new Vector2(0.5f, 0.5f);

			switch (mode)
			{
				case Mode.FullSample:
					DoFullSample();
					break;
				case Mode.FullObject:
					DoFullObject();
					break;
				case Mode.FullWidth:
					DoFullWidth();
					break;
				case Mode.FullHeight:
					DoFullHeight();
					break;
				default:
					break;
			}
		}

		void DoFullSample()
		{
			var isHigher = (sampleRatio.y / sampleRatio.x) > (sampleRect.rect.height / sampleRect.rect.width);

			if (isHigher)
			{
				DoFullWidth();
			}
			else
			{
				DoFullHeight();
			}
		}

		void DoFullObject()
		{
			var isWider = (sampleRatio.x / sampleRatio.y) > (SampleRect.rect.width / SampleRect.rect.height);

			if (isWider)
			{
				DoFullWidth();
			}
			else
			{
				DoFullHeight();
			}
		}

		void DoFullWidth()
		{
			var width = SampleRect.rect.width;

			var scale = width / sampleRatio.x;
			RectTrans.sizeDelta = new Vector2(width, sampleRatio.y * scale);
		}

		void DoFullHeight()
		{
			var height = SampleRect.rect.height;

			var scale = height / sampleRatio.y;
			RectTrans.sizeDelta = new Vector2(sampleRatio.x * scale, height);
		}
	}
}