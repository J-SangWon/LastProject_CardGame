using UnityEngine;
using System.Collections;

public class Dice : MonoBehaviour
{

    private static Dice instance;
    public static Dice Instance => instance;

    [SerializeField] private float rotationSpeed;
    [SerializeField] private float stopDuration;
    [SerializeField] private float diceHeightFromPlayer;
    [SerializeField] private float resultDisplayDuration;


    private Quaternion[] faceRotations;

    private Rigidbody rb;

    private bool isSpin = false;
    private bool isStopping = false;
    private Quaternion baseRotation; // 카메라 회전 각도 보정용
    public bool IsStopped { get; private set; } = false;
    public int LastDiceValue { get; private set; }



    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        faceRotations = new Quaternion[6];
        faceRotations[0] = Quaternion.Euler(0, 90, 0);
        faceRotations[1] = Quaternion.Euler(0, 0, 0);
        faceRotations[2] = Quaternion.Euler(0, 90, 90);
        faceRotations[3] = Quaternion.Euler(0, -90, 90);
        faceRotations[4] = Quaternion.Euler(0, 180, 90);
        faceRotations[5] = Quaternion.Euler(0, 90, 180);

        gameObject.SetActive(false);
    }


    private void Update()
    {
        Spin();

    }

    private void Spin()
    {
        if (!isSpin)
        {
            return;
        }

        float sinFactor = Mathf.Sin(Time.time * 4f);
        float xRotationSpeed = rotationSpeed * sinFactor * 1.5f;

        Quaternion yRotation = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);
        Quaternion xRotation = Quaternion.Euler(xRotationSpeed * Time.deltaTime, 0, 0);

        transform.rotation = yRotation * xRotation * transform.rotation;
    }

    private IEnumerator StopSpin(int value, System.Action<int> onResult)
    {
        isSpin = false;
        isStopping = true;
        // Rigidbody 물리 회전 중지
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // 물리 계산 무시하고 수동 제어

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = baseRotation * faceRotations[value - 1];

        float timer = 0f;

        //AudioManager.Instance.StopSFX();

        while (timer < stopDuration)
        {
            timer += Time.deltaTime;
            float t = timer / stopDuration;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
        rb.isKinematic = false;
        

        yield return new WaitForSeconds(resultDisplayDuration);
        IsStopped = true;
        isStopping = false;


        gameObject.SetActive(false);
        onResult?.Invoke(value);
    }

    public void SetDice(Vector3 playerPosition, Transform cameraTransform)
    {
        gameObject.SetActive(true);
        StartCoroutine(SetDiceCoroutine(playerPosition, cameraTransform));
    }

    private IEnumerator SetDiceCoroutine(Vector3 playerPosition, Transform cameraTransform)
    {
        yield return null;

        transform.position = cameraTransform.position
                   + cameraTransform.forward * 5f   // 카메라 앞 5m
                   + Vector3.up * diceHeightFromPlayer;

        transform.position = playerPosition + Vector3.up * diceHeightFromPlayer;

        Vector3 baseDir = cameraTransform.forward;
        //baseDir.y = 0;

        if (baseDir == Vector3.zero)
        {
            baseRotation = Quaternion.identity;
        }
        else
        {
            baseDir.Normalize();
            baseRotation = Quaternion.LookRotation(baseDir);
        }

        transform.rotation = baseRotation;

        isSpin = true;
        IsStopped = false;

        
    }

    public int StopAndGetDiceValue(System.Action<int> onResult)
    {
        if (isStopping) return LastDiceValue;


        LastDiceValue = Random.Range(1, 7);
        StartCoroutine(StopSpin(LastDiceValue, onResult));
        Debug.Log("주사위 값 : " + LastDiceValue);
        return LastDiceValue;
    }

}
