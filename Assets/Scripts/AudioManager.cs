using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource weaponAudioSource;
    public AudioSource zombieAudioSource;
    public AudioSource playerAudioSource;
    
    [Header("Sound Effects")]
    public AudioClip gunshot;
    public AudioClip reload;
    public AudioClip zombieHit;
    public AudioClip zombieAttack;
    public AudioClip footstep;
    public AudioClip playerHurt;
    public AudioClip gameOver;
    
    private static AudioManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    public void PlayGunshot()
    {
        if (weaponAudioSource != null && gunshot != null)
        {
            weaponAudioSource.PlayOneShot(gunshot);
        }
    }
    
    public void PlayReload()
    {
        if (weaponAudioSource != null && reload != null)
        {
            weaponAudioSource.PlayOneShot(reload);
        }
    }
    
    public void PlayZombieHit()
    {
        if (zombieAudioSource != null && zombieHit != null)
        {
            zombieAudioSource.PlayOneShot(zombieHit);
        }
    }
    
    public void PlayZombieAttack()
    {
        if (zombieAudioSource != null && zombieAttack != null)
        {
            zombieAudioSource.PlayOneShot(zombieAttack);
        }
    }
    
    public void PlayFootstep()
    {
        if (playerAudioSource != null && footstep != null)
        {
            playerAudioSource.PlayOneShot(footstep, 0.3f);
        }
    }
    
    public void PlayPlayerHurt()
    {
        if (playerAudioSource != null && playerHurt != null)
        {
            playerAudioSource.PlayOneShot(playerHurt);
        }
    }
    
    public void PlayGameOver()
    {
        if (playerAudioSource != null && gameOver != null)
        {
            playerAudioSource.PlayOneShot(gameOver);
        }
    }
    
    public static AudioManager GetInstance()
    {
        return instance;
    }
}