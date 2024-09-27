using TMPro;
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;


//* Receber informações de dialogo <- 
//* atores recebem as informações de dialogo. //! não acontece

namespace DialogueSystem
{
    using DialogueSystem.Enumerations;
    using ScriptableObjects;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(DialogueGroupSelector))]
    public class DialogueUIManager : MonoBehaviour
    {
        private bool isDialogueHappening = false;

        [Header("References")]
        [SerializeField] private CharacterDialogueAnimations characterAnimations;
        [SerializeField] private GameObject dialogueUI;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text listenerNameText;
        [SerializeField] private CanvasGroup canvasGroup;
        private DialogueGroupSelector dialogueGroupSelector;
        private DSDialogueSO currentDialogue;

        [Header("Dialogue Animation Settings")]
        [SerializeField] private Animator talkingCharacter;
        [SerializeField] private Animator listeningCharacter;
        [SerializeField] private float popDuration = 0.2f;
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] InputActionReference inputActionReference;


        public static event Action<DSActor, string> OnDialogueChanged = delegate { }; // evento pra disparar toda vez que o dialogo muda
        public event Action OnDialogueStart = delegate { };
        public event Action OnDialogueEnd = delegate { };

        public GameObject rightGroup;
        public List<DSActor> actors = new List<DSActor>(); // Inicializa uma lista vazia
        private Vector3 originalScale = new Vector3(1.4f, 1.4f, 1); // modificar para ser dinamico



        //* iniciar o primeiro current dialogue
        private void Start()
        {
            // characterAnimations = DSIOUtility.LoadAsset<CharacterDialogueAnimations>("Assets/Plugins/DialogueResources", "CharactersDialogueAnimations");
            dialogueUI.SetActive(false);
            dialogueGroupSelector = GetComponent<DialogueGroupSelector>();
            if (currentDialogue == null) currentDialogue = dialogueGroupSelector.targetDialogue;

        }

        private void OnEnable()
        {
            // inscrever input 
            if (dialogueUI.activeSelf && currentDialogue != null)
            {
                InitializeDialogueUI(currentDialogue);
            }
        }

        public void StartDialogue(DialogueGroupSelector dialogue)
        {
            actors = dialogue.ActorsOnDialogue;
            dialogueUI.SetActive(true);
            isDialogueHappening = true;
            currentDialogue = dialogue.targetDialogue;
            InitializeDialogueUI(currentDialogue);
            OnDialogueStart.Invoke();
        }

        private void Update()
        {
            InputDetected();
        }


        //* REFATORAR: switch to new input system
        // inputActionReference.action.WasPressedThisFrame();
        private void InputDetected()
        {
            if (isDialogueHappening && Input.anyKeyDown)
            {
                if (currentDialogue.Choices[0].NextDialogue == null)
                {
                    FinishDialogue();
                    return;
                }
                UpdateDialogue();

            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 50, 50), "Iniciar diálogo"))
            {
                StartDialogue(dialogueGroupSelector);
            }
        }



        private void UpdateDialogue()
        {
            currentDialogue = currentDialogue.Choices[0].NextDialogue;
            dialogueText.SetText(currentDialogue.RequestText);
            DSActor newActor = currentDialogue.Actor; // passar o actor
            characterNameText.SetText(newActor.ToString());
            PlayPopAnimation(characterNameText);
            dialogueText.alignment = TextAlignmentOptions.Left;
            OnDialogueChanged?.Invoke(newActor, currentDialogue.speechAnimation);
        }



        private void PlayPopAnimation(TMP_Text text)
        {
            text.transform.DOScale(popScale, popDuration / 2).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    text.transform.DOScale(originalScale, popDuration / 2).SetEase(Ease.InQuad);
                });
        }



        void InitializeActors()
        {
            // Debug.Log("cheguei no initialize actors");
            var DialogueActors = GetComponentsInChildren<DialogueActor>();
            for (int i = 0; i < actors.Count; i++)
            {

                DialogueActors[i].InitializeActor(actors[i], characterAnimations);
            }

        }


        private void InitializeDialogueUI(DSDialogueSO currentDialogue)
        {
            //* updating text
            dialogueText.text = currentDialogue.RequestText;
            characterNameText.text = currentDialogue.Actor.ToString();
            PlayPopAnimation(characterNameText);


            //* set first actor
            InitializeActors();
            OnDialogueChanged?.Invoke(currentDialogue.Actor, currentDialogue.speechAnimation);
        }

        private void FinishDialogue()
        {
            dialogueUI.SetActive(false);
            isDialogueHappening = false;
            actors.Clear();
            dialogueText.text = "";
            listenerNameText.text = "";
            characterNameText.text = "";
            OnDialogueEnd.Invoke();

        }


    }
}
