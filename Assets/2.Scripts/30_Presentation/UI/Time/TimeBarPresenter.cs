using System;
using UnityEngine;
using UnityEngine.UI;
using Hourbound.Domain.Time;
using Hourbound.Presentation.Time;
using TMPro;

namespace Hourbound.Presentaton.Time
{
    // 시간 자원(TimeResource)을 UI에 표시하는 프레젠터
    // TimeResourcePresenter를 인스펙터로 받아 이벤트를 구독함.
    public sealed class TimeBarPresenter : MonoBehaviour
    {
        [Header("의존성 주입")] 
        [SerializeField] private TimeResourcePresenter timePresenter;

        [Header("UI 참조")] 
        [SerializeField] private Slider timeSlider;
        [SerializeField] private TMP_Text timeText;

        [Header("표시 설정")] 
        [SerializeField] private bool showAsSeconds = true;
        [SerializeField] private int decimalPlaces = 0;
        [SerializeField] private bool showMaxInText = true;

        private ITimeResource _time;

        private void Awake()
        {
            if(timePresenter == null)
            {
                Debug.LogError("TimeBarPresenter : TimeResourcePresenter가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _time = timePresenter.Time;

            if (timeSlider != null)
            {
                timeSlider.minValue = 0f;
                timeSlider.maxValue = _time.Max;
                timeSlider.value = _time.Current;
            }

            UpdateText(_time.Current, _time.Max);
        }

        private void OnEnable()
        {
            if (_time == null) return;
            _time.Changed += HandleTimeChanged;
            _time.Depleted += HandleDepleted;
        }

        private void OnDisable()
        {
            if (_time == null) return;
            _time.Changed -= HandleTimeChanged;
            _time.Depleted -= HandleDepleted;
        }

        private void HandleTimeChanged(TimeChangedArgs args)
        {
            // 슬라이더 갱신
            if (timeSlider != null)
            {
                // Max가 바뀌는 경우도 있으니 매번 동기화
                timeSlider.maxValue = _time.Max;
                timeSlider.value = _time.Current;
            }
            
            // 텍스트 갱신
            UpdateText(_time.Current, _time.Max);
        }

        private void HandleDepleted(TimeDepletedArgs args)
        {
            // 시간이 0인 상태에서의 UI 처리
            // 예 : 텍스트를 빨갛게, 깜박임 등은 여기서 추가 가능
            UpdateText(_time.Current, _time.Max);
        }

        private void UpdateText(float current, float max)
        {
            if (timeText == null) return;

            string cur = FormatValue(current);
            if (!showMaxInText)
            {
                timeText.text = cur;
                return;
            }

            string mx = FormatValue(max);
            timeText.text = $"{cur} / {mx}";
        }

        private string FormatValue(float value)
        {
            // 지금은 숫자 표기만 담당(초 / 단위는 showAsSecond로 조절
            // showAsSecond가 false면 나중에 "시간 게이지" 같은 표기 규칙으로 바꿔도 됨
            string format = "F" + Mathf.Clamp(decimalPlaces, 0, 3);
            return value.ToString(format);
        }
    }
}