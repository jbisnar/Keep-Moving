using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Friendly : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public float viewRadius = Mathf.Infinity;
    public float viewAngle = 45f;
    public List<Transform> visibleTargets = new List<Transform>();

    Mesh visionmesh;
    float visionlength = 10f;
    float aimMax = 4.5f;
    float aimRateMoving = .5f;
    float aimRateStill = 1f;
    float aimProgress = 0f;
    LineRenderer bulletLine;
    public Canvas localcanv;
    public RectTransform aimBar;

    public SpyPlane plane;
    public bool selected = false;
    public bool turning;
    public Vector3 target;
    public GameObject corpse;

    // Start is called before the first frame update
    void Start()
    {
        bulletLine = GetComponent<LineRenderer>();
        bulletLine.enabled = false;
        bulletLine.positionCount = 2;
        //localcanv.enabled = false;
        localcanv.transform.SetParent(null, false);
        localcanv.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (turning)
        {
            var turnquat = Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(-(Mathf.Atan2(target.z - transform.position.z, target.x - transform.position.x) * Mathf.Rad2Deg) + 90, Vector3.up), 360 * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, turnquat) == 0)
            {
                turning = false;
            }
            transform.rotation = turnquat;
        }

        FindVisibleTargets();
        if (visibleTargets.Count > 0)
        {
            if (GetComponent<NavMeshAgent>().velocity == Vector3.zero)
            {
                aimProgress += aimRateStill * Time.deltaTime;
            }
            else
            {
                aimProgress += aimRateMoving * Time.deltaTime;
            }
            if (aimProgress >= aimMax)
            {
                aimProgress = 0;
                //Shoot
                bulletLine.SetPosition(0, transform.position);
                bulletLine.SetPosition(1, visibleTargets[0].position);
                bulletLine.enabled = true;
                StartCoroutine("EraseLine", .05f);
                visibleTargets[0].GetComponent<Enemy>().Kill();
            }
        }
        else
        {
            aimProgress = 0;
            localcanv.transform.GetChild(0).gameObject.SetActive(false);
        }

        if (aimProgress > 0)
        {
            localcanv.transform.GetChild(0).gameObject.SetActive(true);
            aimBar.sizeDelta = new Vector2((aimProgress / (float)aimMax) * aimBar.parent.GetComponent<RectTransform>().sizeDelta.x, aimBar.sizeDelta.y);
        }

        localcanv.transform.position = transform.position;
        if (selected)
        {
            localcanv.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            localcanv.transform.GetChild(1).gameObject.SetActive(false);
        }

        visionmesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        Vector2[] uv = new Vector2[3];
        int[] triangles = new int[3];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(visionlength * Mathf.Sin(viewAngle / 2 * Mathf.Deg2Rad), 0, visionlength * Mathf.Cos(viewAngle / 2 * Mathf.Deg2Rad));
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
            else if (Physics.Raycast(transform.position, dirToTarget, distance, obstacleMask))
            {
                visibleTargets.RemoveAt(i);
                continue;
            }
        }

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {

            Vector3 dirToTarget = (targetsInViewRadius[i].transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
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

    IEnumerator EraseLine(float delay)
    {
        yield return new WaitForSeconds(delay);
        bulletLine.enabled = false;
    }
}
