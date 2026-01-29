using System;
using UnityEngine;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;

namespace Hourbound.Presentaton.Time
{
    public sealed class GameOverOnTimeDepleted : MonoBehaviour
    {
        [Header("의존성 주입")]
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("패배 처리 옵션")] 
        [SerializeField] private bool pauseTimeScale = true; // 패배 시 Time.timeScale을 0으로 만들지 여부
        [SerializeField] private bool disablePlayerInput = false; // 플레이어 입력 차단(프로젝트 구조에 맞춰 추후 연결)
        
        private ITimeResource _time;
        private bool _alreadyHandled;
        
        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("GameOverOnTimeDepleted : TimeResourcePresenter가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            _time = timePresenter.Time;
        }

        private void OnEnable()
        {
            if (_time == null) return;
            _time.Depleted += HandleDepleted;
        }

        private void OnDisable()
        {
            if (_time == null) return;
            _time.Depleted -= HandleDepleted;
        }

        private void HandleDepleted(TimeDepletedArgs args)
        {
            // 중복 실행 방지
            if (_alreadyHandled) return;
            _alreadyHandled = true;
            
            // 1. 일단 로그로 확인
            Debug.Log("시간이 0이 되어 패배 하였습니다.", this);
            
            // 2. 타임 스케일 정지
            if (pauseTimeScale)
            {
                UnityEngine.Time.timeScale = 0f;
            }
            // 3. 입력 차단 (지금은 옵션만 두고 , 실제 차단은 프로젝트 입력 구조에 맞게 연결)
            if (disablePlayerInput)
            {
                // 예시 :
                // playerInput.enabled = false;
                // 또는 입력 서비스에 Disable 요청
                Debug.Log("플레이어의 입력이 차단되었습니다.");
            }
            
            // 4. 여기에 게임 오버 UI를 띄우거나, 씬 전환, 리스타트 메뉴 호출 등을 수행하면 됨
            // 예 : 
            // gameOverPanel.SetActive(true);
            // 또는 RunStateMachine에 "GameOver" 이벤트 발송
        }
        
        // 에디터에서 재생 종료 후 Time.timeScale이 0으로 남는걸 방지하기 위한 안전장치
        private void OnApplicationQuit()
        {
            if (pauseTimeScale)
                UnityEngine.Time.timeScale = 1f;
        }
    }
}
