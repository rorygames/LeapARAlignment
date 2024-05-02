using Leap.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeapOffsetChanger : MonoBehaviour
{
    [SerializeField]
    LeapXRServiceProvider _leapXRServiceProvider = null;

    [SerializeField]
    private InputActionAsset _inputActions;

    private int _currentMode = 0;

    private float _incrementValue = 0.001f;

    private bool _xLocked = false, _yLocked = false;
    private float _xLockTime = 0f, _yLockTime = 0f;

    [SerializeField]
    private TextMeshProUGUI _yOffset, _zOffset, _xTilt, _incrementAmount;
    [SerializeField]
    private GameObject _ui;
    private string _yOffsetString, _zOffsetString, _xTiltString, _incrementAmountString;

    private const string _yKey = "YOffset", _zKey = "ZOffset", _xKey = "XTilt";

    private void Awake()
    {
        _inputActions.Enable();
        _yOffsetString = _yOffset.text;
        _zOffsetString = _zOffset.text;
        _xTiltString = _xTilt.text;
        _incrementAmountString = _incrementAmount.text;
        UpdateText();

    }

    private void Start()
    {
        if(PlayerPrefs.HasKey(_yKey))
        {
            _leapXRServiceProvider.deviceOffsetYAxis = PlayerPrefs.GetFloat(_yKey);
        }
        if (PlayerPrefs.HasKey(_zKey))
        {
            _leapXRServiceProvider.deviceOffsetZAxis = PlayerPrefs.GetFloat(_zKey);
        }
        if (PlayerPrefs.HasKey(_xKey))
        {
            _leapXRServiceProvider.deviceTiltXAxis = PlayerPrefs.GetFloat(_xKey);
        }
        UpdateText();
    }

    private void Update()
    {
        if (_inputActions.FindAction("Mode").WasPressedThisFrame())
        {
            ChangeMode();
        }
        ChangeValue();
    }

    public void ChangeMode()
    {
        _currentMode++;
        if (_currentMode > 3)
        {
            _currentMode = 0;
        }
        UpdateText();
    }

    public void ChangeValue()
    {
        if (_currentMode == 3)
            return;

        Vector2 currentStickVal = _inputActions.FindAction("Stick").ReadValue<Vector2>();

        if(_xLockTime > 0)
        {
            _xLockTime -= Time.deltaTime;
        }
        if(_yLockTime > 0)
        {
            _yLockTime -= Time.deltaTime;
        }

        if (currentStickVal.x < 0.5f && currentStickVal.x > -0.5f || _xLockTime <= 0)
        {
            _xLocked = false;
        }

        if (currentStickVal.y < 0.5f && currentStickVal.y > -0.5f || _yLockTime <= 0)
        {
            _yLocked = false;
        }

        if (!_xLocked)
        {
            if (currentStickVal.x > 0.5f)
            {
                _incrementValue += 0.001f;
                _incrementValue = Mathf.Round(_incrementValue * 1000f) / 1000f;
                _xLocked = true;
                _xLockTime = 0.1f;
                UpdateText();
            }
            if (currentStickVal.x < -0.5f)
            {
                _incrementValue -= 0.001f;
                if(_incrementValue < 0)
                {
                    _incrementValue = 0f;
                }
                _incrementValue = Mathf.Round(_incrementValue * 1000f) / 1000f;
                _xLocked = true;
                _xLockTime = 0.1f;
                UpdateText();
            }
        }

        if (!_yLocked)
        {
            if (currentStickVal.y > 0.5f)
            {
                UpdateIncrement(true);
                _yLocked = true;
                _yLockTime = 0.1f;
            }
            if (currentStickVal.y < -0.5f)
            {
                UpdateIncrement(false);
                _yLocked = true;
                _yLockTime = 0.1f;
            }
        }
    }

    private void UpdateIncrement(bool up)
    {
        float val = _incrementValue * (up ? 1 : -1);
        switch (_currentMode)
        {
            case 0:
                _leapXRServiceProvider.deviceOffsetYAxis = Mathf.Round((_leapXRServiceProvider.deviceOffsetYAxis + val) * 1000f) / 1000f;
                PlayerPrefs.SetFloat(_yKey, _leapXRServiceProvider.deviceOffsetYAxis);
                break;
            case 1:
                _leapXRServiceProvider.deviceOffsetZAxis = Mathf.Round((_leapXRServiceProvider.deviceOffsetZAxis + val) * 1000f) / 1000f;
                PlayerPrefs.SetFloat(_zKey, _leapXRServiceProvider.deviceOffsetZAxis);
                break;
            case 2:
                _leapXRServiceProvider.deviceTiltXAxis = Mathf.Round((_leapXRServiceProvider.deviceTiltXAxis + val) * 1000f) / 1000f;
                PlayerPrefs.SetFloat(_xKey, _leapXRServiceProvider.deviceTiltXAxis);
                break;
        }
        PlayerPrefs.Save();
        UpdateText();
    }

    private void UpdateText()
    {
        _yOffset.text = $"{_yOffsetString} {_leapXRServiceProvider.deviceOffsetYAxis}";
        _zOffset.text = $"{_zOffsetString} {_leapXRServiceProvider.deviceOffsetZAxis}";
        _xTilt.text = $"{_xTiltString} {_leapXRServiceProvider.deviceTiltXAxis}";
        _incrementAmount.text = $"{_incrementAmountString} {_incrementValue}";

        _yOffset.color = _currentMode == 0 ? Color.red : Color.white;
        _zOffset.color = _currentMode == 1 ? Color.red : Color.white;
        _xTilt.color = _currentMode == 2 ? Color.red : Color.white;

        _ui.SetActive(_currentMode != 3);
    }
}
