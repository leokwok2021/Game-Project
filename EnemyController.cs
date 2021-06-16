using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] float wanderRange = 10f;
    [SerializeField] float nextCheck = 0;
    [SerializeField] float timeToRefresh = 5;
    [SerializeField] float detactPlayerDistance = 5; //The closest distance to detact the player
    [SerializeField] float lostPlayerDistance = 10; //The furthest distance to lost the player
    [SerializeField] float turnTime = 10;
    private float distanceThatFollow;

    private float nextTimeToFire;
    private Renderer thisRenderer;
    private Color thisColor;

    private bool isplayerStealthActive;

    private Vector3 randomPoint;


    [SerializeField] GameObject enemySP; //Enemy Spawn Point

    [SerializeField] GameObject ourProjectile; //Enemy Projectile
    [SerializeField] GameObject firePosition; // The position where the enemy fire their projectile
    [SerializeField] float fireRate = 3f;
    [SerializeField] float projectileSpeed = 60f;
    [SerializeField] float rangeForFireProjectile = 7f;

    private EnemyState thisEnemyState;

    private enum EnemyState
    {
        followingPlayer, //0
        backToSpawn, //1
        idleState, //2
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        NewPosition();
        thisRenderer = GetComponent<Renderer>();
        thisColor = thisRenderer.material.color;
        distanceThatFollow = detactPlayerDistance;
        isplayerStealthActive = false;
        Vector3 enemySP = transform.position;
        thisEnemyState = EnemyState.idleState;
        Debug.Log(enemySP);
    }

    

    // Update is called once per frame
    void Update()
    {
        // Constantly check for the player
        CheckForFollowingPlayer();
        Debug.Log(thisEnemyState);
        switch ((int)thisEnemyState)
        {
            case (0): //followingPlayer
                RotateTowardsPlayer();
                //transform.LookAt(player.transform.position);
                EnemyFollowPlayer();
                distanceThatFollow = lostPlayerDistance;
                break;

            case (1): //backToSpawn
                EnemyMove(enemySP.transform.position);
                distanceThatFollow = detactPlayerDistance;
                break;
            case (2): //idelStat
                NewPosition();
                EnemyIdle();
                distanceThatFollow = detactPlayerDistance;
                break;
            default:
                break;
        }

        //Change to idleState when near the spawn
        if (thisEnemyState == EnemyState.backToSpawn && agent.velocity == new Vector3 (0,0,0))
        {
            thisEnemyState = EnemyState.idleState;
        }

        // Fire a SphereCast to detect the player
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, rangeForFireProjectile, ~player.layer);
        if (thisEnemyState == EnemyState.followingPlayer && hit.transform)
        {
            //Fire a projectile when player is detected
            thisRenderer.material.color = Color.red;
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1F / fireRate;
                Shoot();

            }
            /*
            Debug.Log("hit");
            if (Physics.Raycast(transform.position, transform.forward, out hit, rangeForFireProjectile, ~player.layer))
            {
                //Fire a projectile when player is detected
                thisRenderer.material.color = Color.red;
                if (Time.time >= nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1F / fireRate;
                    Shoot();

                }
            } 
            else
            {
                thisRenderer.material.color = thisColor;
            }
            */
        }
        else
        {
            thisRenderer.material.color = thisColor;
        }
    }
    
    //Give the enemy a new position to go when idle
    public void NewPosition()
    {
        randomPoint = gameObject.transform.position + Random.insideUnitSphere * wanderRange;

    }

    // Rotate to look at player
    public void RotateTowardsPlayer()
    {
        float turnVelocity = 1;
        float targetAngle = Mathf.Atan2(player.transform.position.x, player.transform.position.z) * Mathf.Rad2Deg;
        float currentAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnTime);
        transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    //Check if the player is nearby
    private void CheckForFollowingPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distanceThatFollow);

        foreach (var hitCol in hitColliders)
        {
            if (hitCol.GetComponent<Collider>().tag == "Player" && !isplayerStealthActive)
            {
                thisEnemyState = EnemyState.followingPlayer;
                return;
            }
        }
        if (thisEnemyState == EnemyState.followingPlayer)
        {
            thisEnemyState = EnemyState.backToSpawn;
        }
        return;
    }

    //Enemy follows the player
    public void EnemyFollowPlayer()
    {
        agent.SetDestination(player.transform.position);
        
    }

    //Enemy towards a position
    private void EnemyMove(Vector3 thisPosition)
    {
        agent.SetDestination(thisPosition);
    }

    //Shoot projectile
    private void Shoot()
    {
        Quaternion fireRange = Quaternion.Euler(firePosition.transform.rotation.eulerAngles + new Vector3(Random.Range(2, 0), Random.Range(1, -1), 0));
        //Spawn Projectile
        GameObject projectile = Instantiate(ourProjectile, firePosition.transform.position,
             fireRange) as GameObject;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = projectile.transform.forward * (projectileSpeed + Random.Range(0, -2));

        //Destroy projectile
        Destroy(projectile, 5f);

    }

    //Enemy is not doing anything
    public void EnemyIdle()
    {
        float distToDest = Vector3.Distance(gameObject.transform.position, agent.destination);
        nextCheck -= Time.deltaTime;
            
        if (nextCheck <= 0)
        {
            float oof = Random.Range(0, 2);
            if (oof >= 1)
            {
                nextCheck = timeToRefresh;
                randomPoint = gameObject.transform.position + Random.insideUnitSphere * wanderRange;
            }
            else
            {
                nextCheck = timeToRefresh;
            }
            agent.SetDestination(randomPoint);
        }
        
    }
}
