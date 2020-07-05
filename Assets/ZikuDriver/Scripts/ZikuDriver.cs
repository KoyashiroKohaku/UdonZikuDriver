using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class ZikuDriver : UdonSharpBehaviour
{
    private readonly string _parameterName = "State";
    private Animator _animator;
    private VRC_Pickup _vrcPickup;
    private AudioSource _pickUpSound;
    private AudioSource _lunchSound;
    private AudioSource _equipSound;

    private bool _isEquipped = false;
    private VRCPlayerApi _rider;
    private int _state = 0;
    private Vector3 _positionOffset = Vector3.zero;
    private Quaternion _rotationOffset = Quaternion.identity;

    [SerializeField]
    private int _debugLevel = 0;

    [SerializeField]
    private Text _text;

    #region Events
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vrcPickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        var audioSources = transform.GetComponentsInChildren<AudioSource>();
        _pickUpSound = audioSources[0];
        _lunchSound = audioSources[1];
        _equipSound = audioSources[2];
    }

    private void Update()
    {
        if (_isEquipped)
        {
            var chestPosition = _rider.GetBonePosition(HumanBodyBones.Chest);
            var chestRotation = _rider.GetBoneRotation(HumanBodyBones.Chest);

            transform.position = chestPosition + chestRotation * _positionOffset;
            transform.rotation = chestRotation * _rotationOffset;
        }
    }

    public override void OnPickup()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PickUp));
    }

    public override void OnPickupUseDown()
    {
        switch (_state)
        {
            case 0:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(LunchZikuDriver));
                break;
            case 1:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EquipZikuDriver));
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

    public void PlayLaunchSound()
    {
        WriteLog("call", nameof(PlayLaunchSound), 2);
        WriteLog("audio", "＼ｼﾞｸｳﾄﾞﾗｲﾊﾞｰ／", 1);

        _lunchSound.Play();
    }

    public void PlayEquipSound()
    {
        WriteLog("call", nameof(PlayEquipSound), 2);
        WriteLog("audio", "（装着音）", 1);

        _equipSound.Play();
    }
    #endregion

    #region Network Events
    public void PickUp()
    {
        WriteLog("call", nameof(PickUp), 2);

        ResetState();
        PlayPickUpSound();
    }

    public void LunchZikuDriver()
    {
        WriteLog("call", nameof(LunchZikuDriver), 2);

        PlayLaunchSound();

        SetState(1);
        _animator.Play(nameof(LunchZikuDriver));
    }

    public void EquipZikuDriver()
    {
        WriteLog("call", nameof(EquipZikuDriver), 2);

        SetState(2);
        _isEquipped = true;
        _rider = _vrcPickup.currentPlayer;

        var chestPosition = _vrcPickup.currentPlayer.GetBonePosition(HumanBodyBones.Chest);
        var chestRotation = _vrcPickup.currentPlayer.GetBoneRotation(HumanBodyBones.Chest);

        var zikuDriverPosition = transform.position;
        var zikuDriverRotation = transform.rotation;

        SetPositionOffset(zikuDriverPosition - chestPosition);
        SetRotationOffset(zikuDriverRotation * Quaternion.Inverse(chestRotation));

        _animator.Play(nameof(EquipZikuDriver));
        PlayEquipSound();
        _vrcPickup.Drop();
    }
    #endregion

    #region Setter (with WriteLog)
    private void SetState(int value)
    {
        WriteLog(nameof(_state), _state + "=>" + value, 2);

        _state = value;
    }

    private void SetPositionOffset(Vector3 offset)
    {
        WriteLog(nameof(offset.x), offset.x.ToString(), 2);
        WriteLog(nameof(offset.y), offset.y.ToString(), 2);
        WriteLog(nameof(offset.z), offset.z.ToString(), 2);

        _positionOffset = offset;
    }

    private void SetRotationOffset(Quaternion offset)
    {
        WriteLog(nameof(offset.x), offset.x.ToString(), 2);
        WriteLog(nameof(offset.y), offset.y.ToString(), 2);
        WriteLog(nameof(offset.z), offset.z.ToString(), 2);

        _rotationOffset = offset;
    }
    #endregion

    #region Utilities
    public void ResetState()
    {
        WriteLog("call", nameof(ResetState), 2);

        _isEquipped = false;
        _rider = null;
        SetState(0);
        SetPositionOffset(Vector3.zero);
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
