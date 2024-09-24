using System.Collections.Generic;
using UnityEngine;


namespace DialogueSystem
{
    using DialogueSystem.Enumerations;
    using ScriptableObjects;
    public class DialogueActor : MonoBehaviour
    {
        public CharacterDialogueAnimations characterDialogueAnimations; // modificar para receber o speech animation diretamente do graphview como animation clip 
        public Animator animator;
        public CanvasGroup canvasGroup;



        public DSActor Actor;
        public bool Active;

        void Start()
        {
            Active = false;
            animator = GetComponent<Animator>();
            canvasGroup.alpha = 0;
        }

        void OnEnable()
        {
            // if (dialogueUIManager == null) Debug.Log("dialogueisNull");
            DialogueUIManager.OnDialogueChanged += OnDialogueChange;
        }

        void OnDialogueChange(DSActor Actor, string SpeechAnimation)
        {
            if (!Active && this.Actor == Actor)
            {
                Active = true;
                canvasGroup.alpha = 1;
                SetAnimation(SpeechAnimation);

            }


            if (this.Actor == Actor)
            {
                SetAnimation(SpeechAnimation);
            }
            else
            {
                SetAnimation("listening");
            }

        }

        public void SetAnimation(string animation)
        {
            AnimationClip animationClip = characterDialogueAnimations.GetAnimationClip(this.Actor, animation);
            Debug.Log($"{animationClip.name} on {Actor}");
            ChangeAnimation(animationClip);

        }

        public void InitializeActor(DSActor Actor, string animation)
        {
            Active = true;
            this.Actor = Actor;
            SetAnimation(animation);
        }

        void ChangeAnimation(AnimationClip newClip)
        {
            if (newClip == null)
            {
                Debug.LogError("New animation clip is null!");
                return;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Animator runtime controller is null!");
                return;
            }

            var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

            if (overrideController.runtimeAnimatorController.animationClips.Length == 0)
            {
                Debug.LogError("No animation clips found in the animator controller!");
                return;
            }

            var currentClip = overrideController.runtimeAnimatorController.animationClips[0];
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(currentClip, newClip)
            };

            overrideController.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = overrideController;
            animator.Play("default", 0, 0);
        }

    }

}
