using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class LoadFibexFileEvent : PubSubEvent<LoadFibexFileEventArgs> { }
    public class FibexFileLoadedEvent : PubSubEvent<FibexFileLoadedEventArgs> { }
    public class LoadFibexFileEventArgs
    {
        //public Dictionary<string, FibexBusCatalog> FibexDataSources { get; set; } = new Dictionary<string, FibexBusCatalog>();
    }
    public class FibexFileLoadedEventArgs
    {
        public bool IsMultipleFiles { get; set; }
    }
    public class StartLoadFibexFileEvent : PubSubEvent<StartLoadFibexFileEventArgs>
    {

    }

    public class StartLoadFibexFileEventArgs
    {
        public int NumberOfFilesLoading { get; set; }
    }
}