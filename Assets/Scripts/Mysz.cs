using UnityEngine;
using FMODUnity;

public class RandomAmbientSound : MonoBehaviour
{
    [Header("Events")]
    [Tooltip("Lista eventów myszy; jeœli pusta, u¿yje pojedynczego 'ambientEvent'")]
    [SerializeField] private EventReference[] ambientEvents;
    [SerializeField] private EventReference ambientEvent; // backward compatibility

    [Header("Timing")]
    [SerializeField] private float minInterval = 5f;      // Minimum 5 sekund
    [SerializeField] private float maxInterval = 15f;     // Maksimum 15 sekund
    [Tooltip("Mno¿nik czêstotliwoœci (>1 = czêœciej)")]
    [SerializeField] private float frequencyMultiplier = 1.5f;

    [Header("Proximity (opcjonalne)")]
    [Tooltip("Gdy gracz blisko, dodatkowe przyspieszenie")]
    [SerializeField] private bool increaseWhenPlayerNear = false;
    [SerializeField] private float proximityRadius = 10f;
    [SerializeField] private float nearMultiplier = 2f;

    [Header("Variation")]
    [Tooltip("Zakres pitch (1 = normalne)")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;
    [Tooltip("Zakres g³oœnoœci (1 = normalne)")]
    [SerializeField] private float minVolume = 0.8f;
    [SerializeField] private float maxVolume = 1.0f;

    private float timer;

    void Start()
    {
        timer = GetNextInterval();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PlayRandomAmbient();
            timer = GetNextInterval(); // Losujemy czas do nastêpnego razu (bardziej prawdopodobne — mniejsze wartoœci = czêœciej)
        }
    }

    private float GetNextInterval()
    {
        float baseInterval = Random.Range(minInterval, maxInterval);
        float effectiveMultiplier = Mathf.Max(0.01f, frequencyMultiplier);

        if (increaseWhenPlayerNear && Camera.main != null)
        {
            float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
            if (dist <= proximityRadius)
            {
                effectiveMultiplier *= Mathf.Max(0.01f, nearMultiplier);
            }
        }

        return Mathf.Max(0.05f, baseInterval / effectiveMultiplier);
    }

    private void PlayRandomAmbient()
    {
        // Wybierz event (najpierw z listy, jeœli dostêpna)
        EventReference chosen = ambientEvent;
        if (ambientEvents != null && ambientEvents.Length > 0)
        {
            int idx = Random.Range(0, ambientEvents.Length);
            chosen = ambientEvents[idx];
        }

        if (chosen.IsNull) return;

        // Utwórz instancjê, zastosuj wariacje i odtwórz przestrzennie
        var instance = RuntimeManager.CreateInstance(chosen);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        float pitch = Random.Range(minPitch, maxPitch);
        float volume = Random.Range(minVolume, maxVolume);

        // Ustaw pitch i volume jeœli dostêpne w API
        instance.setPitch(pitch);
        instance.setVolume(volume);

        instance.start();
        instance.release(); // zwalnia pamiêæ po zakoñczeniu
    }
}