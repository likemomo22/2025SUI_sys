using UnityEditor;

[InitializeOnLoad]
internal class UnityEditorStartup
{
    static UnityEditorStartup()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(
            buildPlayerOptions =>
            {
                CreateAssetBundles.BuildAllAssetBundles();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
            }
        );
    }
}