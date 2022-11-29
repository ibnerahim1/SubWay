using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    public Animator anim;
    [SerializeField] GameManager gmanager;

    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).GetChild(Random.Range(0, transform.GetChild(0).childCount)).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Walk()
    {
        anim.Play("walk");
        transform.DOMoveX(0, 2).SetEase(Ease.Linear);
        transform.DORotate(new Vector3(0, 180, 0), 0.5f).SetEase(Ease.Linear).SetDelay(2).OnComplete(() =>
        {
            anim.Play("talk");
            StartCoroutine(gmanager.makeSample());
        });
    }
}
