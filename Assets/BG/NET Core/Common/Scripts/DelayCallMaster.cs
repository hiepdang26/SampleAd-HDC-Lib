using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.Common
{
    public static class DelayCallMaster
    {
        /// <param name="isUsingUnscaleTime">if true, ignore timescale</param>
        /// <returns></returns>
        public static IEnumerator WaitAndDoIE(float time, System.Action OnComplete, bool isUsingUnscaleTime = false)
        {
            yield return isUsingUnscaleTime ? (object)new WaitForSecondsRealtime(time) : new WaitForSeconds(time);
            OnComplete?.Invoke();
        }

        public static IEnumerator RepeatCall(float timeRate, System.Action<int> action, bool isUsingUnscaleTime = false)
        {
            var frameNumb = 0;
            while (true)
            {
                action?.Invoke(frameNumb);
                frameNumb++;

                yield return isUsingUnscaleTime ? (object)new WaitForSecondsRealtime(timeRate) : new WaitForSeconds(timeRate);
            }
        }

        public static IEnumerator RepeatCall(float timeRate, System.Action action, bool isUsingUnscaleTime = false)
        {
            while (true)
            {
                action?.Invoke();
                yield return isUsingUnscaleTime ? (object)new WaitForSecondsRealtime(timeRate) : new WaitForSeconds(timeRate);
            }
        }

        public static IEnumerator RepeatCall(float timeRate, int callTimes, System.Action<int> action, bool isUsingUnscaleTime = false)
        {
            var count = 0;
            while (count < callTimes)
            {
                action?.Invoke(count);
                count++;
                yield return isUsingUnscaleTime ? (object)new WaitForSecondsRealtime(timeRate) : new WaitForSeconds(timeRate);
            }
        }

        public static IEnumerator WaitForEndOfFrame(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        public static IEnumerator WaitForFrame(System.Action action, int frame = 1)
        {
            var t = frame;
            while (t > 0)
            {
                t--;
                yield return null;
            }
            action?.Invoke();
        }
    }
}