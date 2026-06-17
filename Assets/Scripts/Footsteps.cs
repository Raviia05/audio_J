using FMODUnity;
using UnityEngine;


/// <summary>
/// Zarządza odtwarzaniem dźwięków kroków, skoków i lądowania w zależności od powierzchni.
/// </summary>
public class Footsteps : MonoBehaviour
{
    // Publiczne referencje do zdarzeń FMOD.
    public EventReference footstepsEvent;
    public EventReference jumpEvent;
    public EventReference landEvent;

    // Nazwy parametrów w FMOD Studio (ustaw takie, jakie masz w programie!)
    [Header("FMOD Parameter Names")]
    [SerializeField] private string footstepsParamName = "Surface";
    [SerializeField] private string jumpParamName = "Surface3"; 
    [SerializeField] private string landParamName = "Surface2";    

    private float lastFootstepTime = 0f;
    private float distToGround = 0.2f;

    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isJumping = false;
    private float jumpCooldownTimer = 0f;

    private CharacterController controller;

    void Start()
    {
        // Sprawdzamy wysokość collidera (lub domyślnie bierzemy 1f dla CharacterControllera)
        if (GetComponent<Collider>() != null)
        {
            distToGround = GetComponent<Collider>().bounds.extents.y;
        }
        else
        {
            distToGround = 1f;
        }

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Sprawdzanie czy jesteśmy na ziemi (przypisanie do zmiennej widocznej w Inspectorze)
        isGrounded = IsGrounded();

        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        // Skok: Spacja + jesteśmy na ziemi + brak cooldownu
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && jumpCooldownTimer <= 0f)
        {
            PlayJump();
            jumpCooldownTimer = 0.5f;
            isJumping = true;
        }

        HandleFootsteps();
    }

    /// <summary>
    /// Obsługuje logikę timera kroków.
    /// </summary>
    private void HandleFootsteps()
    {
        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && isGrounded;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isMoving)
        {
            float footstepInterval = isRunning ? 0.25f : 0.5f;

            if (Time.time - lastFootstepTime > footstepInterval)
            {
                lastFootstepTime = Time.time;
                PlayFootsteps();
            }
        }
    }

    /// <summary>
    /// Odtwarza dźwięk kroków.
    /// </summary>
    private void PlayFootsteps()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
        {
            string surfaceTag = hit.collider.tag;
            // KROKI: Używamy metody z instancją i parametrem kroków
            PlaySurfaceSound(footstepsEvent, footstepsParamName, surfaceTag);
        }
    }

    /// <summary>
    /// Odtwarza dźwięk skoku.
    /// </summary>
    private void PlayJump()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
        {
            string surfaceTag = hit.collider.tag;
            // SKOK: Używamy parametru dla skoku
            PlaySurfaceSound(jumpEvent, jumpParamName, surfaceTag);
        }
    }

    /// <summary>
    /// Wykrywanie lądowania przez fizykę (Trigger/Collision lub CharacterController).
    /// </summary>
    private void OnCollisionEnter(Collision col)
    {
        if (isJumping)
        {
            PlayLanding();
        }
    }

    // Jeśli używasz CharacterControllera zamiast Rigidbody, odkomentuj to poniżej:
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isJumping && controller.isGrounded)
        {
            PlayLanding();
        }
    }

    /// <summary>
    /// Odtwarza dźwięk lądowania.
    /// </summary>
    private void PlayLanding()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.5f))
        {
            string surfaceTag = hit.collider.tag;
            // LĄDOWANIE: Używamy parametru dla lądowania
            PlaySurfaceSound(landEvent, landParamName, surfaceTag);
        }
        isJumping = false;
    }

    /// <summary>
    /// Tworzy jednorazową instancję zdarzenia, ustawia parametr i ją odtwarza.
    /// </summary>
    private void PlaySurfaceSound(EventReference eventRef, string paramName, string surfaceTag)
    {
        string surfaceParameter = null;

        // Mapowanie tagów Unity na etykiety (Labels) w FMOD Studio
        switch (surfaceTag)
        {
            case "Stone":
            case "Inside_stone":
            case "Outside":
                surfaceParameter = "Stone";
                break;

            case "Wood":
            case "Inside_wood":
                surfaceParameter = "Wood";
                break;

            case "Bed":
                surfaceParameter = "Bed";
                break;
        }

        // Jeśli trafiliśmy na zdefiniowane podłoże, odpalamy dźwięk przez Instancję (OneShot z parametrem)
        if (surfaceParameter != null && !eventRef.IsNull)
        {
            FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(eventRef);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
            instance.setParameterByNameWithLabel(paramName, surfaceParameter);
            instance.start();
            instance.release(); // Automatycznie zniszcz instancję po zakończeniu odtwarzania
        }
    }

    /// <summary>
    /// Sprawdza za pomocą Raycastu, czy stoimy na ziemi.
    /// </summary>
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, distToGround + 0.3f);
    }
}