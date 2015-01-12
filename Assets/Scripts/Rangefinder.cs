using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;

public class Rangefinder : MonoBehaviour
{
    public static Rangefinder Instance;

    [HideInInspector] [System.NonSerialized]
    public float distance_mm;

    [HideInInspector] [System.NonSerialized]
    public float raw_mm;

    [HideInInspector] [System.NonSerialized]
    public float idleTime;

    [HideInInspector] [System.NonSerialized]
    public bool IsActive;

    private SerialPort sp = new SerialPort("COM4", 9600);
    private List<float> buffer;
    private List<float> rawBuffer;

    private GameManager gameManager;

    private float RANGE_MIN;
    private float RANGE_MAX;

    public void Reset() {
      idleTime = 0f;
      // Game starts in attract mode where sensor sweep should be empty and
      // sensor value should be maxed out.
      buffer = new List<float>() {RANGE_MAX};
      rawBuffer = new List<float>() {0};
    }

    void Awake() {
      Instance = this;
      RANGE_MIN = PlayerPrefs.GetFloat("SensorMinDistance", 80f);
      RANGE_MAX = PlayerPrefs.GetFloat("SensorMaxDistance", 270f);
      Reset();
    }

    void Start() {
      StartCoroutine("Connect");
      gameManager = GameManager.Instance;
    }

    void Update() {
      IsActive = sp.IsOpen;

      if (IsActive) {
        ReadSerialData();
      } else {
        if (Input.GetKey(KeyCode.DownArrow)) {
          distance_mm += 40 * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.UpArrow)) {
          distance_mm -= 40 * Time.deltaTime;
        }
      }

      distance_mm = Mathf.Max(distance_mm, RANGE_MIN);

      // Accrue idle time when sensor gets no readings (zeroes) or average
      // distance is beyond the max.
      if (raw_mm < 0.1f || distance_mm >= RANGE_MAX) {
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
      if (rawBuffer.Count() > 10) rawBuffer.RemoveAt(0);
      raw_mm = rawBuffer.Average();

      // Accumulate nonzero sensor readings converted to distance, calc average.
      if (range_us > 0) buffer.Add(range_us / 58f);
      if (buffer.Count() > 10) buffer.RemoveAt(0);
      distance_mm = buffer.Average();
    }

}
