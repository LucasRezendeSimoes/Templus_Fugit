using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;  // assign na Inspector
    private Transform    _target;
    private Vector3      _offset;
    private Camera       _cam;
    private RectTransform _rt;

    public void Initialize(Transform target, Vector3 offset)
    {
        _target = target;
        _offset = offset;
        _cam    = Camera.main;
        _rt     = GetComponent<RectTransform>();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 screenPos = _cam.WorldToScreenPoint(_target.position + _offset);
        _rt.position = screenPos;
    }

    public void SetHealthPercent(float t)
    {
        _fillImage.fillAmount = Mathf.Clamp01(t);
    }
}