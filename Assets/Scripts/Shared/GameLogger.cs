using UnityEngine;

namespace WarGame.Shared
{
    internal static class GameLogger
    {
        // Orange for server, cyan-blue for client — both readable on Unity's dark console background
        private const string ServerColor = "#FF8C00";
        private const string ClientColor = "#4FC3F7";

        private static readonly string ServerTag = $"<color={ServerColor}><b>[Server]</b></color>";
        private static readonly string ClientTag = $"<color={ClientColor}><b>[Client]</b></color>";

        public static void Server(string message)
        {
            Debug.Log($"{ServerTag} {message}");
        }

        public static void Client(string message)
        {
            Debug.Log($"{ClientTag} {message}");
        }
    }
}