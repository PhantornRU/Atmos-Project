using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] frameArray;
    public int currentFrame;
    public float timer;
    public float framerate = .1f;
    private SpriteRenderer spriteRenderer;
    //public bool isLoop = true;
    public bool isDeleteAfterAnimate = true;



    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frameArray[0];
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= framerate)
        {
            timer -= framerate;

            currentFrame = (currentFrame + 1) % frameArray.Length;

            //Debug.Log($"Новый спрайт {currentFrame}, {frameArray[currentFrame]}");
            spriteRenderer.sprite = frameArray[currentFrame];

            if (isDeleteAfterAnimate && currentFrame == frameArray.Length - 1)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
