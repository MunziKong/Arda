public enum SceneType
{
    Main,
    Forest,
    Valley,
    Dungeon
}

public static class SceneNames
{
    public static string GetSceneName(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Main:
                return "Main";

            case SceneType.Forest:
                return "Forest";

            case SceneType.Valley:
                return "Valley_Test";

            case SceneType.Dungeon:
                return "Dungeon_Test";

            default:
                return "Main";
        }
    }
}