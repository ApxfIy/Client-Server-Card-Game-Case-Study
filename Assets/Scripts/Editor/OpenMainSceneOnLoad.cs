using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class OpenMainSceneOnLoad
    {
        static OpenMainSceneOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorSceneManager.GetActiveScene().name == "Untitled")
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
                }
            };
        }
    }
}