using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Central game state manager (singleton).
/// Inspector: No required fields. Attach to a persistent GameManager GameObject.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, LevelComplete }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        OnStateChanged?.Invoke(newState);
    }

    public bool IsPlaying => CurrentState == GameState.Playing;

    public void PauseGame()  => SetState(GameState.Paused);
    public void ResumeGame() => SetState(GameState.Playing);

    public void EndLevel(bool success)
    {
        SetState(success ? GameState.LevelComplete : GameState.GameOver);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
