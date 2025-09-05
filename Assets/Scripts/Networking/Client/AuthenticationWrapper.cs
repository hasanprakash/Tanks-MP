using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState CurrentAuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries)
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

        await SignInAnonymouslyAsync(maxTries);

        return CurrentAuthState;
    }

    public static async Task SignInAnonymouslyAsync(int maxTries = 5)
    {
        int tries = 0;
        CurrentAuthState = AuthState.Authenticating;

        while (CurrentAuthState != AuthState.Authenticated && tries < maxTries)
        {
            tries++;
            Debug.Log($"Attempting authentication, try {tries}/{maxTries}...");

            try
            {
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
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Authentication failed: {ex.Message}");
                CurrentAuthState = AuthState.Error;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Request failed: {ex.Message}");
                CurrentAuthState = AuthState.Error;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unexpected error during authentication: {ex.Message}");
                CurrentAuthState = AuthState.Error;
            }

            await Task.Delay(1000); // Wait for 1 second before retrying
        }
        
        if (CurrentAuthState != AuthState.Authenticated)
        {
            Debug.LogError("Authentication failed after maximum retries.");
            CurrentAuthState = AuthState.Timeout;
        }
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