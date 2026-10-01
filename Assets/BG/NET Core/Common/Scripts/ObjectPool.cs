using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.Common
{
    [System.Serializable]
    public class ObjectPool
    {
        public System.Action<GameObject> OnObjRemoved;

        [SerializeField] List<GameObject> listCurrent;
        public List<GameObject> ListCurrent => listCurrent;

        readonly List<GameObject> listAvailable;
        public List<GameObject> ListAvailable => listAvailable;

        readonly GameObject prefab;
        readonly Transform spawnerTrans;

        public ObjectPool(GameObject prefab, Transform spawnerTrans)
        {
            listCurrent = new List<GameObject>();
            listAvailable = new List<GameObject>();

            this.prefab = prefab;
            this.spawnerTrans = spawnerTrans;
        }

        public GameObject GetObjInPool()
        {
            if (listAvailable.Count > 0)
            {
                var obj = listAvailable[0];

                listAvailable.Remove(obj);
                listCurrent.Add(obj);

                return obj;
            }
            else
            {
                InitantiateObj();
                return GetObjInPool();
            }
        }

        /// <summary>
        /// Spawn obj, and add to avaiable list
        /// </summary>
        public void InitantiateObj()
        {
            var obj = Object.Instantiate(prefab, spawnerTrans);
            listAvailable.Add(obj);
        }

        /// <summary>
        /// Remove obj to available list
        /// </summary>
        public void RemoveObj(GameObject obj)
        {
            if (!listCurrent.Contains(obj)) return;

            listCurrent.Remove(obj);
            listAvailable.Add(obj);
            obj.transform.SetParent(spawnerTrans);

            OnObjRemoved?.Invoke(obj);
        }

        /// <summary>
        /// Remove all obj to available list
        /// </summary>
        public void RemoveAllCurrentObj()
        {
            while (listCurrent.Count > 0)
            {
                RemoveObj(listCurrent[0]);
            }
        }

        public void DestroyListAvailable()
        {
            while (listAvailable.Count > 0)
            {
                Object.Destroy(listAvailable[0]);
                listAvailable.RemoveAt(0);
            }
        }
    }
}