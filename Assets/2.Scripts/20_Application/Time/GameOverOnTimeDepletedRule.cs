using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;
using Hourbound.Presentation.UI.GameOver;

namespace Hourbound.Presentation.Rule.Time
{
    /// <summary>
    /// 시간 자원이 0이 되면 패배(GameOver) 처리
    ///  - TimeResourcePresenter가 도메인 TimeResource 생성
    ///  - ITimeResource.Deleted 구독
    ///  - timeScale=0이어도 UI입력은 InputSystemUIInputModule이 처리 가능
    ///  - PlayerInput 액션맵을 UI로 전환하여 게임플레이 입력 차단
    /// </summary>
    [DefaultExecutionOrder(-80)] // TimeResourcePresenter, 다른 Rule 이후
    public sealed class GameOverOnTimeDepletedRule : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TimeResourcePresenter timePresenter;
        [SerializeField] private GameOverUIController gameOverUI;
        
        [Tooltip("PlayerInput을 쓰는 경우 연결 권장(게임오버 시 UI 맵으로 전환 가능")]
        [SerializeField] private PlayerInput playerInput;
        
        [Header("패배 처리 옵션")] 
        [SerializeField] private bool pauseTimeScale = true; // 패배 시 Time.timeScale을 0으로 만들지 여부
        
        [Header("레거시 옵션(향후 Gate/서비스 연결용). 지금은 로그만 남김")]
        [SerializeField] private bool disablePlayerInput = false; // 플레이어 입력 차단(프로젝트 구조에 맞춰 추후 연결)
        
        [Header("Action Map 전환")]
        [SerializeField] private bool switchToUIActionMap = true;
        [SerializeField] private string uiActionMapName = "UI";
        
        [Header("Debug")]
        [SerializeField] private bool log = true;
        
        
        private ITimeResource _time;
        private bool _alreadyHandled;

        private void Reset()
        {
            if (timePresenter == null)
                timePresenter = FindAnyObjectByType<TimeResourcePresenter>();
            
            if (gameOverUI == null) 
                gameOverUI = FindFirstObjectByType<GameOverUIController>();
            
            if (playerInput == null)
                playerInput = FindFirstObjectByType<PlayerInput>();
        }

        private void Awake()
        {
            if (timePresenter == null)
            {
                Debug.LogError("GameOverOnTimeDepleted : TimeResourcePresenter가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            EnsureTime();
            if (_time == null)
            {
                Debug.LogError("GameOverOnTimeDepletedRule : timePresenter.Time이 null입니다.", this);
            }
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (_time == null) return;
            _time.Depleted -= HandleDepleted;
            
            // 에디터에서 Play Stop 시 timeScale이 0으로 남는 문제 방지
            if (pauseTimeScale && UnityEngine.Time.timeScale == 0f)
                UnityEngine.Time.timeScale = 1f;
        }

        private void TrySubscribe()
        {
            EnsureTime();
            if (_time == null) return;
            
            // 중복 구독 방지
            _time.Depleted -= HandleDepleted;
            _time.Depleted += HandleDepleted;
        }

        private void EnsureTime()
        {
            if (_time == null && timePresenter != null)
                _time = timePresenter.Time;
        }
        
        private void HandleDepleted(TimeDepletedArgs args)
        {
            // 중복 실행 방지
            if (_alreadyHandled) return;
            _alreadyHandled = true;
            
            // 1. 일단 로그로 확인
            if (log)
                Debug.Log($"[GAME OVER] 시간이 0이 되어 패배 하였습니다. context={args.Context}", this);
            
            // 여기에 게임 오버 UI를 띄우거나, 씬 전환, 리스타트 메뉴 호출 등을 수행하면 됨
            if (gameOverUI != null) 
                gameOverUI.Show();
            
            // 2. PlayerInput 액션맵을 UI로 전환해서 게임플레이 입력 차단
            if (switchToUIActionMap && playerInput != null && !string.IsNullOrWhiteSpace(uiActionMapName))
            {
                if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != uiActionMapName)
                    playerInput.SwitchCurrentActionMap(uiActionMapName);
            }
            
            // 3. 타임 스케일 정지
            if (pauseTimeScale)
            {
                UnityEngine.Time.timeScale = 0f;
            }
            
            // 4. 입력 차단 (지금은 옵션만 두고 , 실제 차단은 프로젝트 입력 구조에 맞게 연결)
            if (disablePlayerInput)
            {
                // 예시 :
                // playerInput.enabled = false;
                // 또는 입력 서비스에 Disable 요청
                Debug.Log("플레이어의 입력이 차단되었습니다.");
            }
        }
    }
}
