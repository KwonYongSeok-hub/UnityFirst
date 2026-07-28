using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 기획서의 UI 레이아웃(좌상단 미니맵/재화, 우상단 잠식게이지/부적,
/// 좌하단 스킬, 우하단 장비 5슬롯, 하단중앙 체력/기력/마나, 상단중앙 보스체력바)을
/// 런타임에 자동으로 생성하는 스크립트.
///
/// 빈 GameObject 하나에 이 스크립트만 붙이면 Canvas부터 전부 알아서 생성됨.
/// 실제 시스템(스태미나/마나/잠식게이지/장비/스킬)이 만들어지면
/// 아래 public 메서드들을 호출해서 값만 갱신하면 됨.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("연결 (비워두면 자동 탐색: Player 태그)")]
    [SerializeField] private Health playerHealth;

    // 외부(스태미나/마나/장비/스킬 시스템)에서 참조할 수 있도록 보관
    private Image healthFill;
    private Image staminaFill;
    private Image manaFill;
    private Image corruptionFill;
    private GameObject charmIcon;
    private GameObject bossHealthBarRoot;
    private Image bossHealthFill;
    private Text currencyText;
    private Image[] equipmentSlots = new Image[5];
    private Image[] skillSlots = new Image[4];

    private void Awake()
    {
        Canvas canvas = BuildCanvas();
        BuildTopLeft(canvas.transform);      // 미니맵 + 재화
        BuildTopRight(canvas.transform);     // 잠식게이지 + 부적
        BuildTopCenter(canvas.transform);    // 보스 체력바 (기본 숨김)
        BuildBottomLeft(canvas.transform);   // 스킬 목록
        BuildBottomRight(canvas.transform);  // 장비 5슬롯
        BuildBottomCenter(canvas.transform); // 체력/기력/마나

        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerHealth = playerObj.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += (current, max) => healthFill.fillAmount = max > 0 ? current / max : 0f;
            healthFill.fillAmount = playerHealth.MaxHealth > 0 ? playerHealth.CurrentHealth / playerHealth.MaxHealth : 1f;
        }
    }

    // ---------- 외부에서 호출할 공개 API (나중에 시스템 붙일 때 사용) ----------

    public void SetStaminaRatio(float ratio) => staminaFill.fillAmount = Mathf.Clamp01(ratio);
    public void SetManaRatio(float ratio) => manaFill.fillAmount = Mathf.Clamp01(ratio);
    public void SetCorruptionRatio(float ratio) => corruptionFill.fillAmount = Mathf.Clamp01(ratio);
    public void SetHasCharm(bool hasCharm) => charmIcon.SetActive(hasCharm);
    public void SetCurrency(int amount) => currencyText.text = amount.ToString();

    public void ShowBossHealthBar(bool show) => bossHealthBarRoot.SetActive(show);
    public void SetBossHealthRatio(float ratio) => bossHealthFill.fillAmount = Mathf.Clamp01(ratio);

    public void SetEquipmentIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex < 0 || slotIndex >= equipmentSlots.Length) return;
        equipmentSlots[slotIndex].sprite = icon;
        equipmentSlots[slotIndex].enabled = icon != null;
    }

    public void SetSkillIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length) return;
        skillSlots[slotIndex].sprite = icon;
        skillSlots[slotIndex].enabled = icon != null;
    }

    // ---------- 내부 생성 로직 ----------

    private Canvas BuildCanvas()
    {
        GameObject canvasObj = new GameObject("HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        return canvas;
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        RectTransform rt = CreatePanel(name, parent, anchorMin, anchorMax, anchoredPos, sizeDelta);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, string content, int fontSize)
    {
        RectTransform rt = CreatePanel(name, parent, anchorMin, anchorMax, anchoredPos, sizeDelta);
        Text text = rt.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        return text;
    }

    // 좌상단: 미니맵 + 재화
    private void BuildTopLeft(Transform parent)
    {
        CreateImage("Minimap_Placeholder", parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, -90), new Vector2(160, 160), new Color(0.15f, 0.15f, 0.15f, 0.8f));
        currencyText = CreateText("CurrencyText", parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, -180), new Vector2(160, 30), "0 G", 20);
        currencyText.alignment = TextAnchor.MiddleCenter;
    }

    // 우상단: 잠식게이지 + 부적 보유 여부
    private void BuildTopRight(Transform parent)
    {
        CreateImage("CorruptionGauge_BG", parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -30), new Vector2(180, 20), new Color(0.1f, 0.1f, 0.1f, 0.8f));
        corruptionFill = CreateImage("CorruptionGauge_Fill", parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-110, -30), new Vector2(180, 20), new Color(0.6f, 0.1f, 0.8f));
        corruptionFill.type = Image.Type.Filled;
        corruptionFill.fillMethod = Image.FillMethod.Horizontal;
        corruptionFill.fillAmount = 0f;

        charmIcon = CreateImage("CharmIcon", parent, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -70), new Vector2(30, 30), Color.yellow).gameObject;
        charmIcon.SetActive(false); // 기본은 부적 없음
    }

    // 상단중앙: 보스 체력바 (평소엔 숨김)
    private void BuildTopCenter(Transform parent)
    {
        bossHealthBarRoot = new GameObject("BossHealthBar_Root", typeof(RectTransform));
        bossHealthBarRoot.transform.SetParent(parent, false);
        RectTransform rt = bossHealthBarRoot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -40);
        rt.sizeDelta = new Vector2(500, 30);

        CreateImage("BossHP_BG", bossHealthBarRoot.transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0.1f, 0.1f, 0.1f, 0.85f));
        bossHealthFill = CreateImage("BossHP_Fill", bossHealthBarRoot.transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0.8f, 0.1f, 0.1f));
        bossHealthFill.type = Image.Type.Filled;
        bossHealthFill.fillMethod = Image.FillMethod.Horizontal;
        bossHealthFill.fillAmount = 1f;

        bossHealthBarRoot.SetActive(false);
    }

    // 좌하단: 스킬 목록 (4슬롯)
    private void BuildBottomLeft(Transform parent)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            Vector2 pos = new Vector2(30 + i * 60, 30);
            skillSlots[i] = CreateImage($"SkillSlot_{i}", parent, new Vector2(0, 0), new Vector2(0, 0), pos, new Vector2(50, 50), new Color(0.2f, 0.2f, 0.2f, 0.85f));
        }
    }

    // 우하단: 장비 5슬롯 (주무기/보조무기/장신구1/장신구2/소모품)
    private void BuildBottomRight(Transform parent)
    {
        string[] labels = { "주무기", "보조무기", "장신구1", "장신구2", "소모품" };
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            Vector2 pos = new Vector2(-260 + i * 60, 30);
            equipmentSlots[i] = CreateImage($"EquipSlot_{labels[i]}", parent, new Vector2(1, 0), new Vector2(1, 0), pos, new Vector2(50, 50), new Color(0.2f, 0.2f, 0.2f, 0.85f));
        }
    }

    // 하단중앙: 체력 / 기력 / 마나
    private void BuildBottomCenter(Transform parent)
    {
        CreateImage("HP_BG", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(300, 18), new Color(0.1f, 0.1f, 0.1f, 0.85f));
        healthFill = CreateImage("HP_Fill", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(300, 18), new Color(0.8f, 0.1f, 0.1f));
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillAmount = 1f;

        CreateImage("Stamina_BG", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 48), new Vector2(300, 12), new Color(0.1f, 0.1f, 0.1f, 0.85f));
        staminaFill = CreateImage("Stamina_Fill", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 48), new Vector2(300, 12), new Color(0.8f, 0.7f, 0.1f));
        staminaFill.type = Image.Type.Filled;
        staminaFill.fillMethod = Image.FillMethod.Horizontal;
        staminaFill.fillAmount = 1f;

        CreateImage("Mana_BG", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(300, 12), new Color(0.1f, 0.1f, 0.1f, 0.85f));
        manaFill = CreateImage("Mana_Fill", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(300, 12), new Color(0.1f, 0.3f, 0.9f));
        manaFill.type = Image.Type.Filled;
        manaFill.fillMethod = Image.FillMethod.Horizontal;
        manaFill.fillAmount = 1f;
    }
}
