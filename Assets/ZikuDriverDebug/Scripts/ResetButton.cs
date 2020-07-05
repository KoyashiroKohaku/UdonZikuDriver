using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

public class ResetButton : UdonSharpBehaviour
{
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    [SerializeField]
    private ZikuDriver _zikuDriver;

    private void Start()
    {
        _initialPosition = new Vector3(0f, 0.05f, 0f);
        _initialRotation = Quaternion.identity;
    }

    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Reset));
    }

    public void Reset()
    {
        _zikuDriver.ResetState();
        _zikuDriver.gameObject.transform.position = _initialPosition;
        _zikuDriver.gameObject.transform.rotation = _initialRotation;
    }
}
