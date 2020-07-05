using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class ZikuDriver : UdonSharpBehaviour
{
    private readonly string _parameterName = "State";
    private Animator _animator;
    private VRC_Pickup _vrcPickup;
    private bool _isEquipped = false;
    private VRCPlayerApi _rider;
    private int _state = 0;
    private float _offsetY;
    private float _offsetZ;

    [SerializeField]
    private bool _isDebug = false;

    [SerializeField]
    private Text _text;

    #region Events
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _vrcPickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
    }

    private void Update()
    {
        if (_isEquipped)
        {
            var chestPosition = _rider.GetBonePosition(HumanBodyBones.Chest);
            var chestRotation = _rider.GetBoneRotation(HumanBodyBones.Chest);

            transform.position = new Vector3(chestPosition.x, chestPosition.y + _offsetY, chestPosition.z + _offsetZ);
        }
    }

    public override void OnPickup()
    {
        WriteLog("call", nameof(OnPickup));
        Reset();
    }

    public override void OnPickupUseDown()
    {
        WriteLog("call", nameof(OnPickupUseDown));

        switch (_state)
        {
            case 0:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(LunchZikuDriver));
                break;
            case 1:
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PutOnZikuDriver));
                break;
        }
    }
    #endregion

    #region Network Events
    public void Reset()
    {
        WriteLog("call", nameof(Reset));

        _isEquipped = false;
        _rider = null;
        SetState(0);
        SetOffsetY(0);
        SetOffsetZ(0);
    }

    public void LunchZikuDriver()
    {
        WriteLog("call", nameof(LunchZikuDriver));

        WriteLog("audio", "＼ｼﾞｸｳﾄﾞﾗｲﾊﾞｰ／");

        SetState(1);
        _animator.Play(nameof(LunchZikuDriver));
    }

    public void PutOnZikuDriver()
    {
        WriteLog("call", nameof(PutOnZikuDriver));

        SetState(2);
        _isEquipped = true;
        _rider = _vrcPickup.currentPlayer;

        var chestPosition = _vrcPickup.currentPlayer.GetBonePosition(HumanBodyBones.Chest);
        var chestRotation = _vrcPickup.currentPlayer.GetBoneRotation(HumanBodyBones.Chest);

        var zikuDriverPosition = transform.position;
        var zikuDriverRotation = transform.rotation;

        var offset = chestRotation * (chestPosition - zikuDriverPosition);

        SetOffsetY(offset.y);
        SetOffsetZ(offset.z);

        _animator.Play(nameof(PutOnZikuDriver));
        _vrcPickup.Drop();
    }
    #endregion

    #region Setter (with WriteLog)
    private void SetState(int value)
    {
        WriteLog(nameof(_state), _state + "=>" + value);
        _state = value;
    }

    private void SetOffsetY(float value)
    {
        WriteLog(nameof(_offsetZ), _offsetY + "=>" + value);
        _offsetY = value;
    }

    private void SetOffsetZ(float value)
    {
        WriteLog(nameof(_offsetZ), _offsetZ + "=>" + value);
        _offsetZ = value;
    }
    #endregion

    #region Utilities
    private void WriteLog(string type, string message)
    {
        if (_isDebug)
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
