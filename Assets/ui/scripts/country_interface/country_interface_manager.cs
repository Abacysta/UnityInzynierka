using UnityEngine;
using UnityEngine.UI;

public class country_interface_manager : MonoBehaviour
{
    [SerializeField] Map map;

    [SerializeField] private Image cn_in_country_color_img;
    [SerializeField] private GameObject[] tab_buttons;
    [SerializeField] private GameObject[] tab_panels;
    [SerializeField] private GameObject[] subwindows;

    [SerializeField] Color activeColor = new Color32(92, 92, 92, 255);
    [SerializeField] Color inactiveColor = new Color32(77, 77, 77, 255);

    private int activeTab = 0;

    public int ActiveTab { get => activeTab; set => activeTab = value; }

    void Awake()
    {
        gameObject.SetActive(false);
        foreach (var panel in tab_panels) 
        { 
            panel.SetActive(false);
        }
    }

    void Start()
    {
        ActivateTab(activeTab);
        foreach(var window in subwindows) {
            window.SetActive(false);
        }
        for (int i = 0; i < tab_buttons.Length; i++)
        {
            int index = i;
            tab_buttons[i].GetComponent<Button>().onClick.AddListener(() => ActivateTab(index));
        }
    }

    private void OnEnable()
    {
        SetCoatOfArmsColor();
    }

    public void ActivateTab(int index)
    {
        for (int i = 0; i < tab_buttons.Length; i++)
        {
            tab_panels[i].SetActive(false);
            tab_buttons[i].GetComponent<Image>().color = inactiveColor;
        }

        tab_panels[index].SetActive(true);
        tab_buttons[index].GetComponent<Image>().color = activeColor;

        activeTab = index;
    }

    public void SetCoatOfArmsColor()
    {
        cn_in_country_color_img.color = map.CurrentPlayer.Color;
    }

    public void HideCountryInterface()
    {
        gameObject.SetActive(false);
    }

    public void ShowCountryInterface()
    {
        gameObject.SetActive(true);
    }
}