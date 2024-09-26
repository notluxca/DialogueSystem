using System.Collections.Generic;
using UnityEngine;


namespace DialogueSystem
{
    using DialogueSystem.Enumerations;
    using ScriptableObjects;
    public class DialogueActor : MonoBehaviour
    {
        CharacterDialogueAnimations characterAnimations; // modificar para receber o speech animation diretamente do graphview como animation clip 
        private Animator animator;
        private CanvasGroup canvasGroup;
        public AnimationClip currentAnimationClip = null;


        [SerializeField] private DSActor actor;
        private bool active = false;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            animator = GetComponent<Animator>();
            canvasGroup.alpha = 0;
        }

        void OnEnable()
        {
            DialogueUIManager.OnDialogueChanged += OnDialogueChange;
        }

        void OnDialogueChange(DSActor Actor, string SpeechAnimation)
        {

            if (!active && this.actor == Actor)
            {
                active = true;
                canvasGroup.alpha = 1;
                SetAnimation(SpeechAnimation);
                // return;
            }
            if (this.actor == Actor)
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
            AnimationClip animationClip = characterAnimations.GetAnimationClip(this.actor, animation);
            Debug.Log($"{animationClip.name} on {actor}");
            currentAnimationClip = animationClip;
            ChangeCurrentAnimation(animationClip);

        }

        public void InitializeActor(DSActor Actor, CharacterDialogueAnimations characterAnimations)
        {
            //Debug.Log("Actor tried to initialize");
            this.active = true;
            this.characterAnimations = characterAnimations;
            canvasGroup.alpha = 1;
            active = true;
            this.actor = Actor;
            SetAnimation("listening");
        }

        void ChangeCurrentAnimation(AnimationClip newClip)
        {
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

        void OnDisable()
        {
            DialogueUIManager.OnDialogueChanged -= OnDialogueChange;
        }

    }

}
