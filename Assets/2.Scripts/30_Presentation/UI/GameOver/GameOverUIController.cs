using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hourbound.Presentation.UI.GameOver
{
    /// <summary>
    /// 게임오버 UI 표시/숨김 및 UI 포커스 관리
    /// timeScale=0이어도 UI는 입력 받도록 구성
    /// </summary>
    public sealed class GameOverUIController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject firstSelected;
        
        [Header("Cursor")]
        [SerializeField] private bool showCursorOnOpen = true;
        
        public bool IsOpen => root != null && root.activeSelf;

        private void Reset()
        {
            // 컨트롤러를 Panel에 붙였다면 root 자동 지정
            if (root == null) root = gameObject;
        }


        public void Show()
        {
            if (root == null) return;
            root.SetActive(true);
            
            if (showCursorOnOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (EventSystem.current != null && firstSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelected);
            }
        }

        public void Hide()
        {
            if (root == null) return;
            root.SetActive(false);
        }
    }
}