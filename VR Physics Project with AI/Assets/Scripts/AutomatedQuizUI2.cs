using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;


public class AutomatedQuizUI2 : MonoBehaviour
{
    public Canvas canvas; // Assign your canvas in the inspector
    public Font font; // Assign a font in the inspector

    private Text questionText;
    private Text feedbackText; // Text element to provide feedback
    private List<Button> optionButtons = new List<Button>();
    private Button submitButton;
    private Button startQuizButton; // Button to start the quiz
    private Button nextQuestionButton; // Button to load the next question
    private int selectedOption = -1;
    private int currentQuestionIndex = 0; // Index to keep track of the current question
    private List<(string Question, string[] Options, int CorrectAnswerIndex)> questionBank = new List<(string, string[], int)>
    {
        ("1. In a gym pulley system, what is the primary purpose of using pulleys?",
         new string[] { "a) To increase the weight of the load", "b) To reduce the friction between the weights", "c) To multiply the force applied", "d) To decrease the distance the weights move" },
         2), // Correct answer: c) To multiply the force applied

        ("2. When a gym pulley system is used, what happens to the amount of force required to lift a weight?",
         new string[] { "a) It increases", "b) It decreases", "c) It remains the same", "d) It becomes zero" },
         1), // Correct answer: b) It decreases

        ("3. What type of pulley system allows you to lift a load with the least amount of force?",
         new string[] { "a) Fixed pulley", "b) Movable pulley", "c) Block and tackle", "d) Inclined plane" },
         2), // Correct answer: c) Block and tackle

        ("4. In a single fixed pulley system, how does the mechanical advantage compare to the number of ropes supporting the load?",
         new string[] { "a) Equal to the number of ropes", "b) Less than the number of ropes", "c) Greater than the number of ropes", "d) Unrelated to the number of ropes" },
         0), // Correct answer: a) Equal to the number of ropes

        ("5. Which of the following is true about a block and tackle pulley system?",
         new string[] { "a) It always has a mechanical advantage of 1", "b) It always has a mechanical advantage greater than 1", "c) It always requires more force than a single fixed pulley", "d) It always involves only one rope" },
         1), // Correct answer: b) It always has a mechanical advantage greater than 1

        ("6. When you pull a rope to lift a weight in a pulley system, what is the direction of your applied force compared to the direction of the weight's movement?",
         new string[] { "a) The same direction", "b) Opposite direction", "c) Perpendicular direction", "d) Irrelevant to the pulley system" },
         0), // Correct answer: a) The same direction

        ("7. What is the advantage of using a compound pulley system in the gym?",
         new string[] { "a) It allows you to lift heavier weights with less effort", "b) It increases the speed at which you can lift weights", "c) It reduces the distance the weights move", "d) It simplifies the pulley setup" },
         0), // Correct answer: a) It allows you to lift heavier weights with less effort

        ("8. In a compound pulley system, if you have two movable pulleys, what is the mechanical advantage?",
         new string[] { "a) 1", "b) 2", "c) 3", "d) 4" },
         1), // Correct answer: b) 2

        ("9. If you wanted to double the mechanical advantage of a pulley system, how many additional pulleys would you need to add?",
         new string[] { "a) 1", "b) 2", "c) 3", "d) 4" },
         1), // Correct answer: b) 2

        ("10. Which type of pulley system is best suited for lifting a heavy load a short distance with minimal effort?",
         new string[] { "a) Fixed pulley", "b) Movable pulley", "c) Block and tackle", "d) Inclined plane" },
         2) // Correct answer: c) Block and tackle
    };


    private (string Question, string[] Options, int CorrectAnswerIndex) currentQuestion;

