using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;

public class GameManager : MonoBehaviour
{
    public enum hapticTypes {soft, light, medium, heavy, success, failure};

    [SerializeField] Transform cashObj, popSound, leaderBoard;
    [SerializeField]GameObject failPanel, winPanel, menuPanel, gamePanel, storePanel;
    [SerializeField]TextMeshProUGUI levelTxt, cashTxt;

    [HideInInspector] public bool gameStarted, onUI;
    [HideInInspector] public int level, ballsLevel, powerLevel, incomeLevel, cash, score, rank;
    [HideInInspector]public float fov, time;


    private Camera cam;
    private bool once;

    private void Awake()
    {
        LoadData();
    }
    private void Start()
    {
        cam = Camera.main;
        fov = cam.fieldOfView;
        level = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 1;
        levelTxt.text = "LEVEL " + level;
    }

    void Update()
    {
        #region MyDebug
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
            Restart();
        if (Input.GetKeyDown("b"))
            Debug.Break();
        if (Input.GetKeyDown("d"))
            PlayerPrefs.DeleteKey("level");
        if (Input.GetKeyDown("d") && Input.GetKey(KeyCode.LeftShift))
            PlayerPrefs.DeleteAll();
        if (Input.GetKeyDown("n"))
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") + 1);
#endif
        #endregion
        cashTxt.text = GetValue(cash);

        if(!gameStarted && !onUI && !once && Input.GetMouseButtonDown(0))
        {
            gameStarted = true;
            once = true;
        }
    }

    public void LevelComplete()
    {
        if (gameStarted)
        {
            PlayHaptic(hapticTypes.success);

            gameStarted = false;
            level++;
            PlayerPrefs.SetInt("level", level);
            levelTxt.text = "LEVEL " + level;
            winPanel.SetActive(true);
            cam.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    public void LevelFail()
    {
        if (gameStarted)
        {
            PlayHaptic(hapticTypes.failure);

            Names names = FindObjectOfType<Names>();

            rank = (int)(rank * Random.Range(0.95f, 0.99f));
            int d = Random.Range(1, 5);
            for (int i = 0; i < leaderBoard.childCount; i++)
            {
                leaderBoard.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = (rank - 3 + i).ToString();
                if (i == d)
                {
                    leaderBoard.GetChild(i).DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear).SetDelay(1.5f);
                    leaderBoard.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "YOU";
                    leaderBoard.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                    leaderBoard.GetChild(i).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = score.ToString();
                }
                else
                {
                    leaderBoard.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = names.names[Random.Range(0, 1000)];
                    leaderBoard.GetChild(i).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = ((int)(score + ( (d - i) * score * Random.Range(0.05f, 0.09f)))).ToString();
                }
            }
            SaveData();

            failPanel.SetActive(true);
            gameStarted = false;
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadData()
    {
        cash = PlayerPrefs.HasKey("cash") ? PlayerPrefs.GetInt("cash") : 0;
        ballsLevel = PlayerPrefs.HasKey("ballsLevel") ? PlayerPrefs.GetInt("ballsLevel") : 1;
        powerLevel = PlayerPrefs.HasKey("powerLevel") ? PlayerPrefs.GetInt("powerLevel") : 1;
        incomeLevel = PlayerPrefs.HasKey("incomeLevel") ? PlayerPrefs.GetInt("incomeLevel") : 1;
        score = PlayerPrefs.HasKey("score") ? PlayerPrefs.GetInt("score") : 0;
        rank = PlayerPrefs.HasKey("rank") ? PlayerPrefs.GetInt("rank") : Random.Range(3000, 5000);
    }
    public void SaveData()
    {
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("cash", cash);

        PlayerPrefs.SetInt("ballsLevel", ballsLevel);
        PlayerPrefs.SetInt("powerLevel", powerLevel);
        PlayerPrefs.SetInt("incomeLevel", incomeLevel);

        PlayerPrefs.SetInt("score", score);
        PlayerPrefs.SetInt("rank", rank);
    }

    public void PlayHaptic(hapticTypes hType)
    {
        switch (hType)
        {
            case hapticTypes.soft:
                MMVibrationManager.Haptic(HapticTypes.SoftImpact);
                break;
            case hapticTypes.light:
                MMVibrationManager.Haptic(HapticTypes.LightImpact);
                break;
            case hapticTypes.medium:
                MMVibrationManager.Haptic(HapticTypes.MediumImpact);
                break;
            case hapticTypes.heavy:
                MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
                break;
            case hapticTypes.success:
                MMVibrationManager.Haptic(HapticTypes.Success);
                break;
            case hapticTypes.failure:
                MMVibrationManager.Haptic(HapticTypes.Failure);
                break;
        }
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

    public void OnUI(bool val)
    {
        onUI = val;
    }
}