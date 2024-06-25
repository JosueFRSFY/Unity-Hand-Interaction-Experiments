using Leap.Unity.Interaction.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineSHMVisuals : MonoBehaviour
{
    public SingleHandedMenu singleHandedMenu;

    [SerializeField]
    private GameObject _particlesPrefab;
    [SerializeField]
    private SpriteRenderer _outline, _partialOutline;

    [SerializeField]
    private Transform _uiAnchor, _handAnchor;

    [SerializeField]
    private AnimationCurve _positionCurve, _rotationCurve, _alphaCurve;

    private float _targetOutlineAlpha = 0;
    private float _targetPartialOutlineAlpha = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        singleHandedMenu.OnAttachStart.AddListener(OnAttachStart);
        singleHandedMenu.OnAttaching.AddListener(OnAttaching);
        singleHandedMenu.OnAttachEnd.AddListener(OnAttachEnd);

        singleHandedMenu.OnDetachStart.AddListener(OnDetachStart);
        singleHandedMenu.OnDetaching.AddListener(OnDetaching);
        singleHandedMenu.OnDetachEnd.AddListener(OnDetachEnd);

        singleHandedMenu.OnMenuShow.AddListener(OnMenuShow);
        singleHandedMenu.OnMenuHide.AddListener(OnMenuHide);
    }

    private void OnDisable()
    {
        singleHandedMenu.OnAttachStart.RemoveListener(OnAttachStart);
        singleHandedMenu.OnAttaching.RemoveListener(OnAttaching);
        singleHandedMenu.OnAttachEnd.RemoveListener(OnAttachEnd);

        singleHandedMenu.OnDetachStart.RemoveListener(OnDetachStart);
        singleHandedMenu.OnDetaching.RemoveListener(OnDetaching);
        singleHandedMenu.OnDetachEnd.RemoveListener(OnDetachEnd);

        singleHandedMenu.OnMenuShow.RemoveListener(OnMenuShow);
        singleHandedMenu.OnMenuHide.RemoveListener(OnMenuHide);
    }

    private void Start()
    {
        _targetOutlineAlpha = 0;
        _targetPartialOutlineAlpha = 0;

        Color c = _outline.color;
        c.a = 0;

        _outline.color = c;
        _partialOutline.color = c;
    }

    private void Update()
    {
        Color c = _outline.color;
        c.a = Mathf.Lerp(c.a, _targetOutlineAlpha, Time.deltaTime * 15f);
        _outline.color = c;

        c = _partialOutline.color;
        c.a = Mathf.Lerp(c.a, _targetPartialOutlineAlpha, Time.deltaTime * 15f);
        _partialOutline.color = c;
    }

    private void OnDetachStart()
    {
        // Show Outline

        _targetOutlineAlpha = 1;
        _targetPartialOutlineAlpha = 1;
    }

    private void OnDetaching(float t)
    {
        // Pull outline
        // Vary outline transparency

        Vector3 newPos = Vector3.Lerp(_uiAnchor.transform.position, _handAnchor.transform.position, _positionCurve.Evaluate(t));
        Quaternion newRot = Quaternion.Slerp(_uiAnchor.transform.rotation, _handAnchor.transform.rotation, _rotationCurve.Evaluate(t));

        _outline.transform.position = newPos;
        _outline.transform.rotation = newRot;

        _partialOutline.transform.parent.position = newPos;
        _partialOutline.transform.parent.rotation = newRot;

        Color c = _outline.color;
        c.a = 1 - _alphaCurve.Evaluate(t);
        _outline.color = c;

    }

    private void OnDetachEnd(bool detached)
    {
        _targetOutlineAlpha = 0;
        _targetPartialOutlineAlpha = 0;

        StartCoroutine(SyncPose(1, _outline.transform, _handAnchor));
        StartCoroutine(SyncPose(1, _partialOutline.transform.parent, _handAnchor));
    }

    private void OnAttachStart()
    {
        // Show dock

        _targetOutlineAlpha = 0;
        _targetPartialOutlineAlpha = 1;

        Color c = _outline.color;
        c.a = 0;
        _outline.color = c;
    }

    private void OnAttaching(float t)
    {
        //pull dock towards hand
        //vary dock transparency back to outline

        Vector3 newPos = Vector3.Lerp(_handAnchor.transform.position, _uiAnchor.transform.position, t);
        Quaternion newRot = Quaternion.Slerp(_handAnchor.transform.rotation, _uiAnchor.transform.rotation, t);

        _outline.transform.position = newPos;
        _outline.transform.rotation = newRot;

        _partialOutline.transform.parent.position = newPos;
        _partialOutline.transform.parent.rotation = newRot;


        Color c = _outline.color;
        c.a = t;
        _outline.color = c;
    }

    private void OnAttachEnd(bool attached)
    {
        // hide dock if failed
        // snap outline then fade if success

        _targetOutlineAlpha = 0;
        _targetPartialOutlineAlpha = 0;

        StartCoroutine(SyncPose(1, _outline.transform, _uiAnchor));
        StartCoroutine(SyncPose(1, _partialOutline.transform.parent, _uiAnchor));
    }

    private void OnMenuShow()
    {
    }

    private void OnMenuHide()
    {
        if (!singleHandedMenu.attachedToHand)
        {
            Instantiate(_particlesPrefab, singleHandedMenu.grabBall.attachedObject.position, singleHandedMenu.grabBall.attachedObject.rotation);
        }
    }

    private IEnumerator SyncPose(float seconds, Transform poseToBeSynced, Transform targetPose)
    {
        float elapsed = 0;
        while (elapsed < seconds)
        {
            poseToBeSynced.position = targetPose.position;
            poseToBeSynced.rotation = targetPose.rotation;
            yield return null;
            elapsed += Time.deltaTime;
        }

    }
}
