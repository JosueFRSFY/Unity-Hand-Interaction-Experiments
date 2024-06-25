using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketingCamera : MonoBehaviour
{
    [SerializeField] float damping = 15f;
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Camera _marketingCamera;
    private bool _enableMarketingCam = false;

    void Awake()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        _enableMarketingCam = true;
#else
        _enableMarketingCam = false;
#endif
        _marketingCamera.gameObject.SetActive(_enableMarketingCam);
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (_enableMarketingCam)
        {
            _marketingCamera.transform.position = _mainCam.transform.position;
            _marketingCamera.transform.rotation = _mainCam.transform.rotation;
        }
    }

    private void Update()
    {
        if (!_enableMarketingCam)
        {
            return;
        }
        _marketingCamera.transform.position = Vector3.Lerp(_marketingCamera.transform.position, _mainCam.transform.position, Time.deltaTime * damping);
        _marketingCamera.transform.rotation = Quaternion.Slerp(_marketingCamera.transform.rotation, _mainCam.transform.rotation, Time.deltaTime * damping);

    }
}
