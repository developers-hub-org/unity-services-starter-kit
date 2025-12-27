using Unity.Services.Multiplayer;
using UnityEngine;

public class ClientMigrationHandler : MonoBehaviour, IMigrationDataHandler
{

    public void Apply(byte[] migrationData)
    {
        Debug.Log("Apply");
    }

    public byte[] Generate()
    {
        Debug.Log("Generate");
        return new byte[1];
    }

    public static ClientMigrationHandler CreateInstance()
    {
        var instance = new GameObject("ClientMigrationHandler").AddComponent<ClientMigrationHandler>();
        DontDestroyOnLoad(instance.gameObject);
        return instance;
    }

}