    void Start()
    {
        // Configure the Canvas for VR
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        // Assuming a standard canvas size for VR, but this can be adjusted as needed
        canvasRect.sizeDelta = new Vector2(2, 1); // 2 meters wide by 1 meter tall
        canvasRect.localScale = Vector3.one * 0.005f; // Scale the canvas to be appropriate for VR
        canvasRect.localPosition = new Vector3(0, 1.5f, 2); // Position in front of the player

        // Ensure the font is set, this will use Arial by default if no other font is specified
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Rest of your Start method code...
    


        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in the scene.");
                return;
            }
        }


        // Create the start quiz button
        startQuizButton = CreateButton("StartQuizButton", "Start Quiz", new Vector2(0, 0), new Vector2(400, 80));
        startQuizButton.onClick.AddListener(InitializeQuiz);


    }



    void InitializeQuiz()
    {
        // Select the first question from the question bank
        currentQuestionIndex = 0;
        LoadQuestion(currentQuestionIndex);

        // Disable the start quiz button after clicking
        startQuizButton.gameObject.SetActive(false);
    }

    void LoadQuestion(int index)
    {
        // Clear previous question and options
        if (questionText != null) Destroy(questionText.gameObject);
        foreach (var button in optionButtons) Destroy(button.gameObject);
        optionButtons.Clear();
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (nextQuestionButton != null) nextQuestionButton.gameObject.SetActive(false);

        // Select a new question from the question bank
        currentQuestion = questionBank[index];

        // Generate the quiz UI for the new question
        GenerateUI();
    }

    void GenerateUI()
    {
        // Create the question text
        questionText = CreateTextElement("QuestionText", currentQuestion.Question, new Vector2(0, 160), new Vector2(400, 35));

        // Create the answer buttons
        for (int i = 0; i < currentQuestion.Options.Length; i++)
        {
            Button optionButton = CreateButton("Option" + (i + 1), currentQuestion.Options[i], new Vector2(0, 60 - (i * 60)), new Vector2(400, 35));
            int index = i; // Local copy for the closure below
            optionButton.onClick.AddListener(() => OnOptionSelected(index));
            optionButtons.Add(optionButton);
        }

        // Create the submit button
        submitButton = CreateButton("SubmitButton", "Submit", new Vector2(0, -200), new Vector2(400, 35));
        submitButton.onClick.AddListener(OnSubmit);
        submitButton.interactable = false; // Start with the submit button disabled

        // Create the feedback text, initially empty and not visible
        feedbackText = CreateTextElement("FeedbackText", "", new Vector2(0, -250), new Vector2(400, 35));
        feedbackText.gameObject.SetActive(false); // Hide until needed

        // Create the next question button, but don't show it yet
        nextQuestionButton = CreateButton("NextQuestionButton", "Next Question", new Vector2(0, -300), new Vector2(400, 35));
        nextQuestionButton.onClick.AddListener(OnNextQuestion);
        nextQuestionButton.gameObject.SetActive(false); // Hide until needed
    }


    Text CreateTextElement(string name, string text, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(canvas.transform);
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.font = font;
        textComponent.text = text;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.black;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;

        return textComponent;
    }

    Button CreateButton(string name, string buttonText, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name, typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(canvas.transform);
        Button buttonComponent = buttonObj.GetComponent<Button>();
        buttonComponent.GetComponent<Image>().color = Color.white;

        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = size;
        btnRect.anchoredPosition = position;

        // Create and set up the button's text
        Text btnText = new GameObject("Text").AddComponent<Text>();
        btnText.transform.SetParent(buttonObj.transform);
        btnText.font = font;
        btnText.text = buttonText;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.black;

        RectTransform textRect = btnText.GetComponent<RectTransform>();
        textRect.sizeDelta = size;
        textRect.anchoredPosition = Vector2.zero;

        return buttonComponent;
    }

    void OnOptionSelected(int optionIndex)
    {
        selectedOption = optionIndex;
        submitButton.interactable = true; // Enable the submit button when an option is selected

        // Provide visual feedback for the selected option
        foreach (var button in optionButtons)
        {
            button.interactable = true;
        }
        optionButtons[optionIndex].interactable = false;
    }

    void OnSubmit()
    {
        if (selectedOption < 0)
        {
            Debug.LogError("No option selected!");
        }
        else
        {
            // Check if the selected option is the correct one
            if (selectedOption == currentQuestion.CorrectAnswerIndex)
            {
                feedbackText.text = "Correct!";
                feedbackText.color = Color.green;
            }
            else
            {
                feedbackText.text = "Incorrect. The correct answer is: " + currentQuestion.Options[currentQuestion.CorrectAnswerIndex];
                feedbackText.color = Color.red;
            }

            feedbackText.gameObject.SetActive(true); // Show feedback text
            submitButton.interactable = false; // Optionally disable the submit button after answering
        }
        // Show the next question button if there are more questions
        if (currentQuestionIndex < questionBank.Count - 1)
        {
            nextQuestionButton.gameObject.SetActive(true);
        }
    }

    void OnNextQuestion()
    {
        // Increment the question index and load the next question
        if (currentQuestionIndex < questionBank.Count - 1)
        {
            currentQuestionIndex++;
            LoadQuestion(currentQuestionIndex);
        }
        else
        {
            // Optionally, handle the end of the quiz here
            Debug.Log("End of the quiz!");
        }
    }
    // Call this method from the VR input handling script when a button is selected
    public void OnVRButtonSelected(Button selectedButton)
    {
        int optionIndex = optionButtons.IndexOf(selectedButton);
        if (optionIndex != -1)
        {
            OnOptionSelected(optionIndex);
        }
        else if (selectedButton == submitButton)
        {
            OnSubmit();
        }
        else if (selectedButton == nextQuestionButton)
        {
            OnNextQuestion();
        }
        else if (selectedButton == startQuizButton)
        {
            InitializeQuiz();
        }
    }

}

