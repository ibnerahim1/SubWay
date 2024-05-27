using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using Dreamteck.Splines;
using SupersonicWisdomSDK;

public class GameManager : MonoBehaviour
{
    public enum hapticTypes { soft, light, medium, heavy, success, failure };
    public enum soundTypes { pop, upgrade, money, coins, tap, win };
    public Transform walls, floors, tables, breadBottom, italianBread, sesameBread, GrillBread, oatsBread, patty, grillPatty, ham, cheese, lettuce, tomato, cucumber, sauceBottle, knife, paper, ovenDoor;
    public Level[] levels;
    public Transform[] audioClips;
    public Image bakeFill;

    [SerializeField] Transform[] saucesObj;
    [SerializeField] Transform cashObj, progressBar, camPositions, rows, ingredientContainer, sauceContainer, sampleCam;
    [SerializeField] GameObject failPanel, winPanel, menuPanel, gamePanel, storePanel, cashPanel, whiteScreen, nextButton, breadSelection, ingredientSelection, sauceSelection, cheeseNToastPrompt, tray, orderPanel, sampleButton, sampleFullButton, cutTut, squeezeTut;
    [SerializeField] TextMeshProUGUI dayTxt, cashTxt, orderCashTxt;
    [SerializeField] Customer customer;
    [SerializeField] GameObject[] stars;

    [HideInInspector] public bool gameStarted;
    [HideInInspector] public int level, currentLevel, wallLevel, floorLevel, tableLevel, cash, orderCash, touchCount;
    [HideInInspector] public enum Step { acceptOrder, chooseBread, cheeseNtoast, ingredients, sauces, done };
    [HideInInspector] Step steps;
    [HideInInspector] public enum Breads { italian, sesame, grill, oats };
    [HideInInspector] Breads breadType;
    [HideInInspector] public enum Ingredients { patty, grillPatty, ham, cheese, lettuce, tomato, cucumber };
    [HideInInspector] Ingredients ingredientType;
    [HideInInspector] public enum Sauce { chilli, mayo, mustard };
    [HideInInspector] Sauce sauceType;
    [HideInInspector] bool cheeseNtoast;

    private List<Ingredients> ingredientLayers = new List<Ingredients>();
    private List<Sauce> sauces = new List<Sauce>();
    private Transform bread, breadBase, ingredient;
    private const int camTween = 0;
    private Camera cam;
    private Ingredients prevIngredient;
    private int targetTouchCount, sauceCount;
    private float dropHeight = 1.6f, popPitch = 0.5f, matchPercentage, squeezeVal;
    private Level currentOrder = new Level();
    private TubeGenerator currentSauce;
    private SplinePositioner bottlePosition;
    public bool cut, squeeze, sauceadded;

    public Texture2D hand, tap;
    private bool mouseDown;

    private void Awake()
    {
        LoadData();
    }

    private void Start()
    {
        cam = Camera.main;
        level = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 1;
        dayTxt.text = "DAY " + (((level - 1) / 5) + 1);
        bottlePosition = sauceBottle.GetComponent<SplinePositioner>();

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
            currentOrder.cheeseNtoast = Random.Range(0, 3) > 0;
            if (currentOrder.cheeseNtoast)
                currentOrder.ingredients.Add(Ingredients.cheese);

            for (int i = 0; i < 6; i++)
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
                            currentOrder.ingredients.Add(Ingredients.lettuce);
                        break;
                    case 4:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.tomato);
                        break;
                    case 5:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.ingredients.Add(Ingredients.cucumber);
                        break;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.sauces.Add(Sauce.chilli);
                        break;
                    case 1:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.sauces.Add(Sauce.mayo);
                        break;
                    case 2:
                        if (Random.Range(0, 3) > 0)
                            currentOrder.sauces.Add(Sauce.mustard);
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
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            mouseDown = true;
        if (Input.GetMouseButtonUp(0))
            mouseDown = false;
        Cursor.SetCursor(mouseDown? tap: hand, new Vector2(35, 35), CursorMode.ForceSoftware);
#endif

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

