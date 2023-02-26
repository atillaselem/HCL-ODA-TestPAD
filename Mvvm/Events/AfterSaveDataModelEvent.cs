using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class AfterSaveDataModelEvent : PubSubEvent<AfterSaveDataModelEventArgs> { }
    public class AfterSaveDataModelEventArgs
    {
        public bool IsSavedSuccessfully { get; set; }
    }
    public class AfterLoadFibexFileEvent : PubSubEvent { }
}