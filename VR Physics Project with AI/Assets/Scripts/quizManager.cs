using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public Button[] answerButtons; // Assign these in the inspector
    public Text feedbackText;      // Assign this in the inspector
    public Button nextQuestionButton; // Assign this in the inspector
    public Canvas canvas;          // Assign this in the inspector
    public Text questionText;      // Assign this in the inspector

    private int currentQuestionIndex = -1;
    private List<Question> questions = new List<Question>();
    private Question currentQuestion;

    void Start()
    {
        // Initialize your questions here
        questions = new List<Question>
        {
        new Question("What is the primary purpose of using a pulley system in gym machines such as the lat pull-down?",
                     "To redirect the force applied by the user",
                     new string[] {"To increase the weight lifted", "To decrease the weight lifted", "To redirect the force applied by the user", "To stabilize the weight"}),

        new Question("How does a pulley in a cable row machine reduce the effort needed to lift a weight?",
                     "By increasing the distance over which the force is applied",
                     new string[] {"By increasing the distance over which the force is applied", "By adding more weights to the system", "By reducing the range of motion", "By using a motor"}),

        new Question("In a lat pull-down machine, what does the pulley system allow the user to do?",
                     "Change the direction of the weight's path",
                     new string[] {"Lift their own body weight", "Lift weights more smoothly", "Change the direction of the weight's path", "None of the above"}),

        new Question("Which principle of physics is primarily utilized in a gym's pulley system to assist in lifting weights?",
                     "Mechanical Advantage",
                     new string[] {"Gravity", "Mechanical Advantage", "Friction", "Inertia"}),

        new Question("When a user pulls down on the bar of a lat pull-down machine, which part of the pulley system moves the weight plates?",
                     "The cable",
                     new string[] {"The cable", "The bar itself", "The pulley wheel", "The weight stack pin"}),

        new Question("On a cable row machine, if you adjust the pulley to a lower setting, how does it affect the exercise?",
                     "Targets different muscle groups",
                     new string[] {"Increases resistance", "Decreases resistance", "Targets different muscle groups", "Has no effect"}),

        new Question("What is a benefit of the adjustable pulley positions available on cable machines?",
                     "They allow for a variety of exercises.",
                     new string[] {"They allow for a variety of exercises.", "They make the weights heavier.", "They require less maintenance.", "They reduce the noise during workouts."}),

        new Question("Which component of the pulley system in a gym machine is responsible for ensuring the cable moves freely and without tangling?",
                     "The pulley wheel",
                     new string[] {"The cable", "The pulley wheel", "The weight stack", "The adjustment pin"}),

        new Question("In terms of biomechanics, why is it important that a pulley system maintains a constant tension on the cable during exercises?",
                     "To ensure the muscle is under consistent load",
                     new string[] {"To ensure the muscle is under consistent load", "To make the exercise easier", "To vary the type of muscle contraction", "To increase the speed of the exercise"}),

        new Question("How does using a single pulley system in a lat pull-down differ from a compound pulley system in terms of the force required from the user?",
                     "Single pulley systems require more force.",
                     new string[] {"Single pulley systems require more force.", "Single pulley systems require less force.", "Compound systems require less force.", "There is no difference in the force required."}),
    };

        // Load the first question
        NextQuestion();
    }

    public void StartQuiz()
    {
        currentQuestionIndex = -1;
        Debug.Log("StartQuiz called"); // This should print to the console when the button is clicked
                                       // Your code here
        NextQuestion();
        
    }

    public void CheckAnswer(string selectedOption)
    {
        feedbackText.text = selectedOption == currentQuestion.CorrectAnswer ? "Correct!" : "Incorrect!";
        nextQuestionButton.gameObject.SetActive(true);
        SetButtonsActive(false);
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            currentQuestion = questions[currentQuestionIndex];
            questionText.text = currentQuestion.Text;
            SetButtonsActive(true);
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.Options[i];
                int closureIndex = i; // Prevents the closure problem in loops
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => CheckAnswer(currentQuestion.Options[closureIndex]));
            }
            feedbackText.text = "";
            nextQuestionButton.gameObject.SetActive(false);
        }
        else
        {
            // Quiz is complete
            questionText.text = "Quiz Completed!";
            SetButtonsActive(false);
            nextQuestionButton.gameObject.SetActive(false);
        }
    }

    private void SetButtonsActive(bool isActive)
    {
        foreach (var button in answerButtons)
        {
            button.gameObject.SetActive(isActive);
        }
    }

    // A Question class to store the question text, correct answer, and options.
    [System.Serializable]
    public class Question
    {
        public string Text;
        public string CorrectAnswer;
        public string[] Options;

        public Question(string text, string correctAnswer, string[] options)
        {
            Text = text;
            CorrectAnswer = correctAnswer;
            Options = options;
        }
    }
}
