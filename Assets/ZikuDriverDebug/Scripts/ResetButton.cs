using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

public class ResetButton : UdonSharpBehaviour
{
    [SerializeField]
    private ZikuDriver _zikuDriver;

    [SerializeField]
    private RideWatch _rideWatch;

    private void Start()
    {
    }

    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Reset));
    }

    public void Reset()
    {
        _zikuDriver.ResetState();
        _zikuDriver.gameObject.transform.position = new Vector3(0f, 0.05f, 0f);
        _zikuDriver.gameObject.transform.rotation = Quaternion.identity;
        _zikuDriver.DetachLSlot();
        _zikuDriver.DetachRSlot();

        _rideWatch.ResetState();
        _rideWatch.gameObject.transform.position = new Vector3(0.2f, 0.05f, 0f);
        _rideWatch.gameObject.transform.rotation = Quaternion.identity;
    }
}
