using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Store : MonoBehaviour
{
    public Button ballsButton, powerButton, incomeButton;
    public TextMeshProUGUI ballsCostTxt, ballsLevelTxt, powerCostTxt, powerLevelTxt, incomeCostTxt, incomeLevelTxt;

    GameManager gManager;
    // Start is called before the first frame update
    void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        UpdateStore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        ballsButton.interactable =  gManager.cash > GetBallsCost();
        powerButton.interactable = gManager.cash > GetPowerCost();
        incomeButton.interactable = gManager.cash > GetIncomeCost();
    }
    public void Upgrade(string val)
    {
        switch (val)
        {
            case "balls":
                gManager.cash -= GetBallsCost();
                gManager.ballsLevel++;
                break;
            case "power":
                gManager.cash -= GetPowerCost();
                gManager.powerLevel++;
                break;
            case "income":
                gManager.cash -= GetIncomeCost();
                gManager.incomeLevel++;
                break;
        }
        UpdateStore();
        gManager.SaveData();
    }
    void UpdateStore()
    {
        ballsLevelTxt.text = "LEVEL " + gManager.ballsLevel;
        powerLevelTxt.text = "LEVEL " + gManager.powerLevel;
        incomeLevelTxt.text = "LEVEL " + gManager.incomeLevel;

        ballsCostTxt.text = gManager.GetValue(GetBallsCost());
        powerCostTxt.text = gManager.GetValue(GetPowerCost());
        incomeCostTxt.text = gManager.GetValue(GetIncomeCost());
    }

    public int GetBallsCost()
    {
        return (int)Mathf.Pow((gManager.ballsLevel) * 10, 2);
    }
    public int GetPowerCost()
    {
        return (int)Mathf.Pow((gManager.powerLevel) * 10, 2);
    }
    public int GetIncomeCost()
    {
        return (int)Mathf.Pow((gManager.incomeLevel) * 10, 2);
    }

    public string GetValue(float val)
    {
        //return (Mathf.Round(val * (Mathf.Abs(val) < 1000 ? 10 : 0.1f)) / (Mathf.Abs(val) < 1000 ? 10 : 100)).ToString() + (Mathf.Abs(val) < 1000 ? "M" : "B");
        string str = null;

        if (val < 1000)
            str = Mathf.RoundToInt(val).ToString();
        else if (val < 1000000)
            str = Round2Frac(val / 1000) + "K";
        else if (val < 1000000000)
            str = Round2Frac(val / 1000000) + "M";
        else if (val < 1000000000000)
            str = Round2Frac(val / 1000000000) + "B";

        return str;
    }
    string Round2Frac(float val)
    {
        string str = null;

        if (Mathf.Round(val) < 10)
        {
            str = (Mathf.Round(val * 100) / 100).ToString();
        }
        else if (Mathf.Round(val) < 100)
        {
            str = (Mathf.Round(val * 10) / 10).ToString();
        }
        else
            str = Mathf.Round(val).ToString();

        return str;
    }
}