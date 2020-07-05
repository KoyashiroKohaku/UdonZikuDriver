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
    private AudioSource _attachSound;
    private AudioSource _standbySound;

    private bool _isEquipped = false;
    private bool _isStandby = false;
    private VRCPlayerApi _rider;
    private int _state = 0;
    private Vector3 _positionOffset = Vector3.zero;
    private Quaternion _rotationOffset = Quaternion.identity;

    [SerializeField]
    private int _debugLevel = 0;

    [SerializeField]
    private Text _text;

    [SerializeField]
    private Transform _lSlot;

    [SerializeField]
    private Transform _rSlot;

    [SerializeField]
    private Transform[] _rideWatches;

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

    public Vector3 GetPositionOffset()
    {
        return _positionOffset;
    }

    public void SetPositionOffset(Vector3 offset)
    {
        WriteLog(nameof(offset.x), offset.x.ToString(), 2);
        WriteLog(nameof(offset.y), offset.y.ToString(), 2);
        WriteLog(nameof(offset.z), offset.z.ToString(), 2);

        _positionOffset = offset;
    }

    public Quaternion GetRotationOffset()
    {
        return _rotationOffset;
    }

    public void SetRotationOffset(Quaternion offset)
    {
        WriteLog(nameof(offset.x), offset.x.ToString(), 2);
        WriteLog(nameof(offset.y), offset.y.ToString(), 2);
        WriteLog(nameof(offset.z), offset.z.ToString(), 2);

        _rotationOffset = offset;
    }
    #endregion

    #region Events
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vrcPickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        var audioSources = transform.GetComponentsInChildren<AudioSource>();
        _pickUpSound = audioSources[0];
        _lunchSound = audioSources[1];
        _equipSound = audioSources[2];
        _attachSound = audioSources[3];
        _standbySound = audioSources[4];
    }

    private void Update()
    {
        if (_isEquipped)
        {
            var chestPosition = _rider.GetBonePosition(HumanBodyBones.Chest);
            var chestRotation = _rider.GetBoneRotation(HumanBodyBones.Chest);

            transform.position = chestPosition + chestRotation * GetPositionOffset();
            transform.rotation = chestRotation * GetRotationOffset();
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
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Lunch));
                break;
            case 1:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Equip));
                break;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        WriteLog("call", nameof(OnTriggerEnter), 2);
        WriteLog("collider", collider.gameObject.name, 2);

        switch (collider.transform.gameObject.name)
        {
            case "ZioRideWatch":
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(AttachZioRideWatch));
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

    public void PlayAttachSound()
    {
        WriteLog("call", nameof(PlayAttachSound), 2);
        WriteLog("audio", "（ライドウォッチセット音）", 1);

        _attachSound.Play();
    }

    public void PlayStandbySound()
    {
        WriteLog("call", nameof(PlayStandbySound), 2);
        WriteLog("audio", "（変身待機音）", 1);

        _isStandby = true;

        _standbySound.Play();
    }
    #endregion

    #region Network Events
    public void PickUp()
    {
        WriteLog("call", nameof(PickUp), 2);

        ResetState();
        PlayPickUpSound();
    }

    public void Lunch()
    {
        WriteLog("call", nameof(Lunch), 2);

        PlayLaunchSound();

        SetState(1);
        _animator.Play(nameof(Lunch));
    }

    public void Equip()
    {
        WriteLog("call", nameof(Equip), 2);

        SetState(2);
        _isEquipped = true;
        _rider = _vrcPickup.currentPlayer;

        var chestPosition = _vrcPickup.currentPlayer.GetBonePosition(HumanBodyBones.Chest);
        var chestRotation = _vrcPickup.currentPlayer.GetBoneRotation(HumanBodyBones.Chest);

        var zikuDriverPosition = transform.position;
        var zikuDriverRotation = transform.rotation;

        SetPositionOffset(zikuDriverPosition - chestPosition);
        SetRotationOffset(zikuDriverRotation * Quaternion.Inverse(chestRotation));

        _animator.Play(nameof(Equip));
        PlayEquipSound();
        _vrcPickup.Drop();
    }

    public void AttachZioRideWatch()
    {
        WriteLog("call", nameof(AttachZioRideWatch), 2);

        Atach(_rideWatches[0]);
    }

    public void Atach(Transform rideWatchTransform)
    {
        WriteLog("call", nameof(Atach), 2);

        if ((transform.position - rideWatchTransform.position).x > 0)
        {
            rideWatchTransform.SetParent(_rSlot);
        }
        else
        {
            rideWatchTransform.SetParent(_lSlot);
        }

        rideWatchTransform.localPosition = Vector3.zero;
        rideWatchTransform.localRotation = Quaternion.identity;

        PlayAttachSound();
        PlayStandbySound();

        ((VRC_Pickup)rideWatchTransform.GetComponent(typeof(VRC_Pickup))).Drop();
    }

    public void DetachLSlot()
    {
        _lSlot.transform.DetachChildren();
    }

    public void DetachRSlot()
    {
        _rSlot.transform.DetachChildren();
    }
    #endregion

    #region Utilities
    public void ResetState()
    {
        WriteLog("call", nameof(ResetState), 2);

        _isEquipped = false;
        _isStandby = false;
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
