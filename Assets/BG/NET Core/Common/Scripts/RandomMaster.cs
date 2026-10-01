using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.Common
{
    public static class RandomMaster
    {
        public static T RandomInList<T>(List<T> listRandom)
        {
            if (listRandom.Count == 0) return default;

            var indexRandom = Random.Range(0, listRandom.Count);
            return listRandom[indexRandom];
        }

        public static T RandomInList<T>(T[] listRandom)
        {
            var indexRandom = Random.Range(0, listRandom.Length);
            return listRandom[indexRandom];
        }

        public static bool RandomRate(float rate)
        {
            return Random.Range(0f, 100f) < rate;
        }

        public static int RandomRate(List<float> listRate)
        {
            var randomNumb = Random.Range(0f, 100f);
            var milestones = new List<float> { 0 };

            for (int i = 0; i < listRate.Count; i++)
            {
                milestones.Add(milestones[i] + listRate[i]);

                if (randomNumb >= milestones[i] && randomNumb < milestones[i + 1])
                    return i;
            }

            return listRate.Count;
        }
        public static int RandomRate(float[] listRate)
        {
            var randomNumb = Random.Range(0f, 100f);
            var milestones = new List<float> { 0 };

            for (int i = 0; i < listRate.Length; i++)
            {
                milestones.Add(milestones[i] + listRate[i]);

                if (randomNumb >= milestones[i] && randomNumb < milestones[i + 1])
                    return i;
            }

            return listRate.Length;
        }

        public static int RandomRangeExcept(int start, int end, List<int> excepts)
        {
            var r = Random.Range(start, end);
            if (excepts.Contains(r)) return RandomRangeExcept(start, end, excepts);
            else return r;
        }
    }

    public class RandomNoRepeat<T>
    {
        /// <summary>
        /// Danh sach goc
        /// </summary>
        protected readonly List<T> listRandom;
        /// <summary>
        /// Danh sach cac item duoc Random
        /// </summary>
        public List<T> ListTemp { get; set; }

        public RandomNoRepeat(IEnumerable<T> listR)
        {
            listRandom = new List<T>(listR);
            ListTemp = new List<T>(listRandom);
        }

        public virtual T Random()
        {
            if (ListTemp.Count == 0)
            {
                ListTemp = new List<T>(listRandom);
            }

			var tempObj = RandomMaster.RandomInList(ListTemp);
			_ = ListTemp.Remove(tempObj);
			return tempObj;
		}
    }
}