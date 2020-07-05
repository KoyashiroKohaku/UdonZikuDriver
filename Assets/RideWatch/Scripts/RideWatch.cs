using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class RideWatch : UdonSharpBehaviour
{
    private readonly string _parameterName = "State";
    private Animator _animator;
    private VRC_Pickup _vrcPickup;
    private AudioSource _pickUpSound;
    private AudioSource _rotateSound;
    private AudioSource _launchSound;

    private bool _isSet = false;
    private int _state = 0;

    [SerializeField]
    private int _debugLevel = 0;

    [SerializeField]
    private Text _text;

    [SerializeField]
    private ZikuDriver _zikuDriver;

    #region Properties (with WriteLog)
    public int GetState()
    {
        return _state;
    }

    public void SetState(int value)
    {
        WriteLog(nameof(_state), _state + "=>" + value, 2);

        _state = value;
    }
    #endregion

    #region Events
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vrcPickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        var audioSources = transform.GetComponentsInChildren<AudioSource>();
        _pickUpSound = audioSources[0];
        _rotateSound = audioSources[1];
        _launchSound = audioSources[2];
    }

    private void Update()
    {
        if (!_isSet)
        {
            
        }
    }

    public override void OnPickup()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PickUp));
    }

    public override void OnPickupUseDown()
    {
        switch (GetState())
        {
            case 0:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Rotate));
                break;
            case 1:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Launch));
                break;
        }
    }
    #endregion

    #region Sounds
    public void PlayPickUpSound()
    {
        WriteLog("call", nameof(PlayPickUpSound), 2);
        WriteLog("audio", "ｶﾞﾁｬｯ", 1);

        _pickUpSound.Play();
    }

    public void PlayRotateSound()
    {
        WriteLog("call", nameof(PlayRotateSound), 2);
        WriteLog("audio", "（回転音）", 1);

        _rotateSound.Play();
    }

    public void PlayLaunchSound()
    {
        WriteLog("call", nameof(PlayRotateSound), 2);
        WriteLog("audio", "＼ｼﾞｯｵｰｳ／", 1);

        _launchSound.Play();
    }
    #endregion

    #region Network Events
    public void PickUp()
    {
        WriteLog("call", nameof(PickUp), 2);

        transform.parent = null;

        ResetState();
        PlayPickUpSound();
    }

    public void Rotate()
    {
        WriteLog("call", nameof(Rotate), 2);

        PlayRotateSound();
        _animator.Play(nameof(Rotate));
        SetState(1);
    }

    public void Launch()
    {
        WriteLog("call", nameof(Launch), 2);

        PlayLaunchSound();
        _animator.Play(nameof(Launch));
        SetState(2);
    }
    #endregion

    #region Utilities
    public void ResetState()
    {
        WriteLog("call", nameof(ResetState), 2);

        _isSet = false;
        SetState(0);
    }

    private void WriteLog(string type, string message, int debugLevel)
    {
        if (_debugLevel >= debugLevel)
        {
            var lines = _text.text.Split('\n');

            if (lines.Length > 17)
            {
                var firstLineCharCount = lines[0].Length + 1;
                _text.text = _text.text.Substring(firstLineCharCount) + type + ": " + message + "\n";
            }
            else
            {
                _text.text += type + ": " + message + "\n";
            }
        }
    }
    #endregion
}
