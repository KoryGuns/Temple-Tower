﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Movement : MonoBehaviour
{
    private Animator anim;
    public Animator frontMiles;
    public Animator whipAnim; //this is connected to the whip which has the animation WhipExtend on it to play the whip animation
    private AudioSource audioData;
    public AudioClip[] audioClipArray;
    float defaultScale; 
    public float speed;
    public float rollSpeed;
    public float rollLength;
    public float jumpWaitTime;
    public float fallDelayTime;
    public float jumpForce = 6;
    //public bool isJumping = false;
    public bool isDead = false;
    public bool isPaused = false;
    public bool isGrounded;
    public bool isBackTurned = false;
    public bool isRolling = false;
    public bool justJumped = false;
    public bool notMoving = true;
    public bool isWhipping = false;
    public GameObject bloodSpawn;
    public GameObject[] MilesSprites;
    public SpriteRenderer whip;
    public SpriteRenderer MilesFrontWalk;
    public GameObject gameOverScreen;
    private Rigidbody rb;

    public int score;
    public GameObject scoreText;
    private TextMeshProUGUI text;
    private int greenValue = 5;
    private int goldValue = 10;
    private int silverValue = 25;
    private int isGreen = 0;
    private int isGold = 0;
    private int isSilver = 0;
    private int counter = 0;
    private bool playedOnce =false;

    void Awake()
    {
        audioData = GetComponent<AudioSource>();
    }
    void Start()
    {
        anim = GetComponent<Animator>();  
        rb = GetComponent<Rigidbody>();
        text = scoreText.GetComponent<TextMeshProUGUI>();
        isGrounded = false;
        defaultScale = transform.localScale.x; // assuming this is facing right    
    }

    void Update()
    {
        Time.timeScale = 1;
        if (isPaused)
        {
            //pause menu goes here
        }
        else if (isDead)
        {
            SceneManager.LoadScene("DeathScene");
        }
        else if (isWhipping && playedOnce == false)
        {
            //play whipping animation on Miles and the Whip
            anim.Play("MilesWhipExtend"); //player whipping animation
            whipAnim.Play("WhipExtend"); //whip estending animation
            playedOnce = true; //temp variable to make the animation play once and not loop on Update();
        }
        else if (!isWhipping)
        {
            // moves character
            if (Input.GetAxis("Horizontal") != 0)
            {
                notMoving = false;
            }
            else
            {
                notMoving = true;
            }
            if (MilesFrontWalk)
            {
                TurnOffFrontWalk();
            }    
            transform.Translate(Input.GetAxis("Horizontal") * speed * Time.deltaTime, 0f, Input.GetAxis("Vertical") * speed * Time.deltaTime);

            if (Input.GetMouseButton(0))
            {
                isWhipping = true;
            } //press mouse button to whip
        
                


            if (Input.GetAxis("Horizontal") > 0)
            {
                //player is going right
                isBackTurned = false;
                transform.localScale = new Vector3(defaultScale, transform.localScale.y, transform.localScale.z);
                whip.sortingOrder = 10;
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                //player is going left
                isBackTurned = false;
                transform.localScale = new Vector3(-defaultScale, transform.localScale.y, transform.localScale.z);
                whip.sortingOrder = 0;
            }
            //character moving toward camera
            if (Input.GetAxis("Vertical") < 0)
            {
                if (Input.GetAxis("Horizontal") == 0)
                {
                    foreach (GameObject sprites in MilesSprites)
                    {
                        sprites.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    MilesFrontWalk.enabled = true;
                    if (!justJumped && isGrounded)
                    {
                        frontMiles.Play("MilesFrontRunCycle");
                    }
                    isBackTurned = false;
                }
            }
            //character moving away from camera
            if (Input.GetAxis("Vertical") > 0)
            {
                if (Input.GetAxis("Horizontal") == 0)
                {
                    foreach (GameObject sprites in MilesSprites)
                    {
                        sprites.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    MilesFrontWalk.enabled = true;
                    if (!justJumped && isGrounded)
                    {
                        frontMiles.Play("MilesBackRunCycle");
                    }
                    isBackTurned = true;
                }
            }
            // roll
            if (isGrounded && !notMoving && (Input.GetKeyDown("left shift") || Input.GetButtonDown("Fire3")))
            {
                speed += rollSpeed;
                isRolling = true;
                RollAnimation();
                StartCoroutine(RollBack());
            }

            // applies force vertically if the space key is pressed
            if (isGrounded && (Input.GetKeyDown("space") || Input.GetButton("Fire1")))
            {
                if (justJumped == false)
                {
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                    justJumped = true;
                    JumpAnimation();
                    PlayerJumpSound();
                    StartCoroutine(JumpReset());
                }
            }
        }

        // coins
        if (isGreen > 0)
        {
            if (counter != greenValue)
            {
                score += 1;
                ScoreDisplay();
                text.text += score;
                counter++;

                if (counter == greenValue)
                {
                    counter = 0;
                    isGreen--;
                }
            }
        }

        if (isGold > 0)
        {
            if (counter != goldValue)
            {
                score += 1;
                ScoreDisplay();
                text.text += score;
                counter++;

                if (counter == goldValue)
                {
                    counter = 0;
                    isGold--;
                }
            }
        }

        if (isSilver > 0)
        {
            if (counter != silverValue)
            {
                score += 1;
                ScoreDisplay();
                text.text += score;
                counter++;

                if (counter == silverValue)
                {
                    counter = 0;
                    isSilver--;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "green")
        {
            isGreen++;
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "gold")
        {
            isGold++;
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "silver")
        {
            isSilver++;
            Destroy(collision.gameObject);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            
            if (notMoving == true && isGrounded == true && justJumped == false && isRolling == false)
                IdleAnimation();
            else if (notMoving == false && isGrounded == true && justJumped == false && isRolling == false)
                RunAnimation();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        if (isGrounded == false && justJumped == false && isRolling == false)
        {
            StartCoroutine(FallDelay());            
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Trap")            
        {
            Instantiate (bloodSpawn, other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position), this.transform.rotation);
        }
    }

    public void ScoreDisplay()
    {
        text.text = "0";
        for (int i = 0; i <= 5 - score.ToString().Length; i++)
        {
            text.text = text.text + '0';
        }
    }

    public void IdleAnimation()
    {
        anim.Play("MilesIdle"); 
    } 
    public void RunAnimation()
    {
        anim.Play("RunCycle"); 
    } 
    public void RollAnimation()
    {
        anim.Play("DodgeRoll"); 
    }  
    public void JumpAnimation()
    {
        anim.Play("MilesJump3");
        if (isBackTurned)
        {
            frontMiles.Play("MilesBackJump");
        }
        else
        {
            frontMiles.Play("MilesFrontJump");
        } 
    }
    
    public void FallAnimation()
    {
        anim.Play("MilesFallLoop"); 
    }
    
    public void TurnOffFrontWalk()
    {
        foreach (GameObject sprites in MilesSprites)
        {
            sprites.GetComponent<SpriteRenderer>().enabled = true; 
        }
        MilesFrontWalk.enabled = false;
    } 
    
    public void PlayerHurtSound()
    {
        audioData.clip=audioClipArray[Random.Range(0,2)];
        //audioData.Stop();
        audioData.PlayOneShot(audioData.clip);
    }

    public void PlayerJumpSound()
    {
        audioData.clip=audioClipArray[Random.Range(3,6)];
        //audioData.Stop();
        audioData.PlayOneShot(audioData.clip);
    }

    IEnumerator RollBack()
    {
        yield return new WaitForSeconds(rollLength);
        speed -= rollSpeed;
        isRolling = false;
    }

    IEnumerator JumpReset()
    {
        yield return new WaitForSeconds(jumpWaitTime);
        justJumped = false;
    }

    IEnumerator FallDelay()
    {
        yield return new WaitForSeconds(fallDelayTime);
        
        if (isGrounded == false && justJumped == false && isRolling ==false)
        {
            FallAnimation();                   
        }
    }            
}

