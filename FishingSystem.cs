using System.Collections;
using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public GameObject fish;
    [SerializeField] GameObject player;
    [SerializeField] GameObject bait; //This is use to show the player that there's a fish
    [SerializeField] GameObject cameraFishingPoint; //The location of the camera when fishing
    [SerializeField] GameObject cameraSP; //The location of the original player camera
    private PlayerController ourPlayerController;
    private Color baitColor; // The original color of the bait
    private Renderer thisRenderer;
    private bool isFishing; // Check if the player is fishing
    private bool canBeFish; // Chick if the player caught the fish
    
    //private CinemachineBrain thisCinemachineBrain;

    private void OnTriggerStay(Collider col)
    {
        //Check the tag
        if (col.tag == "Player")
        {
            //Player press F
            if (Input.GetButton("Use1") && !isFishing)
            {
                //Start Fishing
                Fishing();
                
            }
        }
    }

    void Start()
    {
        isFishing = false;
        canBeFish = false;
        bait.SetActive(false);
        ourPlayerController = player.GetComponent<PlayerController>();
        thisRenderer = bait.GetComponent<Renderer>();
        baitColor = thisRenderer.material.color;
    }

    void Update()
    {
        // Fishing out a fish
        if (Input.GetKeyDown(KeyCode.R) && canBeFish)
        {
            canBeFish = false;
            StopCoroutine("StartFishing");
            thisRenderer.material.color = Color.yellow;

            GameObject thisFish = Instantiate(fish, bait.transform.position,
             bait.transform.rotation) as GameObject;

            ItemTest fishitemTest = thisFish.GetComponent<ItemTest>();

            fishitemTest.UseThisItem(player);

            StartCoroutine("ContinueFishing");
        }

        // Stop player from fishing mode
        if (Input.GetButton("Use1") && isFishing)
        {
            bait.SetActive(false);
            canBeFish = false;
            
            ourPlayerController.charState = CharState.movable;
            //ourPlayerController.isPlayerMoveable = true;

            thisRenderer.material.color = baitColor;

            StopCoroutine("StartFishing"); // Stop the Coroutine
            Camera.main.transform.position = cameraSP.transform.position;
            Camera.main.transform.rotation = cameraSP.transform.rotation;
            StartCoroutine("ExitFishing"); // Exit fishing
        }
    }
    public void Fishing()
    {
        ourPlayerController.charState = CharState.fishing;
        //ourPlayerController.isPlayerMoveable = false;
        // Set Camera position and rotation
        Camera.main.transform.position = cameraFishingPoint.transform.position;
        Camera.main.transform.rotation = cameraFishingPoint.transform.rotation;
        bait.SetActive(true);
        // Make the player look at the river
        Vector3 playerLookRotation = Quaternion.LookRotation(transform.position).eulerAngles;
        playerLookRotation.y = 0f;
        player.transform.rotation = Quaternion.Euler(playerLookRotation);
        StartCoroutine("StartFishing"); // Start a coroutine
    }
    private IEnumerator StartFishing()
    {
        canBeFish = false;
        yield return new WaitForSeconds(0.2f);
        isFishing = true;
        while (isFishing)
        {
            float waitTime = Random.Range(3f, 6f);
            yield return new WaitForSeconds(waitTime); //Wait for a few second for the fish
            thisRenderer.material.color = Color.red; //Change the color when there's fish

            canBeFish = true;
            waitTime = Random.Range(2f, 3f);
            yield return new WaitForSeconds(waitTime); //This is the window of time for the player to catch the fish
            canBeFish = false;
            thisRenderer.material.color = baitColor;
        }
    }

    private IEnumerator ExitFishing()
    {
        yield return new WaitForSeconds(0.2f);
        isFishing = false;
    }
    private IEnumerator ContinueFishing()
    {
        yield return new WaitForSeconds(0.2f);
        thisRenderer.material.color = baitColor;
        StartCoroutine("StartFishing");
    }
}
