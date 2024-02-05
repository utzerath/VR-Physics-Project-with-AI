using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class quizScript : MonoBehaviour
{
    public Text questionText; // Assign in Inspector
    public Text feedbackText; // Assign in Inspector
    public Button[] answerButtons; // Assign all four buttons in Inspector
    public Button nextQuestionButton; // Assign in Inspector

    private List<(string Question, string[] Options, int CorrectAnswerIndex)> questions;
    private int currentQuestionIndex = -1;
    private int score = 0;


    void Start()
    {
        // Initialize your questions here (add all your questions in this format)
        questions = new List<(string, string[], int)>
        {
            ("This is the Start of the Quiz!", new string[] {"Click Here to continue", "Dont Click Here", "Dont Click Here", "Dont Click Here"}, 0),
            ("How does a pulley system in gym equipment affect the force required to lift weights?", new string[] {"Increases the force needed", "Decreases the force needed", "Does not affect the force needed", "Changes the direction of the force"}, 1),
            ("In a pulley system with multiple pulleys, how is the force distributed?", new string[] {"Evenly across all pulleys", "Concentrated on the first pulley", "Concentrated on the last pulley", "Randomly distributed"}, 0),
            ("When using a pulley system, what happens to the total mechanical energy (potential + kinetic) assuming no friction?", new string[] {"Increases", "Decreases", "Stays the same", "First increases, then decreases"}, 2),
            ("When lifting a weight using a pulley, what happens to the work done if the rope is pulled twice as far?", new string[] {"Doubles", "Halves", "Stays the same", "Quadruples"}, 0),
            ("What is the primary advantage of using a pulley in gym equipment?", new string[] {"Increases the weight", "Decreases the weight", "Changes the direction of the applied force", "Reduces the speed of lifting"}, 2),
            ("How does a pulley system affect the speed of lifting a weight?", new string[] {"Increases speed", "Decreases speed", "Does not affect speed", "Speed varies randomly"}, 1),
            ("What factor primarily affects the efficiency of a pulley system in a gym?", new string[] {"Number of pulleys", "Weight of the pulleys", "Friction in the pulley system", "Length of the rope"}, 2),
            ("How does increasing the number of pulleys in a system affect the force needed to lift the same weight?", new string[] {"Increases the force", "Decreases the force", "Does not change the force", "The effect varies depending on the weight"}, 1),
            ("Using a pulley system, what aspect of muscle engagement can be altered?", new string[] {"Type of muscle fibers used", "Intensity of muscle contraction", "Range of motion", "Muscle group targeted"}, 2),
            ("What is the main benefit of using cable machines with pulleys in a gym?", new string[] {"Provide constant tension on the muscles", "Allow for faster exercises", "Decrease the need for supervision", "Increase the weight automatically"}, 0),
            ("When using a lat pulldown machine, what is the role of the pulley system?", new string[] {"It increases the amount of weight lifted", "It decreases the force needed to lift the same weight", "It changes the direction of the applied force", "It reduces the range of motion necessary for the exercise"}, 2),
            ("In a cable tricep pushdown machine, how does the pulley system affect the exercise?", new string[] {"Provides constant tension on the triceps throughout the motion", "Allows for varying the weight lifted during different phases of the motion", "Increases the speed of the movement", "Decreases the effectiveness of the exercise"}, 0)
            
        };


        // Prepare UI for the first question
        nextQuestionButton.onClick.AddListener(NextQuestion);
        nextQuestionButton.gameObject.SetActive(false); // Next question button is initially hidden
        feedbackText.text = ""; // Clear feedback text

        // Initialize answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Local copy for the closure below
            answerButtons[i].onClick.AddListener(() => AnswerSelected(index));
        }

        // Start with the first question
        NextQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < 12 )
        {
            SetQuestion(questions[currentQuestionIndex]);
        }
        else
        {
            // No more questions
            questionText.text = "Quiz complete! Your score: " + score + "/" + 12;
            feedbackText.text = "";
            foreach (var btn in answerButtons)
            {
                btn.gameObject.SetActive(false); // Hide answer buttons
            }
            currentQuestionIndex = -1;
            score = 0;
            NextQuestion();
        }
    }


void SetQuestion((string Question, string[] Options, int CorrectAnswerIndex) questionData)
{
    questionText.text = questionData.Question;
    for (int i = 0; i < answerButtons.Length; i++)
    {
        // Make sure buttons are active
        answerButtons[i].gameObject.SetActive(true);

        // Find the TextMeshProUGUI component in the button's children and set the option text
        TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonText != null)
        {
            buttonText.text = questionData.Options[i];
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI component found on button " + answerButtons[i].name);
        }
    }

    feedbackText.text = "";
    nextQuestionButton.gameObject.SetActive(false); // Hide next question button until an answer is selected
}



void AnswerSelected(int index)
    {
        var correctAnswerIndex = questions[currentQuestionIndex].CorrectAnswerIndex;
        if (index == correctAnswerIndex)
        {
            feedbackText.text = "Correct!";
            feedbackText.color = Color.green;
            score++;// Set feedback text to green if correct
        }
        else
        {
            feedbackText.text = "Incorrect. The correct answer is: " + questions[currentQuestionIndex].Options[correctAnswerIndex];
            feedbackText.color = Color.red; // Set feedback text to red if incorrect
        }

        foreach (var btn in answerButtons)
        {
            btn.gameObject.SetActive(false); // Hide answer buttons after a selection
        }
        if (currentQuestionIndex < questions.Count - 1)
        {
            nextQuestionButton.gameObject.SetActive(true); // Show next question button if there are more questions
        }
    }
}
