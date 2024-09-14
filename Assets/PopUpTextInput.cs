using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PopUpTextInput : MonoBehaviour {

    public Text messageLabel;
    public InputField inputField;
    public Button submitButton;

    private TaskCompletionSource<string> taskCompletionSource;

    // This is the prefab for the PopUp, ensure it is assigned in the inspector
    private static PopUpTextInput instance;

    void Awake() {
        instance = this;
        gameObject.SetActive(false); // Hide initially
    }

    // Method to display the popup and wait for user input
    public static Task<string> AskForInput(string message = "Please enter your input:") {
        instance.gameObject.SetActive(true); // Show the popup
        instance.messageLabel.text = message; // Set the message
        instance.inputField.text = ""; // Clear previous input
        instance.inputField.ActivateInputField(); // Focus the input field

        // Set up TaskCompletionSource to wait for the result
        instance.taskCompletionSource = new TaskCompletionSource<string>();

        // Return the task which will complete when the user submits
        return instance.taskCompletionSource.Task;
    }

    // Method called when Submit button is clicked
    public void OnSubmitButtonClicked() {
        string userInput = inputField.text; // Get the input value

        // Complete the task with the user's input and close the popup
        taskCompletionSource.SetResult(userInput);
        gameObject.SetActive(false); // Hide the popup
    }

    public void OnCancelButtonClicked() {
        taskCompletionSource.SetResult(null); // Return null if cancelled
        gameObject.SetActive(false); // Hide the popup
    }
}