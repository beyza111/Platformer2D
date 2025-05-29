using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CustomerAI : MonoBehaviour
{
    [Header("Settings")]
    public float waitTimeAtPoints = 3f;
    public float walkSpeedThreshold = 0.1f;
    public float exitDelayAfterCheckout = 0.5f;
    public float angryExitDelay = 2f; // Moved angry delay to serialized field

    private NavMeshAgent agent;
    private Animator anim;
    private Customer customer;
    private Transform currentTarget;
    private Transform spawnPoint;
    private Transform waitPoint;
    private Transform checkoutPoint;
    private Transform exitPoint;

    private enum CustomerState { MovingToWait, Waiting, MovingToCheckout, AtCheckout, Leaving }
    private CustomerState currentState;
    private bool isAngry;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        customer = GetComponent<Customer>();
    }

    void Start()
    {
        currentState = CustomerState.MovingToWait;
        MoveToTarget(waitPoint);
    }

    // Add this new method to force immediate leaving
    public void ForceLeave()
    {
        if (currentState == CustomerState.Leaving) return;

        currentState = CustomerState.Leaving;
        customer.IsActiveInScene = false; // ❌ Artık ürün alamaz
        customer.ReadyToCheckout = false;
        isAngry = false;

        agent.isStopped = false;
        anim.SetBool("IsAngry", false);
        anim.SetBool("IsWalking", true);
        MoveToTarget(exitPoint);
    }


    public void SetWaypoints(Transform spawn, Transform wait, Transform checkout, Transform exit)
    {
        spawnPoint = spawn;
        waitPoint = wait;
        checkoutPoint = checkout;
        exitPoint = exit;
    }

    void Update()
    {
        if (customer.IsServed || isAngry) return;

        anim.SetBool("IsWalking", agent.velocity.magnitude > walkSpeedThreshold);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            HandleStateCompletion();
        }
    }

    void MoveToTarget(Transform target)
    {
        if (target == null) return;

        currentTarget = target;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    void HandleStateCompletion()
    {
        switch (currentState)
        {
            case CustomerState.MovingToWait:
                StartCoroutine(WaitAtPoint(waitTimeAtPoints, CustomerState.MovingToCheckout));
                break;

            case CustomerState.MovingToCheckout:
                // Kasaya gitmeden önce direkt olarak işlemi tamamlamaya çalış
                currentState = CustomerState.AtCheckout;
                customer.ReadyToCheckout = true;  // Artık satış yapılabilir.
                customer.IsActiveInScene = true;  // Müşteri artık ürün alabilir
                anim.SetBool("IsWalking", false);
                break;

            case CustomerState.Leaving:
                if (currentTarget == exitPoint &&
                    !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance)
                {
                    customer.FinalizeExit();
                }
                break;
        }
    }


    IEnumerator WaitAtPoint(float waitTime, CustomerState nextState)
    {
        currentState = CustomerState.Waiting;
        anim.SetBool("IsWalking", false);
        yield return new WaitForSeconds(waitTime);
        currentState = nextState;
        MoveToTarget(nextState == CustomerState.MovingToCheckout ? checkoutPoint : exitPoint);
    }

    public void TriggerAngryReaction()
    {
        if (isAngry) return;

        isAngry = true;
        agent.isStopped = true;
        anim.SetBool("IsAngry", true);
        anim.SetBool("IsWalking", false);
        customer.ReadyToCheckout = false;
        Invoke("ForceLeave", angryExitDelay); // Changed to use ForceLeave
    }

    public void OnCheckoutComplete()
    {
        if (currentState == CustomerState.AtCheckout)
        {
            StartCoroutine(StartLeavingAfterDelay());
        }
    }

    IEnumerator StartLeavingAfterDelay()
    {
        yield return new WaitForSeconds(exitDelayAfterCheckout);
        ForceLeave(); // Changed to use ForceLeave
    }

    void OnDrawGizmosSelected()
    {
        if (waitPoint) Gizmos.DrawLine(transform.position, waitPoint.position);
        if (checkoutPoint && waitPoint) Gizmos.DrawLine(waitPoint.position, checkoutPoint.position);
        if (exitPoint && checkoutPoint) Gizmos.DrawLine(checkoutPoint.position, exitPoint.position);
    }


}