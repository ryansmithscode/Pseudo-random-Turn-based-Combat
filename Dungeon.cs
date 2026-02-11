using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
using TMPro;
using UnityEngine.UI;

// Ryan Smith Pseudo-random Turn-based Combat Script

public class GameProduction : MonoBehaviour
{
    [Header("Rewired")]
    private Player player;

    [Header("Health")]
    // Health integer variable, ui bar & healing with amount
    private int minimumHealth = 10; // because instantly killing them would be too harsh
    private int maximumHealth = 100;
    private int currentHealth;
    public Slider healthSlider;

    [Header("Potion")]
    private int minimumPotion = 0;
    private int maximumPotion = 14; // Limit amount player can have at once
    private int currentPotion = 0;
    public TMP_Text potionAmountText;

    [Header("Canvas'")]
    public GameObject playerHUD; 
    public GameObject scoreScreen;

    [Header("Informing Player")]
    public TMP_Text updateTextHealth; 
    public TMP_Text chatLogUpdates;

    [Header("Score Screen")]
    private int defeatedCounter;
    private float elapsedTime = 0f;
    private bool timerRunning;
    public TMP_Text defeatedText;
    public TextMeshProUGUI timerText;
    public GameObject scoreScreenDeathTitle; //circumstantial/ responsive title
    public GameObject scoreScreenAliveTitle;

    [Header("Enemy")]
    private double spawnChance = 0.6; // Chance of enemy appearing
    private int enemyCurrentHealth;
    private int currentDamage;
    public GameObject enemy;
    public Slider enemyHealthBar;
    public TMP_Text enemyText;
    public Animator enemyHitAnimation;

    private void Awake()
    {
        currentHealth = 100;

        timerRunning = true;

        player = ReInput.players.GetPlayer(0); // Gets Scene's Actions (Just the reset key really)
    }

    //-----------------------------------Start is called once upon creation-------------------------
    private void Start()
    {
        UpdateHealthBar();

        currentPotion = Random.Range(minimumPotion, maximumPotion);
        UpdatePotionAmountText();

        enemyCurrentHealth = Random.Range(minimumHealth, maximumHealth);

        enemyHealthBar.value = enemyCurrentHealth;
    }

    //-----------------------------------Update is called once per frame----------------------------
    void Update()
    {
        UpdateHealthBar();
        UpdatePotionAmountText();
        checkCurrentHealth();

        if (player.GetButtonDown("Reset")) // This is set as the R key for now, to allow a fast reset if stuck
        {
            SceneManager.LoadScene(0);
        }

        if (timerRunning == true) 
        {
            elapsedTime += Time.deltaTime; // Just a normal timer for the scorescreen until it's made false
        }

        if (scoreScreen.activeInHierarchy) // Score Screen stops time count and displays
        {
            timerRunning = false; // Stops Timer

            if (timerRunning == false)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}"; // Displays minutes and seconds instead of just say the time in only seconds, appears like normal time
            }

            return;
        }
    }

    //-----------------------------------Healing-------------------------
    public void Potion()
    {
        if (currentPotion > minimumPotion && currentHealth < maximumHealth)
        {
            int healAmount = Random.Range(1, 20);
            currentHealth = Mathf.Min(currentHealth + healAmount, maximumHealth);
            currentPotion -= 1;
            updateTextHealth.text = "+" + healAmount.ToString() + " Vitality";
            UpdateHealthBar();
            UpdatePotionAmountText();
        }
    }

    //-----------------------------------Player Movement----------------------------
    public void forward()
    {
        if (enemy.activeSelf)
            return; // Stops player from moving and causing issues

        chatLogUpdates.text = "You moved forward";

        float roll = Random.value; // Picks random amount
        if (roll < spawnChance) // Uses said amount to determine whether enemy should 'spawn'
        {
            enemy.SetActive(true);
        }

        if (roll < 0.2 && defeatedCounter > 4) // So the player cannot win straight away
        {
            playerHUD.SetActive(false);

            scoreScreen.SetActive(true);

            scoreScreenAliveTitle.SetActive(true); 
        }
    }
    public void rotateRight() // Both right and left do not do anything as of now besides inform player
    {
        if (enemy.activeSelf)
            return;

        chatLogUpdates.text = "You rotated right";
    }

    public void rotateLeft()
    {
        if (enemy.activeSelf)
            return;

        chatLogUpdates.text = "You rotated left";
    }

    //-----------------------------------Attacking----------------------------
    public void playerAttack()
    {
        if (!(enemy?.activeInHierarchy ?? false))
        {
            return; // Stops player from attacking if an enemy is not on screen
        }

        if (enemy.activeInHierarchy)
        {
            enemyText.gameObject.SetActive(true);
            currentDamage = Random.Range(2, 18);
            enemyCurrentHealth -= currentDamage;
            enemyText.text = "You attacked for " + currentDamage.ToString() + " damage";
            enemyHealthBar.value = enemyCurrentHealth;

            checkCurrentHealthEnemy();
            enemyHitAnimation.SetTrigger("Hit"); // Small Animation to visually represent Attack
        }

        enemyAttack();
    }

    //-----------------------------------Player's Death----------------------------
    void checkCurrentHealth()
    {
        if (currentHealth <= 0)
        {
            playerHUD.SetActive(false); // Hides hud

            scoreScreen.SetActive(true);

            scoreScreenDeathTitle.SetActive(true); // Changes title, default is off for both titles
        }
    }

    //-----------------------------------Enemy Health----------------------------

    void checkCurrentHealthEnemy() // Hides sprite to visually represent defeating enemy
    {
        if (enemyCurrentHealth <= 0)
        {
            enemy.SetActive(false);
            enemyText.gameObject.SetActive(true);
            defeatedCounter += 1; // Counter for the score screen
            defeatedText.text = defeatedCounter.ToString();

            currentPotion += Random.Range(1, 6); // Rewards player with potion/s

            Start();
        }
    }

    //-----------------------------------Enemy Attack----------------------------
    void enemyAttack()
    {
        currentDamage = Random.Range(2, 18); // Picks a number in-between for the damage amount 

        currentHealth -= currentDamage; // Simply takes the damage amount off the player's health counter

        updateTextHealth.text = currentDamage.ToString() + " Damage taken"; // Informs player how much damage they've taken
    }

    //-----------------------------------Keeps Health and Potion Correctly Displayed-------------------------
    void UpdateHealthBar()
    {
        healthSlider.value = currentHealth;
    }

    void UpdatePotionAmountText()
    {
        potionAmountText.text = currentPotion.ToString(); // int to string
    }
}