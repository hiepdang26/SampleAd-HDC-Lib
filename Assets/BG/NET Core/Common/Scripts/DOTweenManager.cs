using DG.Tweening;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.Common
{
	[CreateAssetMenu(fileName = AssetName, menuName = "BG_Library/DG/Create config")]
	public class DOTweenManager : ScriptableObject
	{
		public const string AssetName = "Dotween manager";

		static DOTweenManager ins;
		public static DOTweenManager Ins
		{
			get
			{
				if (ins == null)
				{
					Setup();
				}

				return ins;
			}
		}

		static void Setup()
		{
			ins = LoadSource.LoadObject<DOTweenManager>(AssetName);
			if (ins == null)
			{
				Debug.LogError("Create 'Dotween manager' in Resources. Scriptable Objects => DOTween");
			}

			_ = DOTween.Init(false, true, LogBehaviour.Verbose).SetCapacity(250, 1000);
		}

#if UNITY_EDITOR
		[MenuItem("BG/Dotween/Settings")]
		static void SelectSettingDotweenConfig()
		{
			Master.CreateAndSelectAssetInResource<DOTweenManager>(AssetName);
		}
#endif

		#region Image
		public Tweener TweenFillImageTime(Image image, float to, float duration
			, bool isUsingUnscaleTime = false)
		{
			return image.DOFillAmount(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenFillImageTime(Image image, float start, float to, float duration
			, bool isUsingUnscaleTime = false)
		{
			image.fillAmount = start;
			return TweenFillImageTime(image, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenFillImageVelocity(Image image, float to, float velocity
			, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs(to - image.fillAmount) / velocity;
			return TweenFillImageTime(image, to, duration, isUsingUnscaleTime);
		}
		public Tweener TweenFillImageVelocity(Image image, float start, float to, float velocity
			, bool isUsingUnscaleTime = false)
		{
			image.fillAmount = start;
			return TweenFillImageVelocity(image, to, velocity, isUsingUnscaleTime);
		}

		public Tweener TweenChangeAlphaImage(Image image, float to, float duration
			, bool isUsingUnscaleTime = false)
		{
			return image.DOFade(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenChangeAlphaImage(Image image, float start, float to, float duration
			, bool isUsingUnscaleTime = false)
		{
			var color = image.color;
			color.a = start;
			image.color = color;
			return TweenChangeAlphaImage(image, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenColorImage(Image image, Color to, float duration
			, bool isUsingUnscaleTime = false)
		{
			return image.DOColor(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenColorImage(Image image, Color start, Color to, float duration
			, bool isUsingUnscaleTime = false)
		{
			image.color = start;
			return TweenColorImage(image, to, duration, isUsingUnscaleTime);
		}
		#endregion

		#region Text
		public Tweener TweenerColorText(Text text, Color to, float duration, bool isUsingUnscaleTime = false)
		{
			return text.DOColor(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenerColorText(Text text, Color start, Color to, float duration, bool isUsingUnscaleTime = false)
		{
			text.color = start;
			return TweenerColorText(text, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenChangeAlphaText(Text text, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return text.DOFade(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenChangeAlphaText(Text text, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var color = text.color;
			color.a = start;
			text.color = color;
			return TweenChangeAlphaText(text, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenChangeAlphaText(TMP_Text text, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return text.DOFade(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenChangeAlphaText(TMP_Text text, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var color = text.color;
			color.a = start;
			text.color = color;
			return TweenChangeAlphaText(text, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenRunTextTime(Text text, string st, float duration, bool isUsingUnscaleTime = false)
		{
			return text.DOText(st, duration).SetEase(Ease.Linear).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenRunTextVelocity(Text text, string st, float secondPerLetter, bool isUsingUnscaleTime = false)
		{
			var duration = st.Length * secondPerLetter;
			return TweenRunTextTime(text, st, duration, isUsingUnscaleTime);
		}
		#endregion

		#region Canvas
		public Tweener TweenChangeAlphaCanvasGroup(CanvasGroup cvg, float to
			, float duration, bool isUsingUnscaleTime = false)
		{
			return cvg.DOFade(to, duration).SetUpdate(isUsingUnscaleTime);
		}

		public Tweener TweenChangeAlphaCanvasGroup(CanvasGroup cvg, float start
			, float to, float duration, bool isUsingUnscaleTime = false)
		{
			cvg.alpha = start;
			return cvg.DOFade(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		#endregion

		#region Sprite renderer
		public Tweener TweenChangeAlphaSpriteRend(SpriteRenderer spriteRend, float to
			, float duration, bool isUsingUnscaleTime = false)
		{
			return spriteRend.DOFade(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenChangeAlphaSpriteRend(SpriteRenderer spriteRend, float start
			, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var color = spriteRend.color;
			color.a = start;
			spriteRend.color = color;
			return TweenChangeAlphaSpriteRend(spriteRend, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenColorSpriteRend(SpriteRenderer spriteRend, Color start, Color to, float duration, bool isUsingUnscaleTime = false)
		{
			spriteRend.color = start;
			return spriteRend.DOColor(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		#endregion

		#region Transform Scale, Size
		public Tweener TweenScaleTime(Transform trans, Vector3 to, float duration, bool isUsingUnscaleTime = false)
		{
			return trans.DOScale(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenScaleTime(Transform trans, Vector3 start, Vector3 to, float duration, bool isUsingUnscaleTime = false)
		{
			trans.localScale = start;
			return TweenScaleTime(trans, to, duration, isUsingUnscaleTime);
		}
		public Tweener TweenScaleTime(Transform trans, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return trans.DOScale(Vector3.one * to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenScaleTime(Transform trans, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			trans.localScale = Vector3.one * start;
			return trans.DOScale(Vector3.one * to, duration).SetUpdate(isUsingUnscaleTime);
		}

		public Tweener TweenScaleY(Transform trans, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var scale = trans.localScale;
			scale.y = to;
			return TweenScaleTime(trans, scale, duration, isUsingUnscaleTime);
		}
		public Tweener TweenScaleY(Transform trans, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var scale = trans.localScale;
			scale.y = start;
			trans.localScale = scale;
			return TweenScaleY(trans, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenScale(Transform trans, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return trans.DOScale(Vector3.one * to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenScale(Transform trans, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			trans.localScale = Vector3.one * start;
			return trans.DOScale(Vector3.one * to, duration).SetUpdate(isUsingUnscaleTime);
		}

		public Tweener TweenScaleVelocity(Transform trans, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs(to - trans.localScale.x) / velocity;
			return trans.DOScale(Vector3.one * to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenScaleVelocity(Transform trans, float start, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var duration = (to - start) / velocity;
			trans.localScale = Vector3.one * start;
			return trans.DOScale(Vector3.one * to, duration).SetUpdate(isUsingUnscaleTime);
		}

		public Tweener TweenSizeDelta(RectTransform rectTrans, Vector2 to, float duration, bool isUsingUnscaleTime = false)
		{
			return rectTrans.DOSizeDelta(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenSizeDelta(RectTransform rectTrans, Vector2 start, Vector2 to, float duration, bool isUsingUnscaleTime = false)
		{
			rectTrans.sizeDelta = start;
			return TweenSizeDelta(rectTrans, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenSizeDeltaY(RectTransform rectTrans, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return TweenSizeDelta(rectTrans, new Vector2(rectTrans.sizeDelta.x, to), duration, isUsingUnscaleTime);
		}
		#endregion

		#region Move transform
		public Tweener TweenMoveTime(Transform trans, Vector3 to, float duration
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			var tween = isLocal ? trans.DOLocalMove(to, duration) : trans.DOMove(to, duration);
			return tween.SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenMoveTime(Transform trans, Vector3 start, Vector3 to, float duration
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			if (isLocal)
				trans.localPosition = start;
			else
				trans.position = start;

			return TweenMoveTime(trans, to, duration, isLocal, isUsingUnscaleTime);
		}

		public Tweener TweenMoveVelocity(Transform trans, Vector3 to, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			var duration = Vector3.Distance(isLocal ? trans.localPosition : trans.position, to) / velocity;
			return TweenMoveTime(trans, to, duration, isLocal, isUsingUnscaleTime);
		}
		public Tweener TweenMoveVelocity(Transform trans, Vector3 start, Vector3 to, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			if (isLocal)
				trans.transform.localPosition = start;
			else
				trans.transform.position = start;

			return TweenMoveVelocity(trans, to, velocity, isLocal, isUsingUnscaleTime);
		}

		public Tweener TweenMoveVelocityX(Transform trans, float to, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs((isLocal ? trans.localPosition.x : trans.position.x) - to) / velocity;
			var tween = isLocal ? trans.DOLocalMoveX(to, duration) : trans.DOMoveX(to, duration);
			return tween.SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenMoveVelocityX(Transform trans, float start, float to, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			Vector3 currentPos;
			if (isLocal)
			{
				currentPos = trans.transform.localPosition;
				trans.transform.localPosition = new Vector3(start, currentPos.y, currentPos.z);
			}
			else
			{
				currentPos = trans.transform.position;
				trans.transform.position = new Vector3(start, currentPos.y, currentPos.z);
			}

			return TweenMoveVelocityX(trans, to, velocity, isLocal, isUsingUnscaleTime);
		}

		public Tweener TweenMoveVelocityY(Transform trans, float to, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs((isLocal ? trans.localPosition.y : trans.position.y) - to) / velocity;
			var tween = isLocal ? trans.DOLocalMoveY(to, duration) : trans.DOMoveY(to, duration);
			return tween.SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenMoveVelocityY(Transform trans, float start, float to, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			Vector3 currentPos;
			if (isLocal)
			{
				currentPos = trans.transform.localPosition;
				trans.transform.localPosition = new Vector3(currentPos.x, start, currentPos.z);
			}
			else
			{
				currentPos = trans.transform.position;
				trans.transform.position = new Vector3(currentPos.x, start, currentPos.z);
			}

			return TweenMoveVelocityY(trans, to, velocity, isLocal, isUsingUnscaleTime);
		}

		public Sequence TweenJumpTime(Transform trans, Vector3 to, float jumpPower, int numJump, float duration
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			return isLocal ? trans.DOLocalJump(to, jumpPower, numJump, duration).SetUpdate(isUsingUnscaleTime)
				: trans.DOJump(to, jumpPower, numJump, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Sequence TweenJumpTime(Transform trans, Vector3 start, Vector3 to, float jumpPower, int numJump, float duration
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			trans.transform.position = start;
			return TweenJumpTime(trans, to, jumpPower, numJump, duration, isLocal, isUsingUnscaleTime);
		}

		public Sequence TweenJumpVelocity(Transform trans, Vector3 to, float jumpPower, int numJump, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			var duration = Vector3.Distance(trans.position, to) / velocity;
			return TweenJumpTime(trans, to, jumpPower, numJump, duration, isLocal, isUsingUnscaleTime);
		}
		public Sequence TweenJumpVelocity(Transform trans, Vector3 start, Vector3 to, float jumpPower, int numJump, float velocity
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			trans.position = start;
			return TweenJumpVelocity(trans, to, jumpPower, numJump, velocity, isLocal, isUsingUnscaleTime);
		}

		public Tweener TweenRotate(Transform trans, Vector3 to, float duration, RotateMode mode
			, bool isLocal = true, bool isUsingUnscaleTime = false)
		{
			if (isLocal)
			{
				return trans.DOLocalRotate(to, duration, mode).SetUpdate(isUsingUnscaleTime);
			}
			else
			{
				return trans.DORotate(to, duration, mode).SetUpdate(isUsingUnscaleTime);
			}
		}
		public Tweener TweenRotate(Transform trans, Vector3 start, Vector3 to, float duration, RotateMode mode
			, bool isLocal, bool isUsingUnscaleTime = false)
		{
			if (isLocal)
			{
				trans.localEulerAngles = start;
			}
			else
			{
				trans.eulerAngles = start;
			}
			return TweenRotate(trans, to, duration, mode, isLocal, isUsingUnscaleTime);
		}

		public Tweener TweenShake(Transform trans, float duration, float strength = 1, int vibrato = 10, float randomness = 90
			, bool snapping = false, bool fadeOut = true, bool isUsingUnscaleTime = false)
		{
			return trans.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeOut).SetUpdate(isUsingUnscaleTime);
		}
		#endregion

		#region Move anchor
		public Tweener TweenMoveAnchorTime(RectTransform rectTrans, Vector2 to, float duration, bool isUsingUnscaleTime = false)
		{
			return rectTrans.DOAnchorPos(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenMoveAnchorTime(RectTransform rectTrans, Vector2 start, Vector2 to, float duration, bool isUsingUnscaleTime = false)
		{
			rectTrans.anchoredPosition = start;
			return TweenMoveAnchorTime(rectTrans, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenMoveAnchorVelocity(RectTransform rectTrans, Vector2 to, float velocity, bool isUsingUnscaleTime = false)
		{
			var duration = Vector2.Distance(rectTrans.anchoredPosition, to) / velocity;
			return TweenMoveAnchorTime(rectTrans, to, duration, isUsingUnscaleTime);
		}
		public Tweener TweenMoveAnchorVelocity(RectTransform rectTrans, Vector2 start, Vector2 to, float velocity, bool isUsingUnscaleTime = false)
		{
			rectTrans.anchoredPosition = start;
			return TweenMoveAnchorVelocity(rectTrans, to, velocity, isUsingUnscaleTime);
		}

		public Tweener TweenMoveAnchorTimeX(RectTransform rectTrans, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return rectTrans.DOAnchorPosX(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenMoveAnchorTimeX(RectTransform rectTrans, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var pos = rectTrans.anchoredPosition;
			pos.x = start;
			rectTrans.anchoredPosition = pos;
			return TweenMoveAnchorTimeX(rectTrans, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenMoveAnchorVelocityX(RectTransform rectTrans, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs(rectTrans.anchoredPosition.x - to) / velocity;
			return TweenMoveAnchorTimeX(rectTrans, to, duration, isUsingUnscaleTime);
		}
		public Tweener TweenMoveAnchorVelocityX(RectTransform rectTrans, float start, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var pos = rectTrans.anchoredPosition;
			pos.x = start;
			rectTrans.anchoredPosition = pos;
			return TweenMoveAnchorVelocityX(rectTrans, to, velocity, isUsingUnscaleTime);
		}

		public Tweener TweenMoveAnchorTimeY(RectTransform rectTrans, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return rectTrans.DOAnchorPosY(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		public Tweener TweenMoveAnchorTimeY(RectTransform rectTrans, float start, float to, float duration, bool isUsingUnscaleTime = false)
		{
			var pos = rectTrans.anchoredPosition;
			pos.y = start;
			rectTrans.anchoredPosition = pos;
			return TweenMoveAnchorTimeY(rectTrans, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenMoveAnchorVelocityY(RectTransform rectTrans, float start, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var pos = rectTrans.anchoredPosition;
			pos.y = start;
			rectTrans.anchoredPosition = pos;
			return TweenMoveAnchorVelocityY(rectTrans, to, velocity, isUsingUnscaleTime);
		}

		public Tweener TweenMoveAnchorVelocityY(RectTransform rectTrans, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs(rectTrans.anchoredPosition.y - to) / velocity;
			return TweenMoveAnchorTimeY(rectTrans, to, duration, isUsingUnscaleTime);
		}
		#endregion

		#region Camera
		public Tweener TweenOrthoCameraSizeVelocity(Camera cam, float to, float velocity, bool isUsingUnscaleTime = false)
		{
			var duration = Mathf.Abs(cam.orthographicSize - to) / velocity;
			return TweenOrthoCameraSize(cam, to, duration, isUsingUnscaleTime);
		}

		public Tweener TweenOrthoCameraSize(Camera cam, float to, float duration, bool isUsingUnscaleTime = false)
		{
			return cam.DOOrthoSize(to, duration).SetUpdate(isUsingUnscaleTime);
		}
		#endregion

		#region Time
		public Tween TweenDelay(float delayTime, TweenCallback callback, bool isUsingUnscaleTime = false)
		{
			return DOVirtual.DelayedCall(delayTime, callback, isUsingUnscaleTime);
		}

		/*public Tweener TweenIntTime(int from, int to, float duration, System.Action<int> onUpdate)
		{
			return DOVirtual.Int(from, to, duration, f => { onUpdate?.Invoke(f); });
		}
		public Tweener TweenIntVelocity(int from, int to, float velocity, System.Action<int> onUpdate)
		{
			var duration = Mathf.Abs(from - to) / velocity;
			return DOVirtual.Int(from, to, duration, f => { onUpdate?.Invoke(f); });
		}*/

		public Tweener TweenFloatTime(float from, float to, float duration, System.Action<float> onUpdate)
		{
			return DOVirtual.Float(from, to, duration, f => { onUpdate?.Invoke(f); });
		}
		public Tweener TweenFloatVelocity(float from, float to, float velocity, System.Action<float> onUpdate)
		{
			var duration = Mathf.Abs(from - to) / velocity;
			return DOVirtual.Float(from, to, duration, f => { onUpdate?.Invoke(f); });
		}

		public float TweenEasedValue(float from, float to, float lifetimePercentage, Ease ease)
		{
			return DOVirtual.EasedValue(from, to, lifetimePercentage, ease);
		}

		public float TweenEasedValue(float from, float to, float lifetimePercentage, AnimationCurve easeCurve)
		{
			return DOVirtual.EasedValue(from, to, lifetimePercentage, easeCurve);
		}
		#endregion

		public void KillTween(Tween tween)
		{
			if (tween == null || !tween.IsActive())
				return;

			_ = tween.Pause();
			_ = DOTween.Kill(tween);
		}

		public void KillAllTween()
		{
			_ = DOTween.KillAll();
		}

		#region extra methob
		public Tweener ScaleToShow(Transform obj)
		{
			return TweenScaleTime(obj, Vector3.zero, Vector3.one
				, 0.4f).SetEase(Ease.OutBack);
		}

		public Tweener ScaleToHide(Transform obj)
		{
			return TweenScaleTime(obj, Vector3.one, Vector3.zero
				, 0.4f).SetEase(Ease.InBack);
		}

		public Sequence ShakeFail(Transform trans)
		{
			return DOTween.Sequence()
				.Append(TweenRotate(trans, new Vector3(0, 0, 15), 0.1f, RotateMode.Fast))
				.Append(TweenRotate(trans, new Vector3(0, 0, -15), 0.2f, RotateMode.Fast))
				.Append(TweenRotate(trans, Vector3.zero, 0.1f, RotateMode.Fast));
		}

		public void ScaleConfirm(Transform trans, TweenCallback onComplete = null)
		{
			TweenScaleTime(trans, Vector3.one * 1.35f, 0.2f).OnComplete(() =>
			{
				TweenScaleTime(trans, Vector3.one, 0.1f).OnComplete(onComplete);
			});
		}

		public Sequence JumpTo(Transform trans, Vector3 to)
		{
			return TweenJumpTime(trans, to, 3, 1, 0.5f, false);
		}

		/// <summary>
		/// Animation cua vat khi bi roi xuong mat phang
		/// </summary>
		public void FallToGroundAnim(Transform trans)
		{
			trans.DOKill();
			DOTween.Sequence()
				.Append(TweenScaleTime(
					trans, new Vector3(1.1f, 0.9f, 1), 0.1f))
				.Append(TweenScaleTime(
					trans, new Vector3(0.9f, 1.1f, 1), 0.1f))
				.Append(TweenScaleTime(
					trans, Vector3.one, 0.05f));
		}
		#endregion
	}
}
