using UnityEngine;

public class Progetto4Manipulator : MonoBehaviour
{
    [Header("Impostazioni")]
    public float rotationSpeed = 5f;
    public float zoomSpeed = 0.5f;

    // Memoria per il Reset
    private Quaternion rotazioneIniziale;
    private Vector3 scalaIniziale;
    private float lastTapTime = 0;

    void Start()
    {
        // Memorizza la posizione iniziale
        rotazioneIniziale = transform.localRotation;
        scalaIniziale = transform.localScale;
    }

    void Update()
    {
        // 1. Rotazione (Space.World per girare dritto come un piatto)
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;
            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }

        // 2. Zoom Mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) transform.localScale += Vector3.one * scroll * zoomSpeed;

        // 3. Zoom Touch (Due dita)
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
            float prevMag = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
            float curMag = (t0.position - t1.position).magnitude;
            transform.localScale += Vector3.one * (curMag - prevMag) * zoomSpeed * 0.01f;
        }

        // 4. Reset (Doppio Click)
        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - lastTapTime < 0.3f)
            {
                // Resetta posizione e scala
                transform.localRotation = rotazioneIniziale;
                transform.localScale = scalaIniziale;
            }
            lastTapTime = Time.time;
        }
    }
}