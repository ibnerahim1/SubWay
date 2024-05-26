using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TipRenderer : MonoBehaviour
{
    public bool active;
    private Material material;
    private GameManager gManager;

    private void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        material = GetComponent<MeshRenderer>().material;
        material.DOFade(0, 0);
    }

    public void Initialise(float height)
    {
        active = true;
        transform.position = new Vector3(transform.position.x, height + 0.05f, transform.position.z);
        material.DOFade(1f, 0.5f).SetEase(Ease.InSine).SetLoops(-1, LoopType.Yoyo).SetId(transform.GetHashCode());
    }
    private void OnMouseEnter()
    {
        if(active && !PlayerPrefs.HasKey("tap"))
        {
            active = false;
            DOTween.Kill(transform.GetHashCode());
            material.DOFade(0, 0);
            gManager.Touched(transform.position);
        }
    }
    private void OnMouseDown()
    {
        if (active && PlayerPrefs.HasKey("tap"))
        {
            active = false;
            DOTween.Kill(transform.GetHashCode());
            material.DOFade(0, 0);
            gManager.Touched(transform.position);
        }
    }
}
