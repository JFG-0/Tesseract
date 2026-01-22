using UnityEngine;

public class OctoManipulator : MonoBehaviour
{
    public Animator AnimatorOcto; // Trascina qui il FIGLIO
    public string NomeParametro = "Attiva";
    
    // Velocità movimenti
    public float rotationSpeed = 10f;
    public float zoomSpeed = 0.5f;
    
    // Variabili per il doppio tocco
    private float lastTapTime = 0;
    private float tapThreshold = 0.3f; // Tempo massimo tra due click

    void Update()
    {
        // 1. ROTAZIONE (Un dito o Click sinistro)
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;
            
            // Ruota l'oggetto su se stesso (non nel mondo)
            transform.Rotate(Vector3.up, -rotX, Space.Self);
            transform.Rotate(Vector3.right, rotY, Space.Self);
        }

        // 2. ZOOM (Rotellina Mouse)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            transform.localScale += Vector3.one * scroll * zoomSpeed;
        }

        // 3. DOPPIO TOCCO (Logica On/Off)
        if (Input.GetMouseButtonUp(0)) // Rileva quando alzi il dito
        {
            if (Time.time - lastTapTime < tapThreshold)
            {
                // DOPPIO CLICK RILEVATO
                ToggleAnimation();
            }
            lastTapTime = Time.time;
        }
    }

    void ToggleAnimation()
    {
        if (AnimatorOcto != null)
        {
            // Legge lo stato attuale (Vero o Falso) e lo inverte
            bool statoAttuale = AnimatorOcto.GetBool(NomeParametro);
            AnimatorOcto.SetBool(NomeParametro, !statoAttuale);
        }
    }
}