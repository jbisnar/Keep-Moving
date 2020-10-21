using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public float viewRadius = Mathf.Infinity;
    public float viewAngle = 90f;
    public List<Transform> visibleTargets = new List<Transform>();

    Mesh visionmesh;
    float visionlength = 10f;
    float aimMax = 4f;
    float aimRate = 1f;
    float aimDecay = .25f;
    float aimProgress = 0f;
    LineRenderer bulletLine;
    public Canvas localcanv;
    public RectTransform aimBar;
    Transform lastSeenUnit;

    public GameObject corpse;

    NavMeshAgent agent;
    public Transform patrolRoute;
    int nextPatrolPoint = 0;
    public int patroltype; //0: Move, 1: Look
    Vector3 lookPoint;
    bool turning;
    public float looktime;

    // Start is called before the first frame update
    void Start()
    {
        bulletLine = GetComponent<LineRenderer>();
        bulletLine.enabled = false;
        bulletLine.positionCount = 2;
        localcanv.enabled = false;
        localcanv.transform.SetParent(null, false);
        localcanv.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));

        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;

        Patrol();
    }

    // Update is called once per frame
    void Update()
    {
        FindVisibleTargets();
        if (visibleTargets.Count > 0)
        {
            agent.isStopped = true;
            //Turn towards visibleTargets[0]
            turning = false;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(-(Mathf.Atan2(visibleTargets[0].position.z - transform.position.z, visibleTargets[0].position.x - transform.position.x) * Mathf.Rad2Deg) + 90, Vector3.up), 360 * Time.deltaTime);
            aimProgress += aimRate * Time.deltaTime;
            if (aimProgress >= aimMax)
            {
                aimProgress = 0;
                localcanv.enabled = false;
                //Shoot
                bulletLine.SetPosition(0, transform.position);
                bulletLine.SetPosition(1, visibleTargets[0].position);
                bulletLine.enabled = true;
                StartCoroutine("EraseLine", .05f);
                visibleTargets[0].GetComponent<Friendly>().Kill();
                agent.isStopped = false;
                Patrol();
            }
            if (visibleTargets[0] != lastSeenUnit)
            {
                lastSeenUnit = visibleTargets[0];
                aimProgress = 0;
            }
        }
        else
        {
            if (aimProgress > 0)
            {
                aimProgress -= aimDecay * Time.deltaTime;
                if (aimProgress <= 0)
                {
                    aimProgress = 0;
                    agent.isStopped = false;
                    Patrol();
                    localcanv.enabled = false;
                }
            }
        }

        if (aimProgress > 0)
        {
            localcanv.transform.position = transform.position;
            localcanv.enabled = true;
            aimBar.sizeDelta = new Vector2((aimProgress / (float)aimMax) * aimBar.parent.GetComponent<RectTransform>().sizeDelta.x, aimBar.sizeDelta.y);
        }
        else
        {
            if (turning)
            {
                var turnquat = Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(-(Mathf.Atan2(lookPoint.z - transform.position.z, lookPoint.x - transform.position.x) * Mathf.Rad2Deg) + 90, Vector3.up), 360 * Time.deltaTime);
                if (Quaternion.Angle(transform.rotation, turnquat) == 0)
                {
                    turning = false;
                    StartCoroutine("LookAgain", looktime);
                }
                transform.rotation = turnquat;
            }

            if (patroltype == 0 && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                AdvancePatrol();
                Patrol();
            }
        }

        visionmesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        Vector2[] uv = new Vector2[3];
        int[] triangles = new int[3];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(visionlength*Mathf.Sin(viewAngle/2*Mathf.Deg2Rad),0, visionlength * Mathf.Cos(viewAngle / 2 * Mathf.Deg2Rad));
        vertices[2] = new Vector3(-visionlength * Mathf.Sin(viewAngle / 2 * Mathf.Deg2Rad), 0, visionlength * Mathf.Cos(viewAngle / 2 * Mathf.Deg2Rad));

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        visionmesh.vertices = vertices;
        visionmesh.uv = uv;
        visionmesh.triangles = triangles;

        Color[] visioncolors = new Color[3];
        visioncolors[0] = new Color(1, 1, 1, .5f);
        visioncolors[1] = new Color(1, 1, 1, 0);
        visioncolors[2] = new Color(1, 1, 1, 0);
        visionmesh.colors = visioncolors;
        GetComponent<MeshFilter>().mesh = visionmesh;

    }

    void FindVisibleTargets()
    {
        //visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < visibleTargets.Count; i++)
        {
            if (visibleTargets[i] == null)
            {
                visibleTargets.RemoveAt(i);
                continue;
            }
            Vector3 dirToTarget = (visibleTargets[i].transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, visibleTargets[i].transform.position);
            if (Vector3.Angle(transform.forward, dirToTarget) > viewAngle / 2)
            {
                visibleTargets.RemoveAt(i);
                continue;
            }
            else if(Physics.Raycast(transform.position, dirToTarget, distance, obstacleMask)){
                visibleTargets.RemoveAt(i);
                continue;
            }
        }

        for (int i = 0; i < targetsInViewRadius.Length; i++) {

            Vector3 dirToTarget = (targetsInViewRadius[i].transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle/2)
            {
                float distance = Vector3.Distance(transform.position, targetsInViewRadius[i].transform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distance, obstacleMask))
                {
                    if (!visibleTargets.Contains(targetsInViewRadius[i].transform))
                    {
                        visibleTargets.Add(targetsInViewRadius[i].transform);
                    }
                }
            }
        }
    }

    public void Kill()
    {
        GameObject mycorpse = GameObject.Instantiate(corpse, transform.position, Quaternion.Euler(Vector3.zero));
        mycorpse.transform.parent = null;
        Destroy(localcanv.gameObject);
        Destroy(gameObject);
    }

    void Patrol()
    {
        if (patrolRoute == null)
        {
            return;
        }
        else
        {
            if (patroltype == 0)
            {
                agent.destination = patrolRoute.GetChild(nextPatrolPoint).transform.position;
            }
            else
            {
                lookPoint = patrolRoute.GetChild(nextPatrolPoint).transform.position;
                turning = true;
            }
        }
    }
    void AdvancePatrol()
    {
        if (patrolRoute == null)
        {
            return;
        }
        else
        {
            nextPatrolPoint = (nextPatrolPoint + 1) % patrolRoute.childCount;
        }
    }

    IEnumerator EraseLine(float delay)
    {
        yield return new WaitForSeconds(delay);
        bulletLine.enabled = false;
    }

    IEnumerator LookAgain(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvancePatrol();
        Patrol();
    }
}
