using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class TextShowing : MonoBehaviour
{
    public GameObject WinText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WinText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            WinText.SetActive(true);
            
    

            Destroy(gameObject);
        }
    }
}
