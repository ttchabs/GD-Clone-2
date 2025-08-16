using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CombatMusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientMusicSource;
    [SerializeField] private AudioSource combatMusicSource;
    
    [Header("Music Clips")]
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private AudioClip combatMusic;
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 3f;
    [SerializeField] private float combatDetectionRange = 10f;
    [SerializeField] private float combatEndDelay = 1f; 
    
    [Header("Volume Settings")]
    [SerializeField] private float ambientVolume = 0.6f;
    [SerializeField] private float combatVolume = 0.8f;
    
   
    private bool isInCombat = false;
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private int enemiesInRange = 0;
    private Coroutine currentFadeCoroutine;
    private Coroutine combatCheckCoroutine;
    
   
    public static CombatMusicManager Instance { get; private set; }
    
    private void Awake()
    {
     
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
       
        SetupAudioSources();
    }
    
    private void Start()
    {
        
        PlayAmbientMusic();
        
        
        combatCheckCoroutine = StartCoroutine(CombatDetectionLoop());
    }
    
    private void SetupAudioSources()
    {
        if (ambientMusicSource == null)
        {
            GameObject ambientObj = new GameObject("GameMusicSource");
            ambientObj.transform.SetParent(transform);
            ambientMusicSource = ambientObj.AddComponent<AudioSource>();
        }
        
        if (combatMusicSource == null)
        {
            GameObject combatObj = new GameObject("CombatMusicSource");
            combatObj.transform.SetParent(transform);
            combatMusicSource = combatObj.AddComponent<AudioSource>();
        }
        
     
        ambientMusicSource.loop = true;
        ambientMusicSource.playOnAwake = false;
        ambientMusicSource.volume = ambientVolume;
        
        combatMusicSource.loop = true;
        combatMusicSource.playOnAwake = false;
        combatMusicSource.volume = 0f; 
    }
    
    private void PlayAmbientMusic()
    {
        if (ambientMusic != null && ambientMusicSource != null)
        {
            ambientMusicSource.clip = ambientMusic;
            ambientMusicSource.Play();
        }
    }
    
    private IEnumerator CombatDetectionLoop()
    {
        while (true)
        {
            CheckForEnemiesNearPlayer();
            yield return new WaitForSeconds(0.5f); 
        }
    }
    
    private void CheckForEnemiesNearPlayer()
    {
      
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        
        Collider2D[] enemiesInArea = Physics2D.OverlapCircleAll(
            player.transform.position, 
            combatDetectionRange, 
            LayerMask.GetMask("Enemy")
        );
        
        
        int aliveEnemiesCount = 0;
        foreach (Collider2D enemy in enemiesInArea)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsAlive()) 
            {
                aliveEnemiesCount++;
            }
        }
        
       
        if (aliveEnemiesCount > 0 && !isInCombat)
        {
            StartCombatMusic();
        }
        else if (aliveEnemiesCount == 0 && isInCombat)
        {
            StartCoroutine(EndCombatMusicWithDelay());
        }
        
        enemiesInRange = aliveEnemiesCount;
    }
    
    public void StartCombatMusic()
    {
        if (isInCombat || isFadingIn) return;
        
       
        
        isInCombat = true;
        isFadingIn = true;
        
        // Stop any current fade
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
    
        if (combatMusic != null && combatMusicSource != null)
        {
            combatMusicSource.clip = combatMusic;
            combatMusicSource.Play();
            currentFadeCoroutine = StartCoroutine(FadeToCombatMusic());
        }
    }
    
    private IEnumerator EndCombatMusicWithDelay()
    {
        yield return new WaitForSeconds(combatEndDelay);
       
        CheckForEnemiesNearPlayer();
        if (enemiesInRange == 0)
        {
            EndCombatMusic();
        }
    }
    
    public void EndCombatMusic()
    {
        if (!isInCombat || isFadingOut) return;
        
     
        
        isInCombat = false;
        isFadingOut = true;
        
      
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeToAmbientMusic());
    }
    
    private IEnumerator FadeToCombatMusic()
    {
        float elapsedTime = 0f;
        float startAmbientVolume = ambientMusicSource.volume;
        float startCombatVolume = combatMusicSource.volume;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            
        
            ambientMusicSource.volume = Mathf.Lerp(startAmbientVolume, 0f, progress);
            
           
            combatMusicSource.volume = Mathf.Lerp(startCombatVolume, combatVolume, progress);
            
            yield return null;
        }
        
        
        ambientMusicSource.volume = 0f;
        combatMusicSource.volume = combatVolume;
        isFadingIn = false;
        
       
    }
    
    private IEnumerator FadeToAmbientMusic()
    {
        float elapsedTime = 0f;
        float startAmbientVolume = ambientMusicSource.volume;
        float startCombatVolume = combatMusicSource.volume;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeOutDuration;
            
        
            ambientMusicSource.volume = Mathf.Lerp(startAmbientVolume, ambientVolume, progress);
            
         
            combatMusicSource.volume = Mathf.Lerp(startCombatVolume, 0f, progress);
            
            yield return null;
        }
        
      
        ambientMusicSource.volume = ambientVolume;
        combatMusicSource.volume = 0f;
        
    
        combatMusicSource.Stop();
        isFadingOut = false;
        
       
    }
    
   
    public void ForceStartCombat()
    {
        StartCombatMusic();
    }
    
    public void ForceEndCombat()
    {
        EndCombatMusic();
    }
    
    public bool IsInCombat()
    {
        return isInCombat;
    }
    
   
    public void OnEnemyDeath()
    {
        
        StartCoroutine(CheckCombatEndOnNextFrame());
    }
    
    private IEnumerator CheckCombatEndOnNextFrame()
    {
        yield return null;
        CheckForEnemiesNearPlayer();
    }
    
    private void OnDestroy()
    {
        if (combatCheckCoroutine != null)
        {
            StopCoroutine(combatCheckCoroutine);
        }
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
    }
    
 
    private void OnDrawGizmosSelected()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Gizmos.color = isInCombat ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, combatDetectionRange);
        }
    }
}
