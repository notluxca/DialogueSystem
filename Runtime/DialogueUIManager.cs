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
            //* refatorar
            //*  Quando o new actor for diferente do actor default (player)
            // if (newActor != dialogueGroupSelector.ActorsOnDialogue[0])
            // {
            //     // Debug.Log(dialogueGroupSelector.ActorsOnDialogue[1]);
            //     // SetOnRightGroup(listeningCharacter, newActor, currentDialogue.speechAnimation, listenerNameText);
            // }
            // else
            // {
            //     SetCharacterState(talkingCharacter, newActor, currentDialogue.speechAnimation, characterNameText); //* O primeiro character a falar nesse caso esta sendo considerado o player, que fica ao lado esquerdo
            // }
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


        //* REFATORAR ---------------------
        // Unificação de SetCharacterForTalking e SetCharacterForListening
        // private void SetCharacterState(Animator characterAnimator, DSActor actor, string animation, TMP_Text nameText)
        // {

        //     talkingCharacter.gameObject.GetComponent<DialogueActor>().InitializeActor(actor, animation);
        // }

        // private void SetOnRightGroup(Animator characterAnimator, DSActor actor, string animation, TMP_Text nameText)
        // {
        //     nameText.SetText(actor.ToString());
        //     dialogueText.alignment = TextAlignmentOptions.Right;
        //     PlayPopAnimation(nameText);


        //     // // se o ator não está no grupo adicione ele no grupo
        //     // if (!actors.Contains(actor))
        //     // {
        //     //     actors.Add(actor);
        //     // }
        // }
        //* Refatorar ---------------------
        void InitializeActors()
        {
            Debug.Log("cheguei no initialize actors");
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
            // if (currentDialogue != null)
            // {
            //     talkingCharacter.gameObject.GetComponent<DialogueActor>().InitializeActor(currentDialogue.Actor, currentDialogue.speechAnimation.ToString());
            // }

            // if (dialogueGroupSelector.ActorsOnDialogue.Count >= 1)
            // {
            //     string firstListener = dialogueGroupSelector.ActorsOnDialogue[1].ToString();
            //     // Debug.Log($"First talking {dialogueGroupSelector.firstTalking}");
            // }
        }

        private void FinishDialogue()
        {
            dialogueUI.SetActive(false);
            isDialogueHappening = false;
            actors.Clear();
            dialogueText.text = "";
            listenerNameText.text = "";
            characterNameText.text = "";
            // currentDialogue = currentDialogue.;
            foreach (Transform child in rightGroup.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }
            OnDialogueEnd.Invoke();

        }


    }
}
