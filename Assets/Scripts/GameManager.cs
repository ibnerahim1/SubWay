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
    public enum soundTypes { pop, upgrade, money, coins, tap, win};
    public Transform walls, floors, tables, breadBottom, italianBread, sesameBread, GrillBread, oatsBread, patty, grillPatty, ham, cheese, lettuce, tomato, cucumber;
    public Level[] levels;
    public Transform[] audioClips;

    [SerializeField] Transform cashObj, progressBar, camPositions, rows, ingredientContainer, sampleCam;
    [SerializeField] GameObject failPanel, winPanel, menuPanel, gamePanel, storePanel, cashPanel, whiteScreen, nextButton, breadSelection, ingredientSelection, tray, orderPanel, sampleButton, sampleFullButton;
    [SerializeField] TextMeshProUGUI dayTxt, cashTxt, orderCashTxt;
    [SerializeField] Customer customer;
    [SerializeField] GameObject[] stars;

    [HideInInspector] public bool gameStarted;
    [HideInInspector] public int level, currentLevel, wallLevel, floorLevel, tableLevel, cash, orderCash, touchCount;
    [HideInInspector] public enum Breads {italian, sesame, grill, oats};
    [HideInInspector] Breads breadType;
    [HideInInspector] public enum Ingredients { patty, grillPatty, ham, cheese, lettuce, tomato, cucumber};
    [HideInInspector] Ingredients ingredientType;

    private List<Ingredients> ingredientLayers = new List<Ingredients>();
    private Transform bread, breadBase, ingredient;
    private const int camTween = 0;
    private Camera cam;
    private Ingredients prevIngredient;
    private int targetTouchCount;
    private float dropHeight = 1.1f, popPitch = 0.5f, matchPercentage;
    private Level currentOrder = new Level();

    private void Awake()
    {
        LoadData();
    }
    private void Start()
    {
        cam = Camera.main;
        level = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 1;
        dayTxt.text = "DAY " + (((level - 1)/ 5) + 1);

        if ((level - 1) < levels.Length)
            currentOrder = levels[level - 1];
        else
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    currentOrder.breads = Breads.italian;
                    break;
                case 1:
                    currentOrder.breads = Breads.sesame;
                    break;
                case 2:
                    currentOrder.breads = Breads.grill;
                    break;
                case 3:
                    currentOrder.breads = Breads.oats;
                    break;
            }
            for (int i = 0; i < 7; i++)
            {
                switch (i)
                {
                    case 0:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.patty);
                        break;
                    case 1:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.grillPatty);
                        break;
                    case 2:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.ham);
                        break;
                    case 3:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.cheese);
                        break;
                    case 4:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.lettuce);
                        break;
                    case 5:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.tomato);
                        break;
                    case 6:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.cucumber);
                        break;
                }
            }
        }
        for (int i = 0; i < (level - 1) % 5; i++)
        {
            progressBar.GetChild(i).GetChild(0).gameObject.SetActive(true);
        }
        progressBar.GetChild((level - 1) % 5).GetChild(1).gameObject.SetActive(true);
        cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);
        cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);

        walls.GetChild(0).gameObject.SetActive(false);
        floors.GetChild(0).gameObject.SetActive(false);
        tables.GetChild(0).gameObject.SetActive(false);
        walls.GetChild(wallLevel - 1).gameObject.SetActive(true);
        floors.GetChild(floorLevel - 1).gameObject.SetActive(true);
        tables.GetChild(tableLevel - 1).gameObject.SetActive(true);
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
        if (popPitch > 0.5f)
            popPitch = Mathf.Lerp(popPitch, 0.5f, Time.deltaTime);
    }
    public void PlayLevel()
    {
        PlaySound(soundTypes.tap);

        customer.Walk();
        gameStarted = true;
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        cashPanel.SetActive(false);
        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(4).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(4).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
    }
    public void Next()
    {
        PlaySound(soundTypes.win);

        nextButton.SetActive(false);
        cam.transform.GetChild(0).gameObject.SetActive(true);
        ingredientSelection.SetActive(false);
        breadBase.DOMove(new Vector3(0, 1, 0), 0.5f).SetEase(Ease.Linear).SetDelay(0.5f);
        bread.DOJump(new Vector3(0, dropHeight, 0), dropHeight + 0.3f, 1, 1).SetEase(Ease.Linear);
        bread.DORotate(new Vector3(360, 0, 0), 1).SetEase(Ease.Linear).OnComplete(() =>
        {

            cashPanel.SetActive(true);
            gamePanel.SetActive(false);
            bread.parent = breadBase;
            breadBase.DOJump(breadBase.position, 0.2f, 1, 0.5f).SetEase(Ease.Linear).SetDelay(0.5f);
            tray.SetActive(true);

            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(4).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(4).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            StartCoroutine(LevelComplete());
        });
    }

    IEnumerator LevelComplete()
    {
        gameStarted = false;
        cashPanel.SetActive(true);
        level++;
        PlayerPrefs.SetInt("level", level);

        int MatchCount = 0;
        bool ingredientsMatched = true;

        for (int i = 0; i < currentOrder.ingredients.Count; i++)
        {
            if (!ingredientLayers.Contains(currentOrder.ingredients[i]))
                ingredientsMatched = false;
        }
        if(ingredientsMatched)
            MatchCount++;
        if (breadType == currentOrder.breads)
            MatchCount++;
        if(ingredientLayers.Count == currentOrder.ingredients.Count)
            MatchCount++;

        yield return new WaitForSeconds(0.5f);


        PlayHaptic(hapticTypes.success);
        PlaySound(soundTypes.money);

        switch (MatchCount)
        {
            case 0:
                customer.anim.Play("angry");
                break;
            case 1:
                customer.anim.Play("talk");
                orderCash = (int)(orderCash * 1.1f);
                break;
            case 2:
                customer.anim.Play("happy");
                orderCash = (int)(orderCash * 1.2f);
                break;
            case 3:
                customer.anim.Play("jump");
                orderCash = (int)(orderCash * 1.3f);
                break;
        }
        GetComponent<CoinMagnet>().SpawnCoins((int)(orderCash * 0.1f));
        cash += orderCash;
        SaveData();

        yield return new WaitForSeconds(2);

        gamePanel.SetActive(false);
        cashPanel.SetActive(false);
        winPanel.SetActive(true);
        Instantiate(breadBase, new Vector3(0, -10, 0), Quaternion.identity);

        yield return new WaitForSeconds(1.5f);
        switch (MatchCount)
        {
            case 1:
                stars[0].SetActive(true);
                break;
            case 2:
                stars[0].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                stars[1].SetActive(true);
                break;
            case 3:
                stars[0].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                stars[1].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                stars[2].SetActive(true);
                break;
        }
    }
    //public void LevelFail()
    //{
    //    if (gameStarted)
    //    {
    //        PlayHaptic(hapticTypes.failure);

    //        SaveData();

    //        failPanel.SetActive(true);
    //        gameStarted = false;
    //    }
    //}
    public void Restart()
    {
        PlaySound(soundTypes.tap);

        whiteScreen.GetComponent<Image>().DOFade(1, 0.5f).SetEase(Ease.Linear).OnComplete(()=> SceneManager.LoadScene(0));
    }

    public void LoadData()
    {
        cash = PlayerPrefs.HasKey("cash") ? PlayerPrefs.GetInt("cash") : 0;
        wallLevel = PlayerPrefs.HasKey("wallLevel") ? PlayerPrefs.GetInt("wallLevel") : 1;
        floorLevel = PlayerPrefs.HasKey("floorLevel") ? PlayerPrefs.GetInt("floorLevel") : 1;
        tableLevel = PlayerPrefs.HasKey("tableLevel") ? PlayerPrefs.GetInt("tableLevel") : 1;
    }
    public void SaveData()
    {
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("cash", cash);

        PlayerPrefs.SetInt("wallLevel", wallLevel);
        PlayerPrefs.SetInt("floorLevel", floorLevel);
        PlayerPrefs.SetInt("tableLevel", tableLevel);
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
    public void PlaySound(soundTypes sType)
    {
        //Destroy(soundIns.gameObject, 1);
        switch (sType)
        {
            case soundTypes.pop:
                Transform temp = Instantiate(audioClips[0]);
                temp.GetComponent<AudioSource>().pitch = popPitch;
                popPitch += 0.05f;
                break;
            case soundTypes.upgrade:
                Instantiate(audioClips[1]);
                break;
            case soundTypes.money:
                Instantiate(audioClips[2]);
                break;
            case soundTypes.coins:
                Instantiate(audioClips[0]);
                break;
            case soundTypes.tap:
                Instantiate(audioClips[3]);
                break;
            case soundTypes.win:
                Instantiate(audioClips[4]);
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
    public void ToggleStore(bool condition)
    {
        PlaySound(soundTypes.tap);

        if (condition)
        {
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(3).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(3).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            storePanel.SetActive(true);
            menuPanel.SetActive(false);
        }
        else
        {
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            storePanel.SetActive(false);
            menuPanel.SetActive(true);
        }
    }
    public void BreadSelect(string name)
    {
        PlaySound(soundTypes.tap);

        switch (name)
        {
            case "italian":
                breadType = Breads.italian;
                bread = Instantiate(italianBread, new Vector3(0, 1.5f, 0), Quaternion.identity);
                break;
            case "sesame":
                breadType = Breads.sesame;
                bread = Instantiate(sesameBread, new Vector3(0, 1.5f, 0), Quaternion.identity);
                break;
            case "grill":
                breadType = Breads.grill;
                bread = Instantiate(GrillBread, new Vector3(0, 1.5f, 0), Quaternion.identity);
                break;
            case "oats":
                breadType = Breads.oats;
                bread = Instantiate(oatsBread, new Vector3(0, 1.5f, 0), Quaternion.identity);
                break;
        }
        breadSelection.SetActive(false);
        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);

        breadBase = Instantiate(breadBottom, new Vector3(0, 1.4f, 0), Quaternion.identity);
        breadBase.DOMove(new Vector3(0, 1, 0), 0.5f).SetEase(Ease.Linear);
        bread.DOMove(new Vector3(0, 1.1f, 0), 0.5f).SetEase(Ease.Linear);

        breadBase.DOMove(new Vector3(0, 1, -0.25f), 0.5f).SetEase(Ease.Linear).SetDelay(0.5f);
        bread.DOJump(new Vector3(0, 1.2f, 0.25f), 0.3f, 1, 1).SetEase(Ease.Linear).SetDelay(0.5f);
        bread.DORotate(new Vector3(180, 0, 0), 1).SetEase(Ease.Linear).SetDelay(0.5f).OnComplete(()=>
        {
            ingredientSelection.SetActive(true);
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        });
    }
    public void SelectIngredient(string name)
    {
        PlaySound(soundTypes.tap);

        touchCount = 0;
        nextButton.SetActive(false);
        switch (name)
        {
            case "patty":
                ingredientContainer.GetChild(0).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.patty;
                targetTouchCount = 5;
                dropHeight += 0.02f;
                ingredient = patty;
                InitialiseSpheres(1);
                break;

            case "grillPatty":
                ingredientContainer.GetChild(1).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.grillPatty;
                targetTouchCount = 5;
                dropHeight += 0.02f;
                ingredient = grillPatty;
                InitialiseSpheres(1);
                break;

            case "ham":
                ingredientContainer.GetChild(2).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.ham;
                targetTouchCount = 5;
                dropHeight += 0.01f;
                ingredient = ham;
                InitialiseSpheres(1);
                break;

            case "cheese":
                ingredientContainer.GetChild(3).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.cheese;
                targetTouchCount = 3;
                dropHeight += 0.01f;
                ingredient = cheese;
                InitialiseSpheres(0);
                break;

            case "lettuce":
                ingredientContainer.GetChild(4).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.lettuce;
                targetTouchCount = 5;
                dropHeight += 0.01f;
                ingredient = lettuce;
                InitialiseSpheres(1);
                break;

            case "tomato":
                ingredientContainer.GetChild(5).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.tomato;
                targetTouchCount = 7;
                dropHeight += 0.01f;
                ingredient = tomato;
                InitialiseSpheres(2);
                break;

            case "cucumber":
                ingredientContainer.GetChild(6).GetComponent<Button>().interactable = false;
                ingredientType = Ingredients.cucumber;
                targetTouchCount = 7;
                dropHeight += 0.01f;
                ingredient = cucumber;
                InitialiseSpheres(2);
                break;
        }
        ingredientSelection.SetActive(false);
        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
    }
    private void InitialiseSpheres(int child)
    {
        for (int i = 0; i < rows.GetChild(child).childCount; i++)
        {
            rows.GetChild(child).GetChild(i).GetComponent<TipRenderer>().Initialise(dropHeight);
        }
    }
    public void Touched(Vector3 position)
    {
        PlaySound(soundTypes.pop);

        touchCount++;
        Transform temp = Instantiate(ingredient, position + new Vector3(0, 0.2f, 0), ingredient.rotation);
        temp.DOMoveY(dropHeight, 0.5f).SetEase(Ease.Linear);
        temp.parent = breadBase;

        if (touchCount > targetTouchCount - 1)
        {
            switch (ingredientType)
            {
                case Ingredients.patty:
                    dropHeight += 0.05f;
                    break;

                case Ingredients.grillPatty:
                    dropHeight += 0.05f;
                    break;

                case Ingredients.ham:
                    dropHeight += 0.01f;
                    break;

                case Ingredients.cheese:
                    dropHeight += 0.02f;
                    break;

                case Ingredients.lettuce:
                    dropHeight += 0.01f;
                    break;

                case Ingredients.tomato:
                    dropHeight += 0.02f;
                    break;

                case Ingredients.cucumber:
                    dropHeight += 0.02f;
                    break;
            }
            ingredientLayers.Add(ingredientType);
            if (ingredientLayers.Count > ingredientContainer.childCount - 1)
                Next();
            else
            {
                DOTween.Kill(camTween);
                cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() => ingredientSelection.SetActive(true));
            }
            if (ingredientLayers.Count > currentOrder.ingredients.Count - 1)
                nextButton.SetActive(true);
        }
    }

    public IEnumerator makeSample()
    {
        orderCash = 150 + (level * 50);
        orderPanel.SetActive(true);
        orderCashTxt.text = GetValue(orderCash);
        sampleCam.GetComponent<Camera>().orthographicSize = 0.6f;
        float sampleHeight = 0, heightAdder = 0;
        int nodes = 1;
        List<Transform> sampleLayers = new List<Transform>();
        Transform temp = null, tempParent = null;
        yield return new WaitForSeconds(0.5f);

        Instantiate(breadBottom, new Vector3(0, -0.1f, 10), Quaternion.identity);

        for (int i = 0; i < currentOrder.ingredients.Count; i++)
        {
            yield return new WaitForSeconds(0.25f);

            switch (currentOrder.ingredients[i])
            {
                case Ingredients.patty:
                    temp = patty;
                    sampleHeight += 0.02f;
                    heightAdder = 0.05f;
                    break;

                case Ingredients.grillPatty:
                    temp = grillPatty;
                    sampleHeight += 0.02f;
                    heightAdder = 0.05f;
                    break;

                case Ingredients.ham:
                    temp = ham;
                    sampleHeight += 0.01f;
                    heightAdder = 0.01f;
                    break;

                case Ingredients.cheese:
                    temp = cheese;
                    nodes = 0;
                    sampleHeight += 0.01f;
                    heightAdder = 0.02f;
                    break;

                case Ingredients.lettuce:
                    temp = lettuce;
                    sampleHeight += 0.01f;
                    heightAdder = 0.01f;
                    break;

                case Ingredients.tomato:
                    temp = tomato;
                    nodes = 2;
                    sampleHeight += 0.01f;
                    heightAdder = 0.02f;
                    break;

                case Ingredients.cucumber:
                    temp = cucumber;
                    nodes = 2;
                    sampleHeight += 0.01f;
                    heightAdder = 0.02f;
                    break;
            }
            for (int j = 0; j < rows.GetChild(nodes).childCount; j++)
            {
                yield return new WaitForSeconds(0.05f);

                if(j==0)
                {
                    tempParent = Instantiate(temp, new Vector3(rows.GetChild(nodes).GetChild(j).position.x, sampleHeight, 10), temp.rotation);
                    sampleLayers.Add(tempParent);
                }
                else
                    Instantiate(temp, new Vector3(rows.GetChild(nodes).GetChild(j).position.x, sampleHeight, 10), temp.rotation, tempParent);
            }
            temp.parent = breadBase;
            sampleHeight += heightAdder;
        }
        yield return new WaitForSeconds(0.25f);

        switch (currentOrder.breads)
        {
            case Breads.italian:
                sampleLayers.Add(Instantiate(italianBread, new Vector3(0, sampleHeight, 10), Quaternion.identity));
                break;
            case Breads.sesame:
                sampleLayers.Add(Instantiate(sesameBread, new Vector3(0, sampleHeight, 10), Quaternion.identity));
                break;
            case Breads.grill:
                sampleLayers.Add(Instantiate(GrillBread, new Vector3(0, sampleHeight, 10), Quaternion.identity));
                break;
            case Breads.oats:
                sampleLayers.Add(Instantiate(oatsBread, new Vector3(0, sampleHeight, 10), Quaternion.identity));
                break;
        }

        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(4).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(4).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() => breadSelection.SetActive(true));

        sampleCam.DOMove(new Vector3(0, 1, 9), 0.5f).SetDelay(0.25f).SetEase(Ease.Linear);
        sampleCam.DORotate(new Vector3(25, 0, 0), 0.5f).SetDelay(0.25f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < sampleLayers.Count; i++)
        {
            yield return new WaitForSeconds(0.1f);

            for (int j = i; j < sampleLayers.Count; j++)
            {

                sampleLayers[j].DOMoveZ(10 + (i * 0.2f), 0.2f).SetEase(Ease.Linear);
            }
        }
    }
    public void SampleFullScreen(bool condn)
    {
        if (condn)
        {
            sampleButton.SetActive(false);
            sampleFullButton.SetActive(true);
        }
        else
        {
            sampleButton.SetActive(true);
            sampleFullButton.SetActive(false);
        }

    }
}
[System.Serializable]
public class Level
{
    public GameManager.Breads breads;
    public List<GameManager.Ingredients> ingredients = new List<GameManager.Ingredients>();
}