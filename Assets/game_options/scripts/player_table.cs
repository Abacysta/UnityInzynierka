using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_table : MonoBehaviour
{
    [SerializeField] private GameObject dummy;
    private List<Map.CountryController> controllers = new List<Map.CountryController>();
    private List<Country> countries = new List<Country>();

    public List<Map.CountryController> Controllers { get => controllers; set => controllers = value; }
    public List<Country> Countries { get => countries; set => countries = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showCountries(List<Country> countries) {
        
        foreach (Country country in countries) { 
                  
        }
    }
}
