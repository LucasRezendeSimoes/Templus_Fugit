using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;  // assign na Inspector
    private Transform _target;                  // o inimigo
    private Vector3   _offset;                  // deslocamento acima da cabeça
    private Camera    _cam;
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
        if (_target == null) { Destroy(gameObject); return; }
        // converte posição world -> screen e coloca ali
        Vector3 screenPos = _cam.WorldToScreenPoint(_target.position + _offset);
        _rt.position = screenPos;
    }

    /// <summary>
    /// Chama sempre que o inimigo perde/ganha vida: valor entre 0 e 1.
    /// </summary>
    public void SetHealthPercent(float t)
    {
        _fillImage.fillAmount = Mathf.Clamp01(t);
    }
}