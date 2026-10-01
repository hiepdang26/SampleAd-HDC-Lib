using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.DEBUG
{
	public class SelectPanels : MonoBehaviour
	{
		public Transform BtnsParent;
		public Transform PanelsParent;

		void Start()
		{
			Select(0);
		}

		public void Select(int order)
		{
			for (var i = 0; i < BtnsParent.childCount; i++)
			{
				if (i == order) BtnsParent.GetChild(i).localScale = Vector3.one;
				else BtnsParent.GetChild(i).localScale = new Vector3(0.8f, 0.8f, 0.8f);
			}
			for (int i = 0; i < PanelsParent.childCount; i++)
			{
				PanelsParent.GetChild(i).gameObject.SetActive(i == order);
			}
		}
	}
}