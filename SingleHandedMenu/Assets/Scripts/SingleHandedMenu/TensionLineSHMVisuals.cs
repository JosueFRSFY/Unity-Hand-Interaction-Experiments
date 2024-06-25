using Leap.Unity.Interaction.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TensionLineSHMVisuals : MonoBehaviour
{
    public SingleHandedMenu singleHandedMenu;

    [SerializeField]
    public GameObject lineTensionPrefab;
    [SerializeField]
    private GameObject _particlesPrefab;
    [SerializeField]
    public GameObject _reattachmentAnchorHand, _reattachmentAnchorMenu;

    private LineTension _lineTension;


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

    private void OnDetachStart()
    {
        if (_lineTension == null)
        {
            _lineTension = Instantiate(lineTensionPrefab).GetComponent<LineTension>();
            _lineTension.enabled = true;
            _lineTension.SetupLineTension(_reattachmentAnchorMenu.transform, _reattachmentAnchorHand.transform, 0.2f);
        }
        else if (!_lineTension.gameObject.activeInHierarchy)
        {
            _lineTension.gameObject.SetActive(true);
        }
    }

    private void OnDetaching(float t)
    {
        if (_lineTension != null)
        {
            _lineTension.UpdateLineTension(t);
        }
    }

    private void OnDetachEnd(bool detached)
    {
        if (detached)
        {
            if (_lineTension != null)
            {
                _lineTension.UpdateLineTension(1);
            }
        }
        else
        {
            if (_lineTension != null && _lineTension.gameObject.activeInHierarchy)
            {
                _lineTension.gameObject.SetActive(false);
            }
        }


        _reattachmentAnchorHand.SetActive(false);
        _reattachmentAnchorMenu.SetActive(false);
    }

    private void OnAttachStart()
    {
        _reattachmentAnchorHand.SetActive(true);
        _reattachmentAnchorMenu.SetActive(true);
    }

    private void OnAttaching(float t)
    {
    }

    private void OnAttachEnd(bool attached)
    {
        if (_lineTension != null && _lineTension.gameObject.activeInHierarchy)
        {
            _lineTension.gameObject.SetActive(false);
        }

        _reattachmentAnchorHand.SetActive(false);
        _reattachmentAnchorMenu.SetActive(false);
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
}
