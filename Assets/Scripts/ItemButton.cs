using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButton : MonoBehaviour
{
    public int type;
    private GameManager gManager;

    private void Start()
    {
        gManager = FindObjectOfType<GameManager>();
    }

    //private void OnMouseEnter()
    //{
    //    if (active && !PlayerPrefs.HasKey("tap"))
    //    {
    //        active = false;
    //        DOTween.Kill(transform.GetHashCode());
    //        material.DOFade(0, 0);
    //        gManager.Touched(transform.position);
    //    }
    //}
    private void OnMouseDown()
    {
        if (Helpers.IsOnUI()) return;
        if(type == 0)
            gManager.BreadSelect(name);
        if (type == 1)
        {
            gManager.SelectIngredient(name);
            gameObject.SetActive(false);
        }
        if (type == 2)
        {
            gManager.SauceSelect(name);
            gameObject.SetActive(false);
        }
    }

}
