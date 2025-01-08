using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    // ĳ���� ��Ʈ�ѷ�
    CharacterController controller;
    // ������ �ۿ�Ǵ� ��
    public float jumpPower = 1.2f;
    // �߷� ���ӵ�
    public float gravity = -15f;
    // �ȱ� ���ǵ�
    public float moveSpeed = 2.0f;
    // ȸ���ӵ�
    public float rotationFactorPerFrame = 0.1f;
    // ������ ��ǥ
    public Transform groundCheck;
    // ������� �Ÿ�
    public float groundDistance = 0.4f;
    // ����Ȯ�� ���̾��ũ
    public LayerMask staticObjectLayerMask;
    // ī�޶� Transform
    public Transform cameraTransform;

    // �̵� ����
    Vector3 moveDirection;
    // ������ �̵� �ӷ�
    float verticalVelocity;
    // ���� ������ y��ġ
    float previousHeight;
    // ���� ��� �ִ��� üũ
    bool isGrounded;
    // Animator ������Ʈ �ν��Ͻ�
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        isGrounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. ������ �߷�ó��
        JumpAndGravity();
        // 2. ���� üũ ó��
        GroundedCheck();
        // 3. �̵�
        Move();
    }

    void JumpAndGravity()
    {
        if (isGrounded)
        {
            // �ִϸ����� �Ķ���� �ʱ�ȭ (���� �����Ƿ� ������ ���� ���´� �ƴϰ� �ٲ۴�)
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);

            // ���ϼӵ��� �ʱ�ȭ �Ѵ�.
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // ���� Ű �Է� üũ
            if (Input.GetAxis("Jump") > Mathf.Epsilon)
            {
                // ���� �ӵ��� ���� �̵��ӵ��� ���
                verticalVelocity = Mathf.Sqrt(jumpPower * -2 * gravity);
                animator.SetBool("IsJumping", true);
            }
        }
        else
        {
            // ���� ������ �����ϴ� ���� üũ
            float diffHeight = previousHeight - transform.position.y;
            if (diffHeight > 0f)
            {
                // �ִϸ��̼��� ���� ���·� ��ȯ
                animator.SetBool("IsFalling", true);
            }
        }

        // �߷��� ������ ���� ���� �̵� �ӵ��� ���
        verticalVelocity += gravity * Time.deltaTime;
    }

    void GroundedCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, staticObjectLayerMask);

        animator.SetBool("IsGrounded", isGrounded);
    }

    void Move()
    {
        // �̵� ���� �ʱ�ȭ
        moveDirection = Vector3.zero;

        // �뽬�� �����°�?
        float dash = Input.GetAxis("Fire3");

        float speedScale = 1.0f;
        if (dash > Mathf.Epsilon)
        {
            speedScale += dash;
        }

        // ���� ���� �̵� �ӵ� ���
        float currentSpeed = moveSpeed * speedScale;

        // �Է°�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // �Է°����� �Է� ���� �����
        Vector3 inputVector = new Vector3(horizontal, 0, vertical);

        // �Էº����� ���� ����
        float inputMagnitude = inputVector.magnitude;

        // ī�޶� �ٶ󺸴� ����� ī�޶��� ������ �������� �Է°��� �� ���� ���� ���� �̵� ���� ���͸� ����
        moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;

        // �� �̵� ������ ���� ������ 0�� ����
        moveDirection.y = 0f;

        // ����ȭ �Ͽ� ũ�⸦ 1�� �����.
        moveDirection.Normalize();

        // ���� ���� �̵� �ӵ��� DeltaTime�� ���ϰ�
        moveDirection *= Time.deltaTime * currentSpeed;

        // ���� �̵����� + (Y���� ���� �̵� �ӵ��� ������)���� �̵����Ϳ� deltaTime�� ���� ���͸�ŭ �����δ�.
        controller.Move(moveDirection + new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        // �̵� ������ ũ�Ⱑ 0���� ũ�ٸ� �ȱ� ���°� �ȴ�.
        bool isWalking = moveDirection.magnitude > Mathf.Epsilon;

        // �ȱ� ���¶��
        if (isWalking)
        {
            // ĳ���Ͱ� �ٶ󺸴� ���⵵ �̵� �������� �����ش�.
            //transform.forward = moveDirection;
            HandleRotation();
        }

        // �ִ� ���� �̵� �ӵ�
        float maxInputMagnitude = 4.0f;

        // ���� ���� �̵� �ӵ�
        float currentMagnitude = inputMagnitude * currentSpeed;

        // currentMagnitu
        float blendSpeed = currentMagnitude / maxInputMagnitude;

        animator.SetFloat("Speed", blendSpeed);
        previousHeight = transform.position.y;
    }

    void HandleRotation()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision!!");
    }
}
