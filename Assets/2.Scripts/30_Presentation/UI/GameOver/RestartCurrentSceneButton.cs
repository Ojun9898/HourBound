using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hourbound.Presentation.UI.GameOver
{
    /// <summary>
    /// 현재 씬을 재시작하는 역할
    /// </summary>
    public sealed class RestartCurrentSceneButton : MonoBehaviour
    {
        public void Restart()
        {
            // timeScale 복구 (GameOver에서 0으로 만든 것)
            UnityEngine.Time.timeScale = 1f;
            UnityEngine.Time.fixedDeltaTime = 0.02f;
            
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}