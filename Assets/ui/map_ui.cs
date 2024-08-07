using UnityEngine;

public class map_ui : MonoBehaviour
{
    [SerializeField] private settings_menu settings_menu_scr;
    [SerializeField] private GameObject settings_menu_ui;
    [SerializeField] private GameObject dialog_box;

    void Start()
    {
        
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!dialog_box.activeSelf)
            {
                settings_menu_scr.toggleMenu(settings_menu_ui);
            } 
        }
    }
}
