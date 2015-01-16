using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;

public class Rangefinder : MonoBehaviour
{
    public static Rangefinder Instance;

    [System.NonSerialized]
    public float distance_cm;

    [System.NonSerialized]
    public float raw_cm;
    [System.NonSerialized]
    public float last_raw_cm;

    [System.NonSerialized]
    public float idleTime;

    [System.NonSerialized]
    public bool IsActive;

    private SerialPort sp = new SerialPort("COM4", 9600);
    private List<float> buffer;
    private List<float> rawBuffer;

    private GameManager gameManager;

    public void Reset() {
      idleTime = 0f;
      // Game starts in attract mode where sensor sweep should be empty and
      // sensor value should be maxed out.
      distance_cm = gameManager.SensorMaxDistance;
      buffer = new List<float>() {distance_cm};
      rawBuffer = new List<float>() {0};
    }

    void Awake() {
      Instance = this;
    }

    void Start() {
      gameManager = GameManager.Instance;
      Reset();
      StartCoroutine("Connect");
    }

    void Update() {
      IsActive = sp.IsOpen;

      if (IsActive) {
        ReadSerialData();
      } else {
        if (Input.GetKey(KeyCode.DownArrow)) {
          distance_cm += 40 * Time.deltaTime;
          idleTime = 0f;
        } else if (Input.GetKey(KeyCode.UpArrow)) {
          distance_cm -= 40 * Time.deltaTime;
          idleTime = 0f;
        }
      }

      distance_cm = Mathf.Max(distance_cm, gameManager.SensorMinDistance);

      // Accrue idle time when sensor gets no readings (zeroes) or average
      // distance is beyond the max.
      var min_cm = gameManager.SensorMinDistance;
      var max_cm = gameManager.SensorMaxDistance;
      if (raw_cm < min_cm * 0.25 || distance_cm >= max_cm) {
        idleTime += Time.deltaTime;
      } else {
        idleTime = 0f;
      }
    }

    void OnApplicationQuit() {
        sp.Close();
    }

    IEnumerator Connect() {
      while (!sp.IsOpen) {
        try {
          sp.Open();
          sp.ReadTimeout = 1;
        } catch (System.Exception e) {
          Debug.Log(e);
        }

        yield return new WaitForSeconds(2);
      }
    }

    void ReadSerialData() {
      if (!sp.IsOpen) return;

      string data = "";
      try {
          data = sp.ReadTo("\x0A");
      } catch (System.TimeoutException) {
      } catch (System.Exception e) {
          Debug.Log("Exception: " + e);
      }
      if (data == "") return;

      int range_us;
      int.TryParse(data, out range_us);

      // Accumulate all sensor readings, including zeroes.
      rawBuffer.Add(range_us / 58f);
      while (rawBuffer.Count() > 10) rawBuffer.RemoveAt(0);
      raw_cm = rawBuffer.Average();
      last_raw_cm = rawBuffer.Last();

      // Accumulate nonzero sensor readings converted to distance, calc average.
      if (range_us > 0) buffer.Add(range_us / 58f);
      while (buffer.Count() > 10) buffer.RemoveAt(0);
      distance_cm = buffer.Average();
    }

}