        if (Input.GetMouseButtonDown(0))
        {
            if (cut)
            {
                knife.DOMoveX(-0.6f, Mathf.Abs(knife.position.x + 0.6f) * 2).SetEase(Ease.Linear).SetId(2).OnComplete(() =>
                {
                    cut = false;
                    cutTut.SetActive(false);
                    DOTween.Kill(2);
                    knife.DOMove(new Vector3(-3, 1.55f, -2), 1).SetEase(Ease.Linear).OnComplete(() => knife.gameObject.SetActive(false));

                    //breadBase.DOMove(new Vector3(0, 1.5f, -0.25f), 0.5f).SetEase(Ease.Linear).SetDelay(0.5f);
                    //bread.DOJump(new Vector3(0, 1.2f, 0.25f), 0.3f, 1, 1).SetEase(Ease.Linear).SetDelay(0.5f);
                    bread.DORotate(new Vector3(180, 0, 0), 1).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        cheeseNToastPrompt.SetActive(true);
                        //ingredientSelection.SetActive(true);
                        //DOTween.Kill(camTween);
                        //cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                        //cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                    });

                });
                knife.DOMoveZ(-0.4f, 0.2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId(2);
            }
            if (squeeze)
            {
                DOTween.To(() => squeezeVal, x => squeezeVal = x, 1, (1 - squeezeVal) * 3f).SetId(3).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (sauceCount < 3)
                        sauceSelection.SetActive(true);
                    squeeze = false;
                    squeezeTut.SetActive(false);
                    nextButton.SetActive(true);
                    bottlePosition.spline = null;
                    bottlePosition.enabled = false;
                    sauceBottle.gameObject.SetActive(false);
                });
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (cut)
            {
                DOTween.Kill(2);
            }
            if (squeeze)
            {
                DOTween.Kill(3);
            }
        }
        if (squeeze)
        {
            bottlePosition.SetPercent(squeezeVal);
            currentSauce.SetClipRange(0, squeezeVal);
        }
    }
    public void PlayLevel()
    {
        PlaySound(soundTypes.tap);
        SupersonicWisdom.Api.NotifyLevelStarted(ESwLevelType.Regular, currentLevel, null);

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
        if (sauceadded)
        {
            PlaySound(soundTypes.win);

            nextButton.SetActive(false);
            cam.transform.GetChild(0).gameObject.SetActive(true);
            sauceSelection.SetActive(false);
            breadBase.DOMove(new Vector3(0, 1.5f, 0), 0.5f).SetEase(Ease.Linear).SetDelay(0.5f);
            bread.parent = null;
            breadBase.parent = null;
            bread.DOMoveY(dropHeight, 1).SetEase(Ease.Linear);
            bread.DORotate(Vector3.zero, 1).From(Vector3.right * 180).SetEase(Ease.Linear).OnComplete(() =>
            {

                cashPanel.SetActive(true);
                gamePanel.SetActive(false);
                bread.parent = breadBase;
                breadBase.DOJump(breadBase.position, 0.2f, 1, 0.5f).SetEase(Ease.Linear).SetDelay(0.5f);
                paper.DOMoveZ(-3, 0.2f).SetEase(Ease.Linear);
                tray.SetActive(true);

                DOTween.Kill(camTween);
                cam.transform.DOMove(camPositions.GetChild(4).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                cam.transform.DORotate(camPositions.GetChild(4).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                StartCoroutine(LevelComplete());
            });
        }
        else
        {
            sauceadded = true;
            ingredientSelection.SetActive(false);
            sauceSelection.SetActive(true);
        }
    }

    IEnumerator LevelComplete()
    {
        SupersonicWisdom.Api.NotifyLevelCompleted(ESwLevelType.Regular, currentLevel, null);

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
        if (ingredientsMatched)
            MatchCount++;
        if (breadType == currentOrder.breads)
            MatchCount++;
        if (ingredientLayers.Count == currentOrder.ingredients.Count)
            MatchCount++;
        if (sauces.Count == currentOrder.sauces.Count)
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
        sampleFullButton.SetActive(false);
        sampleButton.SetActive(false);
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

        whiteScreen.GetComponent<Image>().DOFade(1, 0.5f).SetEase(Ease.Linear).OnComplete(() => SceneManager.LoadScene("Game"));
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
                bread = Instantiate(italianBread, new Vector3(0, 2f, 0.18f), Quaternion.identity);
                break;
            case "sesame":
                breadType = Breads.sesame;
                bread = Instantiate(sesameBread, new Vector3(0, 2f, 0.18f), Quaternion.identity);
                break;
            case "grill":
                breadType = Breads.grill;
                bread = Instantiate(GrillBread, new Vector3(0, 2f, 0.18f), Quaternion.identity);
                break;
            case "oats":
                breadType = Breads.oats;
                bread = Instantiate(oatsBread, new Vector3(0, 2f, 0.18f), Quaternion.identity);
                break;
        }
        breadSelection.SetActive(false);
        //DOTween.Kill(camTween);
        //cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        //cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);

        paper.gameObject.SetActive(true);
        paper.DOMove(new Vector3(0, 1.51f, 0.25f), 0.3f).SetEase(Ease.Linear);
        breadBase = Instantiate(breadBottom, new Vector3(0, 1.9f, 0), Quaternion.identity);
        breadBase.DOMove(new Vector3(0, 1.5f, 0), 0.5f).SetEase(Ease.Linear);
        bread.DOMove(new Vector3(0, 1.6f, 0.18f), 0.5f).SetEase(Ease.Linear);

        cutTut.SetActive(true);
        knife.gameObject.SetActive(true);
        knife.DOMove(new Vector3(0.6f, 1.6f, -0.5f), 0.5f).From(new Vector3(3, 1.6f, -2)).SetEase(Ease.Linear).OnComplete(() => cut = true);
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
                cheeseNToastPrompt.SetActive(false);
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
        //DOTween.Kill(camTween);
        //cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        //cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
    }
    public void SauceSelect(string name)
    {
        PlaySound(soundTypes.tap);

        nextButton.SetActive(false);
        sauceCount++;
        squeezeTut.SetActive(true);
        switch (name)
        {
            case "chilli":
                sauceContainer.GetChild(0).GetComponent<Button>().interactable = false;
                sauceType = Sauce.chilli;
                sauces.Add(Sauce.chilli);
                currentSauce = Instantiate(saucesObj[0], new Vector3(breadBase.position.x, dropHeight - 0.1f, breadBase.position.z), Quaternion.identity).GetComponent<TubeGenerator>();
                currentSauce.transform.parent = breadBase;
                sauceBottle.gameObject.SetActive(true);
                sauceBottle.GetComponent<MeshRenderer>().material.color = Color.red;
                sauceBottle.DOMove(currentSauce.transform.position + Vector3.right * -0.5f, 0.5f).From(new Vector3(-3, 1, 0)).SetEase(Ease.Linear).OnComplete(() =>
                {
                    bottlePosition.spline = currentSauce.GetComponent<SplineComputer>();
                    bottlePosition.enabled = true;
                    squeeze = true;
                    squeezeVal = 0;
                    currentSauce.gameObject.SetActive(true);
                });

                break;

            case "mayo":
                sauceContainer.GetChild(1).GetComponent<Button>().interactable = false;
                sauceType = Sauce.mayo;
                sauces.Add(Sauce.mayo);
                currentSauce = Instantiate(saucesObj[1], new Vector3(breadBase.position.x, dropHeight - 0.1f, breadBase.position.z + 0.1f), Quaternion.identity).GetComponent<TubeGenerator>();
                currentSauce.transform.parent = breadBase;
                sauceBottle.gameObject.SetActive(true);
                sauceBottle.GetComponent<MeshRenderer>().material.color = Color.white;
                sauceBottle.DOMove(currentSauce.transform.position + Vector3.right * -0.5f, 0.5f).From(new Vector3(-3, 1, 0)).SetEase(Ease.Linear).OnComplete(() =>
                {
                    bottlePosition.spline = currentSauce.GetComponent<SplineComputer>();
                    bottlePosition.enabled = true;
                    squeeze = true;
                    squeezeVal = 0;
                    currentSauce.gameObject.SetActive(true);
                });
                break;

            case "mustard":
                sauceContainer.GetChild(2).GetComponent<Button>().interactable = false;
                sauceType = Sauce.mustard;
                sauces.Add(Sauce.mustard);
                currentSauce = Instantiate(saucesObj[2], new Vector3(breadBase.position.x, dropHeight - 0.1f, breadBase.position.z - 0.1f), Quaternion.identity).GetComponent<TubeGenerator>();
                currentSauce.transform.parent = breadBase;
                sauceBottle.gameObject.SetActive(true);
                sauceBottle.GetComponent<MeshRenderer>().material.color = Color.yellow;
                sauceBottle.DOMove(currentSauce.transform.position + Vector3.right * -0.5f, 0.5f).From(new Vector3(-3, 1, 0)).SetEase(Ease.Linear).OnComplete(() =>
                {
                    bottlePosition.spline = currentSauce.GetComponent<SplineComputer>();
                    bottlePosition.enabled = true;
                    squeeze = true;
                    squeezeVal = 0;
                    currentSauce.gameObject.SetActive(true);
                });
                break;
        }
        sauceSelection.SetActive(false);
        //DOTween.Kill(camTween);
        //cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        //cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
    }
    public void Toast()
    {
        breadBase.parent = paper;
        bread.parent = paper;
        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(6).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(6).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() =>
        {
            ovenDoor.DOLocalRotate(new Vector3(90, 0, 0), 0.5f).SetEase(Ease.Linear);
            paper.DOMoveY(2f, 0.3f).SetEase(Ease.Linear);
            paper.DOMove(new Vector3(-4.3f, 2.1f, 0), 0.3f).SetDelay(0.5f).SetEase(Ease.Linear);
            paper.DORotate(new Vector3(0, -90, 0), 0.3f).SetEase(Ease.Linear);
            ovenDoor.DOLocalRotate(new Vector3(0, 0, 0), 0.5f).SetDelay(1).SetEase(Ease.Linear);
            bakeFill.DOFillAmount(1, 2).SetEase(Ease.Linear).SetDelay(1.5f).OnComplete(() =>
            {
                ovenDoor.DOLocalRotate(new Vector3(90, 0, 0), 0.5f).SetEase(Ease.Linear);
                paper.DORotate(new Vector3(0, 0, 0), 0.3f).SetDelay(0.6f).SetEase(Ease.Linear);
                paper.DOMoveY(1.51f, 0.3f).SetDelay(1f).SetEase(Ease.Linear);
                paper.DOMove(new Vector3(0, 2.5f, 0.25f), 0.5f).SetDelay(0.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    DOTween.Kill(camTween);
                    cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                    cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() => ingredientSelection.SetActive(true));
                });
            });
        });
    }
    public void SkipCheeseNtoast()
    {
        cheeseNToastPrompt.SetActive(false);
        ingredientSelection.SetActive(true);
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
            if (ingredientType == Ingredients.cheese)
            {
                Toast();
            }
            else
            {
                if (ingredientLayers.Count > ingredientContainer.childCount - 1)
                    Next();
                else
                {
                    ingredientSelection.SetActive(true);
                    //DOTween.Kill(camTween);
                    //cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                    //cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() => ingredientSelection.SetActive(true));
                }
                if (ingredientLayers.Count > currentOrder.ingredients.Count - 1)
                    nextButton.SetActive(true);
            }
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

        SampleFullScreen(true);
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

                if (j == 0)
                {
                    tempParent = Instantiate(temp, new Vector3(rows.GetChild(nodes).GetChild(j).position.x, sampleHeight, 10), temp.rotation);
                    sampleLayers.Add(tempParent);
                }
                else
                    Instantiate(temp, new Vector3(rows.GetChild(nodes).GetChild(j).position.x, sampleHeight, 10), temp.rotation, tempParent);
            }
            //temp.parent = breadBase;
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
        SampleFullScreen(false);

        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() =>
        {
            breadSelection.SetActive(true);
        });

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

            sampleFullButton.SetActive(true);
            sampleButton.SetActive(false);
            sampleFullButton.transform.DOScale(1, 0.2f).SetEase(Ease.Linear);
        }
        else
        {
            sampleButton.SetActive(true);
            sampleFullButton.transform.DOScale(0, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                sampleFullButton.SetActive(false);
            });
        }

    }
}
[System.Serializable]
public class Level
{
    public GameManager.Breads breads;
    public bool cheeseNtoast;
    public List<GameManager.Ingredients> ingredients = new List<GameManager.Ingredients>();
    public List<GameManager.Sauce> sauces = new List<GameManager.Sauce>();
}