using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpyPlane : MonoBehaviour
{
    public List<Friendly> team;
    public Camera cam;

    public int pingtype; //0: Move, 1: Target
    public int selectedunit = 0;
    float viewRadius = 3;
    public LayerMask friendlayer;

    public Texture2D crosshairmove;
    public Texture2D crosshairlook;
    float crosshairwidth;

    bool ObjectiveClearEnemies = false;
    bool ObjectiveSteal = false;
    bool ObjectiveSurvive = false;
    public float secondsleft = 0;
    public bool KillRequirement;
    public bool StealRequirement;
    public bool SurviveRequirement;
    public Text KillText;
    public Text StealText;
    public Text SurviveText;

    public string nextscene;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.visible = false;
        Cursor.SetCursor(crosshairmove, new Vector2(crosshairmove.width/2, crosshairmove.height/2), CursorMode.ForceSoftware);

        for (int i = 0; i < team.Count; i++)
        {
            team[i].plane = GetComponent<SpyPlane>();
        }

        if (KillRequirement)
        {
            KillText.enabled = true;
        }
        else
        {
            KillText.enabled = false;
        }
        if (StealRequirement)
        {
            StealText.enabled = true;
        }
        else
        {
            StealText.enabled = false;
        }
        if (SurviveRequirement)
        {
            SurviveText.enabled = true;
        }
        else
        {
            SurviveText.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (team[selectedunit] != null)
                {
                    if (pingtype == 0)
                    {
                        team[selectedunit].GetComponent<NavMeshAgent>().SetDestination(hit.point);
                    }
                    else if (pingtype == 1)
                    {
                        team[selectedunit].GetComponent<NavMeshAgent>().SetDestination(team[selectedunit].transform.position);
                        team[selectedunit].GetComponent<Friendly>().target = hit.point;
                        team[selectedunit].GetComponent<Friendly>().turning = true;
                    }
                }
                else
                {
                    Debug.Log("Killed in Action");
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Collider[] selectTargets = Physics.OverlapSphere(hit.point, viewRadius, friendlayer);
                for (int i = 0; i < selectTargets.Length; i++)
                {
                    var friendlyIndex = team.IndexOf(selectTargets[i].GetComponent<Friendly>());
                    SwitchUnit(friendlyIndex);
                }
            }
        }

        if (Input.GetKeyDown("1"))
        {
            SwitchUnit(0);
        }
        if (Input.GetKeyDown("2"))
        {
            SwitchUnit(1);
        }
        if (Input.GetKeyDown("3"))
        {
            SwitchUnit(2);
        }

        if (Input.GetKeyDown("q"))
        {
            SwitchPing(0);
        }
        if (Input.GetKeyDown("w"))
        {
            SwitchPing(1);
        }

        if (Input.GetKeyDown("r"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKey("escape"))
        {
            Debug.Log("Quitting");
            Application.Quit();
        }

        CheckTeamWipe();

        var enemiesleft = GameObject.FindGameObjectsWithTag("Enemy");
        if (!ObjectiveClearEnemies && enemiesleft.Length == 0)
        {
            ObjectiveClearEnemies = true;
            KillText.color = Color.yellow;
            Debug.Log("Objective completed: clear the area");
            CheckMissionComplete();
        }
        var files = GameObject.FindGameObjectsWithTag("Objective");
        if (!ObjectiveSteal && files.Length == 0)
        {
            ObjectiveSteal = true;
            StealText.color = Color.yellow;
            Debug.Log("Objective completed: steal the files");
            CheckMissionComplete();
        }
        if (secondsleft < 0)
        {
            if (!ObjectiveSurvive)
            {
                ObjectiveSurvive = true;
                secondsleft = 0;
                SurviveText.color = Color.yellow;
                Debug.Log("Objective completed: survive");
                CheckMissionComplete();
            }
        }
        else
        {
            secondsleft -= Time.deltaTime;
            var secondclock = (int)secondsleft % 60;
            if (secondclock < 10)
            {
                SurviveText.text = "- Survive for " + (int)secondsleft / 60 + ":0" + (int)secondsleft % 60;
            }
            else
            {
                SurviveText.text = "- Survive for " + (int)secondsleft / 60 + ":" + (int)secondsleft % 60;
            }
        }
    }

    public void SwitchUnit(int unitindex)
    {
        selectedunit = unitindex;
        for(int i = 0; i < team.Count; i++)
        {
            team[i].selected = false;
        }
        team[unitindex].selected = true;
    }

    public void SwitchPing(int newtype)
    {
        pingtype = newtype;
        if (newtype == 0)
        {
            Cursor.SetCursor(crosshairmove, new Vector2(crosshairmove.width / 2, crosshairmove.height / 2), CursorMode.ForceSoftware);
        }
        else
        {
            Cursor.SetCursor(crosshairlook, new Vector2(crosshairlook.width / 2, crosshairlook.height / 2), CursorMode.ForceSoftware);
        }
    }

    public void CheckTeamWipe()
    {
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i] != null)
            {
                return;
            }
        }
        Debug.Log("Mission failed");
        StartCoroutine("Restart", 3f);
    }

    void CheckMissionComplete()
    {
        if (KillRequirement)
        {
            if (!ObjectiveClearEnemies)
            {
                return;
            }
        }
        if (StealRequirement)
        {
            if (!ObjectiveSteal)
            {
                return;
            }
        }
        if (SurviveRequirement)
        {
            if (!ObjectiveSurvive)
            {
                return;
            }
        }
        Debug.Log("Win");
        StartCoroutine("NextLevel", 3f);
    }
    IEnumerator Restart(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    IEnumerator NextLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextscene);
    }
}
