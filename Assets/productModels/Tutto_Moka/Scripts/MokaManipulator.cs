using UnityEngine;

public class MokaManipulator : MonoBehaviour
{
    [Header("Velocità")]
    public float rotationSpeed = 5f; // Abbassata per più controllo
    public float zoomSpeed = 0.5f;

    // Memoria per il Reset
    private Quaternion rotazioneIniziale;
    private Vector3 scalaIniziale;

    // Variabili doppio tocco
    private float lastTapTime = 0;
    private float tapThreshold = 0.3f;

    void Start()
    {
        // Memorizza come è messa la Moka appena parte il gioco
        rotazioneIniziale = transform.localRotation;
        scalaIniziale = transform.localScale;
    }

    void Update()
    {
        // 1. ROTAZIONE (Mouse o Dito)
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // FIX: Usa Space.World per ruotare in modo naturale (come un piatto girevole)
            // Inverte rotX per seguire il dito
            transform.Rotate(Vector3.up, -rotX, Space.World); 
            transform.Rotate(Vector3.right, rotY, Space.World);
        }

        // 2. ZOOM (Rotellina Mouse)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            transform.localScale += Vector3.one * scroll * zoomSpeed;
        }

        // 3. ZOOM TOUCH (Pinch su telefono)
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float prevMag = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
            float curMag = (t0.position - t1.position).magnitude;
            transform.localScale += Vector3.one * (curMag - prevMag) * zoomSpeed * 0.01f;
        }

        // 4. RESET (Doppio Tocco / Doppio Click)
        // Rileva quando alzi il dito/mouse
        if (Input.GetMouseButtonUp(0)) 
        {
            if (Time.time - lastTapTime < tapThreshold)
            {
                ResetMoka();
            }
            lastTapTime = Time.time;
        }
    }

    void ResetMoka()
    {
        // Torna esattamente com'era all'inizio
        transform.localRotation = rotazioneIniziale;
        transform.localScale = scalaIniziale;
    }
}