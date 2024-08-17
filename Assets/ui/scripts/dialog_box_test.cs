using UnityEngine;
using UnityEngine.UI;

//public class dialog_box_test : MonoBehaviour
//{
//    [SerializeField] dialog_box_manager dialog_box_manager;
//    [SerializeField] private Slider dialog_slider;

//    public void TestUpgradeTechnology()
//    {
//        dialog_slider.onValueChanged.RemoveAllListeners();

//        string imageName = "gauls";

//        System.Action onCancel = () => {
//            Debug.Log("Upgrading the technology canceled.");
//        };

//        System.Action onConfirm = UpgradeTechnology;

//        dialog_box_manager.ShowUpgradeTechnologyBox(imageName, onConfirm, onCancel);
//        SetTechnologyCost();
//    }

//    public void TestMoveArmy()
//    {
//        dialog_slider.onValueChanged.RemoveAllListeners();

//        string imageName = "gauls";
//        int recruitable_population = 300;

//        System.Action onCancel = () => {
//            Debug.Log("Moving the army canceled.");
//        };

//        System.Action onConfirm = MoveArmy;

//        dialog_box_manager.ShowMoveArmyBox(imageName, onConfirm, onCancel, recruitable_population);
//        SetMovingArmyCost();
//    }

//    public void TestRecruitArmy()
//    {
//        dialog_slider.onValueChanged.RemoveAllListeners();

//        string imageName = "gauls";
//        int recruitable_population = 300;

//        System.Action onCancel = () => {
//            Debug.Log("Recruting the army canceled.");
//        };

//        System.Action onConfirm = RecruitArmy;

//        dialog_box_manager.ShowRecruitArmyBox(imageName, onConfirm, onCancel, recruitable_population);
//        UpdateRecruitingArmyCost();
//        dialog_slider.onValueChanged.AddListener(delegate { UpdateRecruitingArmyCost(); });
//    }

//    private void SetTechnologyCost()
//    {
//        float sciencePoints = 5f;
//        float actionPoints = 1f;

//        string cost = "Science points: " + sciencePoints;
//        cost += "\nAction points: " + actionPoints;
//        dialog_box_manager.SetCost(cost);
//    }

//    private void SetMovingArmyCost()
//    {
//        float actionPoints = 2f;

//        string cost = "Action points: " + actionPoints;
//        dialog_box_manager.SetCost(cost);
//    }

//    private void UpdateRecruitingArmyCost()
//    {
//        float actionPoints = 1f;
//        float gold = 10f;

//        int sliderValue = Mathf.RoundToInt(dialog_slider.value);
//        gold *= sliderValue;
//        string cost = "Gold: " + gold;
//        cost += "\nAction points: " + actionPoints;
//        dialog_box_manager.SetCost(cost);
//    }

//    private void MoveArmy()
//    {
//        Debug.Log($"Moved the army ({dialog_slider.value} soldiers)");
//    }

//    private void RecruitArmy()
//    {
//        Debug.Log($"Recruited the army ({dialog_slider.value} soldiers)");
//    }

//    private void UpgradeTechnology()
//    {
//        Debug.Log("Upgraded the technology");
//    }
//}
