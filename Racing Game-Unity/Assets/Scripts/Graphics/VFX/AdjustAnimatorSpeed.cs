using UnityEngine;
using System.Collections;


public class AdjustAnimatorSpeed : MonoBehaviour 
{
	public Animator animator;
	public float Speed;
    /*public bool IsLoop = true;
	public bool IsPinPon = false;*/
	//public float A_idleSpeed=0.5f;
	
	void Start () 
	{
		if (animator != null)
        {
            /*animation[Clip.name].wrapMode = IsLoop ? 
                WrapMode.Loop :
                (IsPinPon ? WrapMode.PingPong : WrapMode.Once);
            animation[Clip.name].speed = Speed;*/
			animator.speed = Speed;
        }
	}
}
