namespace WPF_App.Services
{
    public class OpcClientObject
    {
        public Dictionary<string, OpcObjectData> OpcClientData = new Dictionary<string, OpcObjectData>();

        public void Add(OpcObjectData data, bool overwrite = false)
        {
            if (OpcClientData == null)
                return;

            if (!OpcClientData.ContainsKey(data.Key) && !overwrite)
            {
                OpcClientData[data.Key] = data;
            }
        }

        public void Remove(string key)
        {
            if (OpcClientData == null)
                return;

            OpcClientData.Remove(key);
        }
    }

    public class OpcObjectData
    {
        public OpcObjectData(string key, string opcString, Type type)
        {
            Key = key;
            OpcString = opcString;
            Type = type;
        }

        public Type Type { get; private set; } = typeof(string);
        public string Description { get; private set; } = "No user description";
        public string Key { get; private set; }
        public string OpcString { get; private set; }
    }
    public interface IOpcClientConfigurator
    {
        OpcClientObject ClientDataConfig { get; }
        IOpcClientConfigurator Config();
    }
}
