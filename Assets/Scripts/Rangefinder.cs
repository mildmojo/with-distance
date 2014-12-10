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
    public bool IsActive;

    private SerialPort sp = new SerialPort("COM4", 9600);
    private List<float> buffer = new List<float>();

    void Awake() {
      Instance = this;
    }

    void Start() {
      StartCoroutine("Connect");
    }

    void Update() {
        // Connect();
      IsActive = sp.IsOpen;
      ReadSerialData();
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
      if (range_us == 0) return;

      buffer.Add(range_us / 58f);
      if (buffer.Count() > 10) buffer.RemoveAt(0);
      distance_mm = buffer.Average();
    }

}
