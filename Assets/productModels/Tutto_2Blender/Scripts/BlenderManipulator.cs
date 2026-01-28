using UnityEngine;

public class BlenderManipulator : MonoBehaviour
{
    public Animator AnimatorBlender; 
    public string NomeParametro = "Attiva";
    public float rotationSpeed = 5f;
    public float zoomSpeed = 0.5f;

    private Quaternion rotazioneIniziale;
    private Vector3 scalaIniziale;
    private float lastTapTime = 0;

    void Start()
    {
        rotazioneIniziale = transform.localRotation;
        scalaIniziale = transform.localScale;
    }

    void Update()
    {
        // Rotazione
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;
            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) transform.localScale += Vector3.one * scroll * zoomSpeed;

        // Touch Zoom
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
            float prevMag = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
            float curMag = (t0.position - t1.position).magnitude;
            transform.localScale += Vector3.one * (curMag - prevMag) * zoomSpeed * 0.01f;
        }

        // Doppio Click
        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - lastTapTime < 0.3f)
            {
                // Reset
                transform.localRotation = rotazioneIniziale;
                // Animazione
                if (AnimatorBlender != null)
                {
                    bool stato = AnimatorBlender.GetBool(NomeParametro);
                    AnimatorBlender.SetBool(NomeParametro, !stato);
                }
            }
            lastTapTime = Time.time;
        }
    }
}