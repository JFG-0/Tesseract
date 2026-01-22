using UnityEngine;

public class GelatieraManipulator : MonoBehaviour
{
    [Header("Impostazioni Movimento")]
    public float SpeedRotation = 0.4f; 
    public float SpeedZoom = 0.5f;

    [Header("Limiti Zoom")]
    public float MinScale = 0.5f;
    public float MaxScale = 3.0f;

    [Header("Collegamento Animazione")]
    public Animator AnimatorGelatiera; 
    public string NomeParametroAnimazione = "Attiva";

    // Variabili interne
    private float lastTapTime = 0;
    private float tapThreshold = 0.3f;
    private Quaternion rotazioneIniziale;

    void Start()
    {
        // Memorizza la posizione "Dritta" all'avvio
        rotazioneIniziale = transform.localRotation;
    }

    void Update()
    {
        // --- GESTIONE EDITOR (MOUSE) ---
        #if UNITY_EDITOR || UNITY_STANDALONE
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f) ZoomOggetto(scroll * SpeedZoom * 2f);

        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * SpeedRotation * 5f;
            float rotY = Input.GetAxis("Mouse Y") * SpeedRotation * 5f;

            // ROTAZIONE LIBERA (Sfera)
            transform.Rotate(rotY, -rotX, 0, Space.World);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - lastTapTime < tapThreshold) TriggerAnimazione();
            lastTapTime = Time.time;
        }
        #endif

        // --- GESTIONE TOUCH (TELEFONO) ---
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 2) // Zoom
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                float prevMag = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
                float curMag = (t0.position - t1.position).magnitude;
                ZoomOggetto(-(prevMag - curMag) * SpeedZoom * 0.01f);
            }
            else if (Input.touchCount == 1) // Rotazione
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    // ROTAZIONE LIBERA (Sfera)
                    transform.Rotate(t.deltaPosition.y * SpeedRotation, -t.deltaPosition.x * SpeedRotation, 0f, Space.World);
                }
                if (t.phase == TouchPhase.Ended)
                {
                    if (Time.time - lastTapTime < tapThreshold) TriggerAnimazione();
                    lastTapTime = Time.time;
                }
            }
        }
    }

    void ZoomOggetto(float incremento)
    {
        float currentScale = transform.localScale.x;
        float newScale = Mathf.Clamp(currentScale + incremento, MinScale, MaxScale);
        transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    void TriggerAnimazione()
    {
        // 1. RESETTA DRITTO
        transform.localRotation = rotazioneIniziale;

        // 2. FAI PARTIRE ANIMAZIONE (ON/OFF)
        if (AnimatorGelatiera != null)
        {
            bool statoAttuale = AnimatorGelatiera.GetBool(NomeParametroAnimazione);
            AnimatorGelatiera.SetBool(NomeParametroAnimazione, !statoAttuale);
        }
    }
}