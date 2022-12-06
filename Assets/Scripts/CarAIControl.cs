using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarAIControl : MonoBehaviour
{
    public float desiredAccel;
    public Camera leftCamera, centerCamera, rightCamera;

    [HideInInspector] public string imagePath = "";
    [HideInInspector] public float predictedSteering = 0f;
    
    CarController car;
    GameController gameController;
    PredictionClient client;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        gameController.isModeAutonomous = true;

        car = GetComponent<CarController>();
        client = FindObjectOfType<PredictionClient>();

        gameController.CreateDirectory();
        StartCoroutine(CaptureEveryFrame());
    }

    IEnumerator CaptureEveryFrame()
    {
        yield return new WaitForSeconds(1f);

        leftCamera.GetComponent<SnapshotController>().TakeSnapshot();
        SendRequest(imagePath);

        centerCamera.GetComponent<SnapshotController>().TakeSnapshot();
        SendRequest(imagePath);

        rightCamera.GetComponent<SnapshotController>().TakeSnapshot();
        SendRequest(imagePath);

        StartCoroutine(CaptureEveryFrame());
    }

    private void FixedUpdate()
    {
        car.Move(predictedSteering, desiredAccel, 0f, 0f);
    }

    public void SendRequest(string input)
    {
        if (input.Length > 0)
        {
            client.Predict(input, output =>
            {
                predictedSteering = output;
                Debug.Log("Predicted Steering: " + predictedSteering * 6f);
            }, error =>
            {

            });
        }
    }

    public void SavePredictLog ()
    {
        string row = string.Format("{0},{1},{2},{3},{4}\n", gameController.path[0], gameController.path[1], gameController.path[2], predictedSteering, car.currentSpeed);
        System.IO.File.AppendAllText(gameController.dataPath + "/csv/predict_log.csv", row);

    }
}
