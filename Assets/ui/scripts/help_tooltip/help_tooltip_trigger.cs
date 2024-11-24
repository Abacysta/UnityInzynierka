using UnityEngine;
using UnityEngine.EventSystems;

public class help_tooltip_trigger : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string tooltipText;
    private help_tooltip helpTooltipScript;

    public string TooltipText { get => tooltipText; set => tooltipText = value; }

    void Start()
    {
        GameObject helpTooltipObject = GameObject.Find("help_tooltip");

        if (helpTooltipObject != null)
        {
            helpTooltipScript = helpTooltipObject.GetComponent<help_tooltip>();
        }
    }

    private void OnEnable()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>() ?? gameObject.AddComponent<EventTrigger>();

        if (!eventTrigger.triggers.Exists(entry => entry.eventID == EventTriggerType.PointerEnter))
        {
            EventTrigger.Entry entryEnter = new()
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((eventData) => OnPointerEnter());
            eventTrigger.triggers.Add(entryEnter);
        }

        if (!eventTrigger.triggers.Exists(entry => entry.eventID == EventTriggerType.PointerExit))
        {
            EventTrigger.Entry entryExit = new()
            {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((eventData) => OnPointerExit());
            eventTrigger.triggers.Add(entryExit);
        }
    }

    private void OnPointerEnter()
    {
        if (helpTooltipScript != null)
        {
            helpTooltipScript.Info = tooltipText;
            helpTooltipScript.OnMouseEnterElement();
        }

    }

    private void OnPointerExit()
    {
        if (helpTooltipScript != null)
        {
            helpTooltipScript.Info = tooltipText;
            helpTooltipScript.OnMouseExitElement();
        }
    }
}
