using System.IO;
using Newtonsoft.Json;


namespace HCL_ODA_TestPAD.Settings;

public class SettingsSerializer<T> where T : new()
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="fileName"></param>
    public void Save(string fileName)
    {
        File.WriteAllText(fileName, JsonConvert.SerializeObject(this));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="pSettings"></param>
    /// <param name="fileName"></param>
    public static void Save(T pSettings, string fileName)
    {
        File.WriteAllText(fileName, JsonConvert.SerializeObject(pSettings));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T Load(string fileName)
    {
        T t = default;
        if (File.Exists(fileName))
        {
            t = new T();
            t = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
        }
        return t;
    }
}