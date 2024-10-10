using TMPro;
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;


//* Receber informações de dialogo <- 
//* atores recebem as informações de dialogo. //! não acontece

namespace DialogueSystem
{
    using System.Collections;
    using System.Text;
    using DialogueSystem.Enumerations;
    using ScriptableObjects;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(DialogueGroupSelector))]
    public class DialogueUIManager : MonoBehaviour
    {
        private bool isDialogueHappening = false;

        [Header("References")]
        [SerializeField] public CharacterDialogueAnimations characterAnimations;
        [SerializeField] private GameObject dialogueUI;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text listenerNameText;
        [SerializeField] private CanvasGroup canvasGroup;
        private DialogueGroupSelector dialogueGroupSelector;
        private DSDialogueSO currentDialogue;

        [Header("Dialogue Animation Settings")]
        [SerializeField] private float popDuration = 0.2f;
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] InputActionReference inputActionReference;


        [Header("Events")]
        [SerializeField] UnityEvent OnDialogueStart;
        [SerializeField] UnityEvent OnDialogueEnds;



        public static event Action<DSActor, string> OnDialogueChanged = delegate { }; // evento pra disparar toda vez que o dialogo muda
        // public event Action OnDialogueStart = delegate { };
        // public event Action OnDialogueEnd = delegate { };

        public List<DSActor> actors = new List<DSActor>(); // Inicializa uma lista vazia
        private Vector3 originalScale = new Vector3(1.4f, 1.4f, 1); // modificar para ser dinamico

        StringBuilder dialogueStringBuilder;
        private bool textAnimationHappening;
        private float originalTextSpeed;
        public float textSpeed;
        public float speedBoost;




        //* iniciar o primeiro current dialogue
        private void Start()
        {
            originalTextSpeed = textSpeed;
            // characterAnimations = DSIOUtility.LoadAsset<CharacterDialogueAnimations>("Assets/Plugins/DialogueResources", "CharactersDialogueAnimations");
            dialogueUI.SetActive(false);
            dialogueGroupSelector = GetComponent<DialogueGroupSelector>();
            if (currentDialogue == null) currentDialogue = dialogueGroupSelector.targetDialogue;

            inputActionReference.action.actionMap.Enable();

        }

        private void OnEnable()
        {
            // inscrever input 
            if (dialogueUI.activeSelf && currentDialogue != null)
            {
                InitializeDialogueUI(currentDialogue);
            }

            inputActionReference.action.performed += InputDetected;
        }

        void OnDisable()
        {
            inputActionReference.action.performed -= InputDetected;
        }

        private void InputDetected(InputAction.CallbackContext context)
        {

            if (isDialogueHappening && context.action.WasPressedThisFrame())
            {
                if (currentDialogue.Choices[0].NextDialogue == null)
                {
                    FinishDialogue();
                    return;
                }
                if (!textAnimationHappening) UpdateDialogue();
                else
                {
                    textSpeed -= speedBoost;
                    // StopAllCoroutines();
                    // dialogueText.SetText(currentDialogue.RequestText);
                    // textAnimationHappening = false;
                    // UpdateDialogue();
                }

            }
        }

        public void StartDialogue(DialogueGroupSelector dialogue)
        {
            actors = dialogue.ActorsOnDialogue;
            dialogueUI.SetActive(true);
            isDialogueHappening = true;
            currentDialogue = dialogue.targetDialogue;
            InitializeDialogueUI(currentDialogue);
            // OnDialogueStart.Invoke();
        }


        //* REFACTOR: um de um lado um do outro
        private void UpdateDialogue()
        {
            currentDialogue = currentDialogue.Choices[0].NextDialogue;
            StartCoroutine(PrintCharsMessage());
            // dialogueText.SetText(currentDialogue.RequestText);
            DSActor newActor = currentDialogue.Actor; // passar o actor

            if (IsActorOnLeftSide(newActor))
            {
                // Atualiza o nome no lado esquerdo
                characterNameText.SetText(newActor.ToString());
                dialogueText.alignment = TextAlignmentOptions.Center;
                PlayPopAnimation(characterNameText);
            }
            else
            {
                // Atualiza o nome no lado direito
                listenerNameText.SetText(newActor.ToString());
                dialogueText.alignment = TextAlignmentOptions.Center;
                PlayPopAnimation(listenerNameText);
            }

            // characterNameText.SetText(newActor.ToString());
            // PlayPopAnimation(characterNameText);




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

            var DialogueActors = GetComponentsInChildren<DialogueActor>();
            // Define duas arrays e o primeiro e segundo personagem no dialogo vão para a array leftSide
            DialogueActor[] leftSideActors = { DialogueActors[0], DialogueActors[1] }; // Dois primeiros para a esquerda
            DialogueActor[] rightSideActors = { DialogueActors[2], DialogueActors[3] }; // Dois últimos para a direita

            bool isLeftSide = true;

            //* não é uinicializado na segunda vez
            for (int i = 0; i < actors.Count; i++)
            {
                if (isLeftSide && leftSideActors.Length > 0)
                {
                    // Inicializa no lado esquerdo
                    leftSideActors[i / 2].InitializeActor(actors[i], characterAnimations);

                }
                else if (!isLeftSide && rightSideActors.Length > 0)
                {
                    // Inicializa no lado direito
                    rightSideActors[i / 2].InitializeActor(actors[i], characterAnimations);

                }

                // Alterna o lado
                isLeftSide = !isLeftSide;
            }



            OnDialogueChanged?.Invoke(currentDialogue.Actor, currentDialogue.speechAnimation);

        }


        private void InitializeDialogueUI(DSDialogueSO currentDialogue)
        {
            InitializeActors();

            // Definir o primeiro diálogo diretamente
            // dialogueText.SetText(currentDialogue.RequestText);
            StartCoroutine(PrintCharsMessage());

            DSActor newActor = currentDialogue.Actor; // passar o ator

            if (IsActorOnLeftSide(newActor))
            {
                // Atualiza o nome no lado esquerdo
                characterNameText.SetText(newActor.ToString());
                dialogueText.alignment = TextAlignmentOptions.Center;
                PlayPopAnimation(characterNameText);
            }
            else
            {
                // Atualiza o nome no lado direito
                listenerNameText.SetText(newActor.ToString());
                dialogueText.alignment = TextAlignmentOptions.Center;
                PlayPopAnimation(listenerNameText);
            }

            // Disparar o evento OnDialogueChanged com o ator e a animação de fala
            OnDialogueChanged?.Invoke(newActor, currentDialogue.speechAnimation);

            PlayPopAnimation(characterNameText);
        }

        private IEnumerator PrintCharsMessage()
        {
            textAnimationHappening = true;
            string dialogMessage = currentDialogue.RequestText;
            // dialogueText = currentDialogue.RequestText;
            dialogueText.SetText(string.Empty);
            dialogueStringBuilder = new StringBuilder();
            for (int i = 0; i < dialogMessage.Length; i++)
            {
                dialogueStringBuilder.Append(dialogMessage[i]);
                dialogueText.SetText(dialogueStringBuilder);
                yield return new WaitForSeconds(textSpeed);
            }

            textAnimationHappening = false;
            textSpeed = originalTextSpeed;

        }

        private bool IsActorOnLeftSide(DSActor actor)
        {
            return actors.IndexOf(actor) % 2 == 0;
        }
        private void FinishDialogue()
        {
            // Resetar variáveis e estados
            dialogueUI.SetActive(false);
            isDialogueHappening = false;

            // Limpar o diálogo atual e resetar os textos
            currentDialogue = null;
            dialogueText.text = "";
            listenerNameText.text = "";
            characterNameText.text = "";
            StopAllCoroutines();

            // OnDialogueEnd.Invoke(); // Invocar o evento de finalização do diálogo
        }


    }
}
