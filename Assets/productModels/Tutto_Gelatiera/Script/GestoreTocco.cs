using UnityEngine;

public class GestoreTocco : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Questo IF controlla sia il CLICK del mouse (PC) sia il TOUCH (Telefono)
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            // Spara il raggio per vedere cosa tocchi
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.touchCount > 0) ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Se tocchi l'oggetto che ha questo script...
                if (hit.transform == transform)
                {
                    // ...attiva il Trigger. 
                    // Dato che le frecce sono collegate in cerchio,
                    // al primo click va avanti, al secondo torna indietro.
                    anim.SetTrigger("Attiva");
                }
            }
        }
    }
}