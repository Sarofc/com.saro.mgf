namespace Saro
{
    public static class EntitySceneFactory
    {
        public static FScene CreateScene(long id, long instanceID, int zone, SceneType sceneType, string name, FEntity parent = null)
        {
            FScene scene = new FScene(id, instanceID, zone, sceneType, name, parent);

            return scene;
        }

        public static FScene CreateScene(long id, int zone, SceneType sceneType, string name, FEntity parent = null)
        {
            FScene scene = new FScene(id, zone, sceneType, name, parent);

            return scene;
        }

        public static FScene CreateScene(int zone, SceneType sceneType, string name, FEntity parent = null)
        {
            long id = IDGenerater.Instance.GenerateID();
            FScene scene = new FScene(id, zone, sceneType, name, parent);

            return scene;
        }
    }
}