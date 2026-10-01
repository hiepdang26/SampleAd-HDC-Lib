using TMPro;
using UnityEngine;

namespace AppBootstrap.Intro
{
    public class DropdownPro : MonoBehaviour
    {
        public GameObject dropdown;
        public TMP_Text dropdownText;

        private string _currentValue;
        
        public void OpenDropdown()
        {
            dropdown.SetActive(!dropdown.activeInHierarchy);
        }

        public void ChangeValue(string newValue)
        {
            _currentValue = newValue;
            dropdownText.text = _currentValue;
            dropdown.SetActive(false);
        }

        public bool HasSelected()
        {
            return !string.IsNullOrEmpty(_currentValue);
        }
    }
}