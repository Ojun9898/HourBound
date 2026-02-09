using System;
using UnityEngine;
using Hourbound.Application.Rewards;
using Hourbound.Presentation.Time;

namespace Hourbound.Presentation.Debugging
{
    /// <summary>
    /// 디버그용 HUD : 처치(킬) 보상/콤보/가중치 결과를 화면 좌상단에 표시한다.
    /// - 데이터 소스 : EnemyKillRewardController의 Debug_* 프로퍼티
    /// - Unity 6.3 LTS에서 기본 제공되는 OnGUI기반 (UI에셋 불필요)
    /// </summary>
    public sealed class DebugKillComboHud : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private EnemyKillRewardController rewardController;
        
        [Header("View")]
        [SerializeField] private bool visible = true;
        [Min(10f)] [SerializeField] private float fontSize = 16f;
        [Min(0f)] [SerializeField] private float fadeAfterSeconds = 5f;
        
        private GUIStyle _style;

        private void Awake()
        {
            _style = new GUIStyle
            {
                fontSize = Mathf.RoundToInt(fontSize),
                richText = true
            };
        }

        private void OnGUI()
        {
            if (!visible) return;
            if (rewardController == null) return;

            if (fadeAfterSeconds > 0f)
            {
                float t = UnityEngine.Time.unscaledTime - rewardController.Debug_LastKillUnscaledTime;
                // 0 ~ fadeAfterSecond까지 1 -> 0.25로 천천히 흐려지게
                float a = Mathf.Lerp(1f, 0.25f, Mathf.Clamp01(t / fadeAfterSeconds));
                GUI.color = new Color(1f, 1f, 1f, a);
            }

            else
            {
                GUI.color = Color.white;
            }
            
            _style.fontSize = Mathf.RoundToInt(fontSize);
            
            string text = 
                $"<b>[KILL DEBUG HUD]</b>\n" + 
                $"Streak : <b>{rewardController.Debug_Streak}</b> " +
                $"Mul : Combo <b>x{rewardController.Debug_ComboMultiplier:0.00}</b> " +
                $"Enemy <b>x{rewardController.Debug_EnemyMultiplier:0:00}</b> " +
                $"Final <b>x{rewardController.Debug_FinalMultiplier:0.00}</b>\n" +
                $"Gain : <b>+{rewardController.Debug_LastGainSeconds:0.00}</b>\n" +
                $"Source : {rewardController.Debug_LastSourceId}\n" + 
                $"LastKill : {rewardController.Debug_LastKillUnscaledTime:0.00}";
            
            GUI.Label(new Rect(16, 16, 520, 220), text, _style);
            
            GUI.color = Color.white;
        }
    }
}