using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState CurrentAuthState { get; private set; } = AuthState.NotAuthenticated;

    public async static Task<AuthState> DoAuth(int totalTries = 5)
    {
        if (AuthState.Authenticated == CurrentAuthState)
        {
            Debug.LogWarning("Already authenticated.");
            return CurrentAuthState;
        }
        else if (AuthState.Authenticating == CurrentAuthState)
        {
            Debug.LogWarning("Authentication is already in progress.");
            return CurrentAuthState;
        }

        CurrentAuthState = AuthState.Authenticating;

        int tries = 0;
        while (CurrentAuthState != AuthState.Authenticated && tries < totalTries)
        {
            tries++;
            Debug.Log($"Attempting authentication, try {tries}/{totalTries}...");

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            bool isAuthenticated = AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized;

            if (!isAuthenticated)
            {
                Debug.LogError("Authentication failed. Retrying...");
                CurrentAuthState = AuthState.Error;
            }
            else
            {
                CurrentAuthState = AuthState.Authenticated;
                Debug.Log("Authentication successful.");
                return CurrentAuthState;
            }
            
            await Task.Delay(1000); // Wait for 1 second before retrying
        }

        return CurrentAuthState;
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticated,
    Authenticating,
    Error,
    Timeout
}