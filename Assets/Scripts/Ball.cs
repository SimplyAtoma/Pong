using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    public int P1Score;
    public int P2Score;

    public AudioClip bounceClip;
    public AudioClip fastbounceClip;
    public AudioClip scoreClip;
    private bool p1Serve;
    private bool p2Serve;
    public GameObject winTextObject;    
    [Header("Movement")]
    public float speed = 8f;

    [Tooltip("Max bounce angle in degrees off the paddle.")]
    [Range(10f, 89f)]
    public float maxBounceAngle = 75f;

    [Header("Trajectory Clamps")]
    [Tooltip("Prevents the ball from going too vertical (near 90°).")]
    [Range(0f, 30f)]
    public float minAngleFromVertical = 10f;

    [Tooltip("Prevents the ball from going too horizontal (near 0°).")]
    [Range(0f, 30f)]
    public float minAngleFromHorizontal = 5f;

    [Header("Launch")]
    public float serveDelay = 0.5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Start()
    {
        winTextObject.SetActive(false);
        SetScoreText();
        ServeRandom();
    }

    public void ServeRandom()
    {
        rb.linearVelocity = Vector3.zero;
        Invoke(nameof(DoServe), serveDelay);
    }

    public void ServeLoser()
    {
        rb.linearVelocity = Vector3.zero;
        if (p1Serve)
        {
            Invoke(nameof(ServeP1), serveDelay);
            p1Serve = false;
            p2Serve = false;
        }
        else if (p2Serve)
        {
            Invoke(nameof(ServeP2), serveDelay);
            p2Serve = false;
            p1Serve = false;
        }
        else
        {
            Invoke(nameof(DoServe), serveDelay);
        }
    }

    void ServeP1()
    {
        // Serve towards the player who just lost
        float dirX = rb.position.x < 0f ? -1f : 1f;

        // Small random angle variation
        float angle = Random.Range(-15f, 15f);
        Vector3 dir = Quaternion.Euler(0f, 0f, angle) * new Vector3(-1f, 0f, 0f);
        rb.linearVelocity = dir.normalized * speed;
    }
    void ServeP2()
    {
        // Small random angle variation
        float angle = Random.Range(-15f, 15f);
        Vector3 dir = Quaternion.Euler(0f, 0f, angle) * new Vector3(1f, 0f, 0f);
        rb.linearVelocity = dir.normalized * speed;
    }

    void DoServe()
    {
        // A few consistent serve angles (classic feel)
        float[] angles = { -30f, -15f, 15f, 30f };
        float angle = angles[Random.Range(0, angles.Length)];

        float dirX = Random.value < 0.5f ? -1f : 1f;

        // X/Y plane serve direction
        Vector3 dir = Quaternion.Euler(0f, 0f, angle) * new Vector3(dirX, 0f, 0f);
        rb.linearVelocity = dir.normalized * speed;
    }

    void FixedUpdate()
    {
        // Keep constant speed & lock Z movement (2D gameplay in 3D)
        Vector3 v = rb.linearVelocity;
        v.z = 0f;

        if (v.sqrMagnitude > 0.0001f)
            rb.linearVelocity = v.normalized * speed;
        else
            rb.linearVelocity = Vector3.zero;

        // Also keep the ball positioned on Z=0 (optional but helpful)
        Vector3 p = transform.position;
        p.z = 0f;
        transform.position = p;
    }

    void OnCollisionEnter(Collision collision)
    {

        if (collision.collider.CompareTag("Paddle"))
        {   
            if (speed <= 12f)
            {
            SoundFXManager.Instance.PlaySound(bounceClip, transform);
            }
            
            else
            {
            SoundFXManager.Instance.PlaySound(fastbounceClip, transform);
            }
            speed = speed * 1.2f;
            BounceOffPaddle(collision);
        }
        else if (collision.collider.CompareTag("P1ScoreZone"))
        {
            // Scoring zone: reset speed and serve again
            SoundFXManager.Instance.PlaySound(scoreClip, transform);
            speed = 8f;
            P1Score+=1;
            Debug.Log("Player 1 Scored");
            Debug.Log("Score: " + P1Score + " - " + P2Score);
            SetScoreText();
            transform.position = new Vector3(0f,0f,0f);
            p2Serve = true;
            ServeLoser();
        }
        else if (collision.collider.CompareTag("P2ScoreZone"))
        {
            // Scoring zone: reset speed and serve again
            SoundFXManager.Instance.PlaySound(scoreClip, transform);
            speed = 8f;
            P2Score+=1;
            Debug.Log("Player 2 Scored");
            Debug.Log("Score: " + P1Score + " - " + P2Score);
            SetScoreText();
            transform.position = new Vector3(0f,0f,0f);
            p1Serve = true;
            ServeLoser();
        }
        else
        {
            // Walls: reflect using contact normal, then clamp.
            Vector3 reflected = Vector3.Reflect(rb.linearVelocity.normalized, collision.contacts[0].normal);
            SoundFXManager.Instance.PlaySound(bounceClip, transform);
            reflected.z = 0f;
            rb.linearVelocity = ClampDirectionXY(reflected) * speed;
        }
    }

    void BounceOffPaddle(Collision collision)
    {
        // Decide outgoing X direction based on which side the paddle is on.
        float paddleX = collision.transform.position.x;
        float ballX = transform.position.x;
        float outX = ballX > paddleX ? 1f : -1f;

        // Compute normalized hit position along paddle height (-1 bottom .. +1 top)
        Bounds b = collision.collider.bounds;
        float relativeY = (transform.position.y - b.center.y) / b.extents.y;
        relativeY = Mathf.Clamp(relativeY, -1f, 1f);

        // Map hit position to bounce angle
        float bounceAngle = relativeY * maxBounceAngle;
        float rad = bounceAngle * Mathf.Deg2Rad;

        // Build direction in XY plane
        Vector3 dir = new Vector3(Mathf.Cos(rad) * outX, Mathf.Sin(rad), 0f);
        dir = ClampDirectionXY(dir);

        rb.linearVelocity = dir.normalized * speed;
    }

    Vector3 ClampDirectionXY(Vector3 dir)
    {
        dir.z = 0f;
        dir.Normalize();

        // angle between direction and +X axis in XY (0=horizontal, 90=vertical)
        float angle = Mathf.Abs(Mathf.Atan2(dir.y, dir.x)) * Mathf.Rad2Deg;

        // Clamp away from too-horizontal
        if (angle < minAngleFromHorizontal)
        {
            float signY = Mathf.Approximately(dir.y, 0f) ? 1f : Mathf.Sign(dir.y);
            dir.y = Mathf.Tan(minAngleFromHorizontal * Mathf.Deg2Rad) * Mathf.Abs(dir.x) * signY;
        }

        // Clamp away from too-vertical
        if (angle > (90f - minAngleFromVertical))
        {
            float target = 90f - minAngleFromVertical;
            float signX = Mathf.Approximately(dir.x, 0f) ? 1f : Mathf.Sign(dir.x);
            float signY = Mathf.Approximately(dir.y, 0f) ? 1f : Mathf.Sign(dir.y);

            float rad = target * Mathf.Deg2Rad;
            dir = new Vector3(Mathf.Cos(rad) * signX, Mathf.Sin(rad) * signY, 0f);
        }

        return dir.normalized;
    }
        void SetScoreText()
    { 
        if(P1Score == 11)
            {
                winTextObject.SetActive(true);
                Destroy(this.gameObject);
                winTextObject.GetComponent<TextMeshProUGUI>().text = "Player 1 Wins!";
                P1Score=0;
                P2Score=0;
            }
            else if (P2Score == 11)
            {
                winTextObject.SetActive(true);
                Destroy(this.gameObject);
                winTextObject.GetComponent<TextMeshProUGUI>().text = "Player 2 Wins!";
                P1Score=0;
                P2Score=0;
            }
            ScoreText.text = P1Score.ToString() + " - " + P2Score.ToString();
    }
}
